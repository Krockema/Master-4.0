using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class SettlingStart : Option<int>
    {
        public SettlingStart(int value)
        {
            _value = value;
        }

        public SettlingStart()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SettlingStart(value: int.Parse(s: argument)));
            };
        }
    }
}
