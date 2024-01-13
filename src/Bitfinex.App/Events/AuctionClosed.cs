using Bitfinex.App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.App.Events
{
    internal class AuctionClosed
    {
       public AuctionResult Result { get; set; }
        public DateTime Date { get; set; }
    }
}
