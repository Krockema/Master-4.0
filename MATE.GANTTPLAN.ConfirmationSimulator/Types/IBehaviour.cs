using System;
using Akka.Actor;
using Mate.DataCore.Nominal;
using Mate.Ganttplan.ConfirmationSimulator.Agents;

namespace Mate.Ganttplan.ConfirmationSimulator.Types
{
    public interface IBehaviour
    {
        bool Action(object message);
        Agent Agent { get; set; }
        Func<IUntypedActorContext, AgentSetup, IActorRef> ChildMaker { get; }
        SimulationType SimulationType { get; }
        bool AfterInit();
        bool PostAdvance();
        void OnChildAdd(IActorRef actorRef);
    }
}
