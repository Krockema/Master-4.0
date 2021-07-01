using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Options;
using System.Collections.Generic;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment
{
    public class Commands : List<ICommand>
    {
        private static readonly Commands self = new Commands
        {
            new DebugAgents()
            , new TimeToAdvance()
            , new DebugSystem()
            , new KpiTimeSpan()
            , new Seed()
            , new SettlingStart()
            , new SimulationEnd()
            , new WorkTimeDeviation()
            , new SimulationId()
            , new SimulationKind()
            , new SimulationNumber()
            , new TimePeriodForThroughputCalculation()
        };


        public static List<ICommand> GetAllValidCommands => self;

        public static int CountRequiredCommands => self.Count;

    }
}
