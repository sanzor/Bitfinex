using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Domain
{
    public class BidParams
    {
        public Guid AuctionId { get; set; }
        public string Bidder { get; set; }
        public decimal Amount { get; set; }
    }
}
