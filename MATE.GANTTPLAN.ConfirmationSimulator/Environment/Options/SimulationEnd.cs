using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class SimulationEnd : Option<long>
    {
        public SimulationEnd(long value)
        {
            _value = value;
        }

        public SimulationEnd()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationEnd(value: long.Parse(s: argument)));
            };
        }
    }
}
