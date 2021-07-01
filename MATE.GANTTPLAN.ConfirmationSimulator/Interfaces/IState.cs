using AkkaSim;

namespace Mate.Ganttplan.ConfirmationSimulator.Interfaces
{
    public interface IStateManager
    {
        void AfterSimulationStarted(Simulation sim);
        void AfterSimulationStopped(Simulation sim);
        void ContinueExecution(Simulation sim);
        void SimulationIsTerminating(Simulation sim);
    }
}