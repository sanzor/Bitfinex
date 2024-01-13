using Bitfinex.App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Events
{
    internal class AuctionCreated
    {
        public Auction Auction { get; set; }
        public DateTime DateTime { get; set; }
    }
}
