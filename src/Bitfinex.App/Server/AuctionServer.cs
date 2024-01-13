
using Bitfinex.App.Domain;
using Bitfinex.App.Events;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;

namespace Bitfinex.App.Server
{
    internal class AuctionServer : IAuctionServer
    {
        private  HubConnection _hubconnection;
        private readonly string clientId;

        private ConcurrentDictionary<Guid, Auction> auctions = new ConcurrentDictionary<Guid, Auction>();
       

        public static async Task<IAuctionServer> ConnectAsync(string serverUrl)
        {
            var auctionServer = new AuctionServer();
            await auctionServer.ConnectToNetworkAsync(serverUrl);
            return auctionServer;
        }

        public Task ConnectToNetworkAsync(string serverUrl)
        {
            _hubconnection = new HubConnectionBuilder()
           .WithUrl($"{serverUrl}/discoveryHub")
           .Build();
            _hubconnection.On<string>("HeartbeatReceived", HandleHeartbeatReceived);
            _hubconnection.On<(TaskCompletionSource<Guid>,AuctionParams)>("InitiateAuction", HandleInitiateAuction);
            _hubconnection.On<AuctionCreated>("BroadcastInitiateAuction", HandleBroadcastInitiateAuction);
            _hubconnection.On<BidParams>("PlaceBid", HandlePlaceBid);
            _hubconnection.On<BidPlaced>("BroadcastPlaceBid", HandleBroadcastPlaceBid);
            _hubconnection.On<(TaskCompletionSource<AuctionResult>,FinalizeAuctionParams)>("CloseAuction", HandleFinalizedAuction);
            _hubconnection.On<AuctionClosed>("AuctionClosed", HandleAuctionClosed);
            return Task.CompletedTask;
        }

        //--------------------------------------------------------------------Api---------------------------------------//

        public Task<IEnumerable<Auction>> GetExistingAuctions()
        {
            return Task.FromResult(auctions.Values.AsEnumerable());
        }

        public Task<Auction> GetAuction(Guid auctionId)
        {
            if(!auctions.TryGetValue(auctionId,out Auction auction))
            {
                return null;
            }
            return Task.FromResult(auction);
        }
        public async Task<Guid> InitiateAuction(AuctionParams auctionParams)
        {
             var tcs = new TaskCompletionSource<Guid>();
             await _hubconnection.InvokeAsync("InitiateAuction", (tcs,auctionParams));
             return await tcs.Task;
        }

        public Task PlaceBidAsync(BidParams bidParams)
        {
            return _hubconnection.InvokeAsync("PlaceBid", bidParams);
        }

        public async Task<AuctionResult> FinalizeAuctionAsync(FinalizeAuctionParams @params)
        {
            var tcs = new TaskCompletionSource<AuctionResult>();
             await _hubconnection.InvokeAsync("FinalizeAuction", (tcs,@params));
            return await tcs.Task;
        }


        //---------------------------------------------------------callbacks------------------------------------------------//
        private void HandleHeartbeatReceived(string id)
        {
            _hubconnection.InvokeAsync("HeartbeatResponse", id);
        }
        private void HandleInitiateAuction((TaskCompletionSource<Guid> tcs, AuctionParams auctionParams)tuple)
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction
            {
                CreatedBy=clientId,
                AuctionId = auctionId,
                ItemName = tuple.auctionParams.ItemName,
            };
          

            if (!auctions.TryAdd(auction.AuctionId, auction))
            {
                Console.WriteLine($"Could not add auction with id : {auction.AuctionId}");
                return;
            }
            _hubconnection.InvokeAsync("BroadcastInitiateAuction", new AuctionCreated
            {
                Auction = auction,
                DateTime = DateTime.UtcNow
            });
            tuple.tcs.SetResult(auctionId);
            //how do i send the auction id 
        }
      
        private void HandleBroadcastInitiateAuction(AuctionCreated auctionCreatedParams)
        {
           if(auctions.TryGetValue(auctionCreatedParams.Auction.AuctionId,out var auction))
            {
                return;
            }
            if(!auctions.TryAdd(auctionCreatedParams.Auction.AuctionId, auctionCreatedParams.Auction))
            {
                Console.WriteLine($"Could not add new auction with id: {auctionCreatedParams.Auction.AuctionId} ");
            }
        }
        private void HandlePlaceBid(BidParams bidParams)
        {

            var date = DateTime.UtcNow;
            var newBid = new Bid
            {
                Id = Guid.NewGuid(),
                Amount = bidParams.Amount,
                Bidder = bidParams.Bidder,
                DateOfBid = date
            };
            
            if(!auctions.TryGetValue(bidParams.AuctionId,out var auction))
            {
                Console.WriteLine("Could not place bid on target auction");
                return;
            }
          
            if (!ValidateBid(auction,newBid))
            {
                Console.WriteLine("Invalid bid");
                return;
            }
            auction.Bids.Add(newBid);
            var bidPlacedEvent = new BidPlaced
            {
                AuctionId = bidParams.AuctionId,
                Bid =newBid
            };
            _hubconnection.InvokeAsync("BroadcastPlaceBid", bidPlacedEvent);
        }

        private void HandleBroadcastPlaceBid(BidPlaced bidParams)
        {
            if(!auctions.TryGetValue(bidParams.AuctionId,out Auction auction))
            {
                return;
            }
            if (auction.Bids.Any(x => x.Id == bidParams.Bid.Id))
            {
                return;
            }
            auction.Bids.Add(bidParams.Bid);
            Console.WriteLine($"Bid placed on auction:{bidParams.AuctionId} by {bidParams.Bid.Bidder} with amount:{bidParams.Bid.Amount}");

        }
        private void HandleFinalizedAuction((TaskCompletionSource<AuctionResult> tcs,FinalizeAuctionParams @params) par)
        {
            if(!auctions.TryGetValue(par.@params.AuctionId,out var auction))
            {
                par.tcs.SetException(new ArgumentNullException($"Could not find auction with id {par.@params.AuctionId}"));
                return;
            }
            if (!auction.CreatedBy.Equals(clientId))
            {
                par.tcs.SetException(new InvalidOperationException($"No closing rights for auction :${auction.AuctionId} for participant:{clientId}"));
                return;
            }
            var auctionResult = CalculateAuctionResult(auction);
            par.tcs.SetResult(auctionResult);
        }

        private void HandleAuctionClosed(AuctionClosed auctionClosed)
        {
            Console.WriteLine($"Auction with id: {auctionClosed.Result.AuctionId} was closed, with winner {auctionClosed.Result.Winner} for amount {auctionClosed.Result.WinningAmount}");
        }



        //-----------------------------------------------utils----------------------------------------------//
        private AuctionResult CalculateAuctionResult(Auction auction)
        {
            var winnerBid = auction.Bids.LastOrDefault();
            if(winnerBid is null)
            {
                return new AuctionResult
                {
                    AuctionId = auction.AuctionId,
                    Winner = "none",
                    WinningAmount = auction.StartingPrice
                };
            }
            return new AuctionResult { 
                AuctionId=auction.AuctionId,
                WinningAmount=winnerBid.Amount,
                Winner=winnerBid.Bidder
            };

        }
        private bool ValidateBid(Auction auction, Bid newBid)
        {

            if (!auction.Bids.Any())
            {
                return true;
            }
            if (auction.Bids.Last().Amount >= newBid.Amount)
            {
                return false;
            }
            return true;
        }


    }
}
