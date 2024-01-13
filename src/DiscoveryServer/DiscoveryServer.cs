
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoveryServer
{
    internal class DiscoveryServer
    {
        private readonly List<ConnectedPeer> connectedPeers = new List<ConnectedPeer>();
        private readonly TimeSpan peerExpirationThreshold = TimeSpan.FromMinutes(5);
        private readonly TimeSpan heartbeatInterval = TimeSpan.FromSeconds(30);
        private readonly HubConnection _hubconnection;
        public DiscoveryServer(string serverUrl)
        {
            _hubconnection = new HubConnectionBuilder()
                .WithUrl($"{serverUrl}/discoveryHub")
                .Build();
            _hubconnection.On<string>("RegisterPeer", RegisterPeer);
            _hubconnection.On<string>("HearbeatResponse", HandleHeartbeatResponse);
            _hubconnection.On<TaskCompletionSource<IEnumerable<string>>("GetConnectedPeers", GetConnectedPeers);
            StartHeartBeat();
            
        }
        public async Task StartAsync(string id)
        {
            await _hubconnection.StartAsync();
            await _hubconnection.InvokeAsync("Register participant", $"Participant with id:{id} joined");
        }
        
        private void RegisterPeer(string peerId)
        {
            lock (connectedPeers)
            {
                var existingPeer = connectedPeers.FirstOrDefault(x => x.Id == peerId);
                if(existingPeer is not null)
                {
                    existingPeer.LastInteractionTime = DateTime.UtcNow;

                }
                connectedPeers.Add(new ConnectedPeer(peerId));
            }
        }
        private void GetConnectedPeers(TaskCompletionSource<IEnumerable<string>> peerTcs)
        {
            lock (connectedPeers)
            {
                CleanupExpiredPeers();
                peerTcs.SetResult(connectedPeers.Select(x => x.Id));
            }
        }
        private void HandleHeartbeatResponse(string peerId)
        {
            lock (connectedPeers)
            {
                var existingPeer = connectedPeers.FirstOrDefault(x => x.Id == peerId);
                if (existingPeer != null)
                {
                    existingPeer.LastInteractionTime = DateTime.UtcNow;
                }
               
            }
        }
        private void StartHeartBeat()
        {
            Task heartbeat = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(heartbeatInterval);
                    SendHeartbeat();
                }
            });
        }
        private void SendHeartbeat()
        {
            lock (connectedPeers)
            {
                foreach (var connectedPeer in connectedPeers)
                {
                    Console.WriteLine($"Sending heartbeat to {connectedPeer.Id}");
                    _hubconnection.InvokeAsync("HeatbeatReceived", connectedPeer.Id);
                }

                CleanupExpiredPeers();
            }
        }
        private void CleanupExpiredPeers()
        {
            lock (connectedPeers)
            {
                var currentTime = DateTime.UtcNow;
                connectedPeers.RemoveAll(p => (currentTime - p.LastInteractionTime) > peerExpirationThreshold);
            }
        }
        
    }
}
