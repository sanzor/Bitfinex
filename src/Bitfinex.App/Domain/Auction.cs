using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Domain
{
    internal class Auction
    {
        public string CreatedBy { get; set; }
        public Guid AuctionId { get; set; }
        public string ItemName { get; set; }
        public decimal StartingPrice { get; set; }
        public List<Bid> Bids { get; set; } = new List<Bid>();
        
    }
}
