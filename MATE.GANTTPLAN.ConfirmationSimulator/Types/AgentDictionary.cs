using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace Mate.Ganttplan.ConfirmationSimulator.Types
{
    public class AgentDictionary : Dictionary<object, IActorRef>
    {
        public List<IActorRef> ToSimpleList()
        {
            return this.Select(x => x.Value).ToList();
        }
    }

}
