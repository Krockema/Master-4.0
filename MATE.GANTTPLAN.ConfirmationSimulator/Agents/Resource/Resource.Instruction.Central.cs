using Akka.Actor;
using AkkaSim.Definitions;
using static FCentralActivities;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.ResourceAgent
{
    public partial class Resource
    {
        public partial class Instruction
        {
            public class Central
            {
                public class ActivityStart : SimulationMessage
                {
                    public static ActivityStart Create(FCentralActivity activity, IActorRef target)
                    {
                        return new ActivityStart(message: activity, target: target);
                    }
                    private ActivityStart(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralActivity GetObjectFromMessage => Message as FCentralActivity; 
                }
                public class ActivityFinish : SimulationMessage
                {
                    public static ActivityFinish Create(IActorRef target)
                    {
                        return new ActivityFinish(message: null, target: target);
                    }
                    private ActivityFinish(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                }

            }
        }
    }
}