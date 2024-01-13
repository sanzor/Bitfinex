// See https://aka.ms/new-console-template for more information
using DiscoveryServer;

Console.WriteLine("Hello, World!");
var id = "discovery_server";
var discoveryServerUrl = "http://localhost:5000";
var participant = new DiscoveryServer.DiscoveryServer(discoveryServerUrl);
await participant.StartAsync(id);


