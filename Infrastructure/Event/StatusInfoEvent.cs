using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Eureka.SystemE.Base;

namespace Eureka.CoinTrade.Infrastructure.Event
{
    public class StatusInfoEvent : CompositePresentationEvent<Tuple<KeyE,string>>
    {
        
    }

   
}
