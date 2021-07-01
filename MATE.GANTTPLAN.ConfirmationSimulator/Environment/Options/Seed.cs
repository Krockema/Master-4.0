using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class Seed : Option<int>
    {
        public Seed(int value)
        {
            _value = value;
        }

        public Seed()
        {
            Action = (config, argument) => {
                config.AddOption(o: new Seed(value: int.Parse(s: argument)));
            };
        }
    }
}
