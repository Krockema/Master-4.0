using Akka.Actor;
using Mate.DataCore.Nominal;
using Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent.Behaviour;
using Mate.Ganttplan.ConfirmationSimulator.Environment;
using Mate.Ganttplan.ConfirmationSimulator.DistributionProvider;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor

        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, SimulationType simtype, string dbConnectionStringGanttPlan, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, configuration, time, simtype, dbConnectionStringGanttPlan, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, Configuration configuration, long time, SimulationType simtype, string dbConnectionStringGanttPlan, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: new Central(dbConnectionStringGanttPlan, workTimeGenerator, configuration, simtype)));
        }

        protected override void Finish()
        {
            // Do not Close agent by Finishmessage from Job
        }

    }
}