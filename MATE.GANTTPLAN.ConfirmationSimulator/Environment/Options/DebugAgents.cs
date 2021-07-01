using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;
using System;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class DebugAgents : Option<bool>
    {
        public DebugAgents(bool value)
        {
            _value = value;
        }
        public DebugAgents()
        {
            Action = (config, argument) => {
                config.AddOption(o: new DebugAgents(value: bool.Parse(value: argument)));
            };
        }
    }
}
