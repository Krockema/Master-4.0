using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;
using System;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class WorkTimeDeviation : Option<double>
    {
        public WorkTimeDeviation(double value)
        {
            _value = value;
        }

        public WorkTimeDeviation()
        {
            Action = (config, argument) => {
                config.AddOption(o: new WorkTimeDeviation(value: double.Parse(argument)));
            };
        }
    }
}
