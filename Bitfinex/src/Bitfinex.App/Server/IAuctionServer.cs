using Bitfinex.App.Domain;
using Bitfinex.App.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Server
{
    internal interface IAuctionServer
    {
        public Task<IEnumerable<Auction>> GetExistingAuctions();
        public Task<Auction> GetAuction(Guid auctionId);
        public  Task<Guid> InitiateAuction(AuctionParams auctionParams);
        public Task PlaceBidAsync(BidParams bidParams);
        public Task ConnectToNetworkAsync(string serverUrl);
        public Task<AuctionResult> FinalizeAuctionAsync(FinalizeAuctionParams @params);


    }
}
