using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim;
using AkkaSim.Definitions;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Nominal;
using Mate.Ganttplan.ConfirmationSimulator.Agents;
using Mate.Ganttplan.ConfirmationSimulator.Environment;
using Mate.Ganttplan.ConfirmationSimulator.SignalR;

namespace Mate.Ganttplan.ConfirmationSimulator.Interfaces
{
    public interface ISimulation
    {
        DataBase<MateProductionDb> DbProduction { get; }
        IMessageHub MessageHub { get; }
        SimulationType SimulationType { get; set; }
        bool DebugAgents { get; set; }
        Simulation Simulation { get; set; }
        SimulationConfig SimulationConfig { get; set; }
        ActorPaths ActorPaths { get; set; }
        IActorRef JobCollector { get; set; }
        IActorRef StorageCollector { get; set; }
        IActorRef ContractCollector { get; set; }
        IActorRef ResourceCollector { get; set; }
        IActorRef MeasurementCollector { get; set; }
        IStateManager StateManager { get; set; }
        Task<Simulation> InitializeSimulation(Configuration configuration);
    }
}
