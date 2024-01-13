using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoveryServer
{
    internal class ConnectedPeer
    {
        public string Id { get; set; }
        public DateTime LastInteractionTime { get; set; }
        public ConnectedPeer(string peerId)
        {
            Id = peerId;
            LastInteractionTime = DateTime.Now;
        }
    }
}
