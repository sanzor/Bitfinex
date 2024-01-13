using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Domain
{
    internal class Bid
    {
        public Guid Id { get; set; }
        public string Bidder { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateOfBid { get; set; }

    }
}
