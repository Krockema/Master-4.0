using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;
namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class SimulationNumber : Option<int>
    {
        public SimulationNumber(int value)
        {
            _value = value;
        }

        public SimulationNumber()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationNumber(value: int.Parse(s: argument)));
            };
        }
    }
}
