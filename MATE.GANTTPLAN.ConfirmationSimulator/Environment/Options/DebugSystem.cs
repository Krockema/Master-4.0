using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class DebugSystem : Option<bool>
    {
        public DebugSystem(bool value)
        {
            _value = value;
        }

        public DebugSystem()
        {
            Action = (config, argument) => {
                config.AddOption(o: new DebugSystem(value: bool.Parse(value: argument)));
            };
        }
    }
}
