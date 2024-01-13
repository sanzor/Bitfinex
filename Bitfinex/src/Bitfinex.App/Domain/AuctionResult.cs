using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Domain
{
    public class AuctionResult
    {
        public Guid AuctionId { get; set; }
        public string Winner { get; set; }
        public decimal WinningAmount { get; set; }
    }
}
