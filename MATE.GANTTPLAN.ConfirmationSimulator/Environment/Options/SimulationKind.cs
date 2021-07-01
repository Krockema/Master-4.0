using System;
using Mate.DataCore.Nominal;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class SimulationKind : Option<SimulationType>
    {
        public SimulationKind(SimulationType value)
        {
            _value = value;
        }

        public SimulationKind()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(value: SimulationType.Default.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationKind(value: SimulationType.Decentral));
                }
                else if (arg.Equals(value: SimulationType.Queuing.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationKind(value: SimulationType.Central));
                }
                else
                {
                    throw  new Exception(message: "Unknown argument.");
                }
            };
        }
    }
}
