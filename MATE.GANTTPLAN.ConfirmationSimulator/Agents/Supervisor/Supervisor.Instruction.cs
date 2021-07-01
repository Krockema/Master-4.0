using Akka.Actor;
using AkkaSim.Definitions;
using Mate.DataCore.DataModel;
using static FArticles;
using static FSetEstimatedThroughputTimes;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.SupervisorAgent
{
    public partial class Supervisor
    {
        public class Instruction
        {
            /*
                CreateContractAgent,
                RequestArticleBom,
                OrderProvided,
            */
            public class CreateHubAgent : SimulationMessage
            {
                public static CreateHubAgent Create(T_CustomerOrder message, IActorRef target)
                {
                    return new CreateHubAgent(message: message, target: target);
                }
                private CreateHubAgent(T_CustomerOrder message, IActorRef target) : base(message: message, target: target)
                {
                }
                public T_CustomerOrder GetObjectFromMessage { get => Message as T_CustomerOrder; }

            }
            public class RequestArticleBom : SimulationMessage
            {
                public static RequestArticleBom Create(int message, IActorRef target)
                {
                    return new RequestArticleBom(message: message, target: target);
                }
                private RequestArticleBom(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public int GetObjectFromMessage { get => (int)Message; }
            }
            public class OrderProvided : SimulationMessage
            {
                public static OrderProvided Create(FArticle message, IActorRef target)
                {
                    return new OrderProvided(message: message, target: target);
                }
                private OrderProvided(object message, IActorRef target) : base(message: message, target: target, logThis: true)
                {
                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }
            }

            public class EndSimulation : SimulationMessage
            {
                public static EndSimulation Create(object message, IActorRef target)
                {
                    return new EndSimulation(message: message, target: target);
                }
                private EndSimulation(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

            public class PopOrder : SimulationMessage
            {
                public static PopOrder Create(string message, IActorRef target)
                {
                    return new PopOrder(message: message, target: target);
                }
                private PopOrder(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

            public class SystemCheck : SimulationMessage
            {
                public static SystemCheck Create(string message, IActorRef target)
                {
                    return new SystemCheck(message: message, target: target);
                }
                private SystemCheck(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

            public class SetEstimatedThroughputTime : SimulationMessage
            {
                public static SetEstimatedThroughputTime Create(FSetEstimatedThroughputTime message, IActorRef target)
                {
                    return new SetEstimatedThroughputTime(message: message, target: target);
                }
                private SetEstimatedThroughputTime(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public FSetEstimatedThroughputTime GetObjectFromMessage { get => Message as FSetEstimatedThroughputTime; }
            }
        }
    }
}
