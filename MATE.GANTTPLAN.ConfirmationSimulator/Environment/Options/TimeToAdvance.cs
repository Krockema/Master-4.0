using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;
using System;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class TimeToAdvance : Option<TimeSpan>
    {
        public TimeToAdvance(TimeSpan value)
        {
            _value = value;
        }

        public TimeToAdvance()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TimeToAdvance(value:TimeSpan.FromMilliseconds(long.Parse(argument))));
            };
        }
    }

}
