using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;
using System;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    /// <summary>
    /// Defines whether Agent.Debug is also logged to console
    /// Require: Agent.Debug.Equal(true)
    /// </summary>
    public class DebugToConsole : Option<bool>
    {
        public static Type Type => typeof(DebugToConsole);
        public DebugToConsole(bool value)
        {
            _value = value;
        }
    }
}
