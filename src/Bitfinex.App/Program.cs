// See https://aka.ms/new-console-template for more information
using Bitfinex.App.Server;

Console.WriteLine("Hello, World!");
var url = "http://localhost:5000";
var server = await AuctionServer.ConnectAsync(url);
var auctionId=await server.InitiateAuction(new Bitfinex.App.Domain.AuctionParams
{
    ItemName = "some item",
    StartingPrice=500
});

await server.PlaceBidAsync(new Bitfinex.App.Domain.BidParams
{
    Amount = 600,
    AuctionId = auctionId

});

var result=await server.FinalizeAuctionAsync(new Bitfinex.App.Events.FinalizeAuctionParams
{
    AuctionId=auctionId
});
Console.WriteLine($"Auction with id :{result.AuctionId} ended , with the winner:{result.Winner} for the amount:{result.WinningAmount}");
