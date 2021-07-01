using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class SimulationId : Option<int>
    {
        public SimulationId(int value)
        {
            _value = value;
        }

        public SimulationId()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationId(value: int.Parse(argument)));
            };
        }
    }
}
