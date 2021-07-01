using Akka.Actor;
using AkkaSim;
using AkkaSim.Definitions;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Nominal;
using Mate.Ganttplan.ConfirmationSimulator.Agents;
using Mate.Ganttplan.ConfirmationSimulator.Environment;
using Mate.Ganttplan.ConfirmationSimulator.Interfaces;
using Mate.Ganttplan.ConfirmationSimulator.SignalR;
using System.Threading.Tasks;

namespace Mate.Ganttplan.ConfirmationSimulator
{
    public abstract class BaseSimulation : ISimulation
    {
        public DataBase<MateProductionDb> DbProduction { get; }
        public IMessageHub MessageHub { get; }
        public SimulationType SimulationType { get; set; }
        public bool DebugAgents { get; set; }
        public Simulation Simulation { get; set; }
        public SimulationConfig SimulationConfig { get; set; }
        public ActorPaths ActorPaths { get; set; }
        public IActorRef JobCollector { get; set; }
        public IActorRef StorageCollector { get; set; }
        public IActorRef ContractCollector { get; set; }
        public IActorRef ResourceCollector { get; set; }
        public IActorRef MeasurementCollector { get; set; }
        public IStateManager StateManager { get; set; }
        public BaseSimulation(string dbName)
        {
        }

        public abstract Task<Simulation> InitializeSimulation(Configuration configuration);
    }
}
