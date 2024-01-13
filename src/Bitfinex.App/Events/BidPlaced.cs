using Bitfinex.App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Events
{
    internal class BidPlaced
    {
        public Guid AuctionId { get; set; }
        public Bid Bid { get; set; }

    }
}
