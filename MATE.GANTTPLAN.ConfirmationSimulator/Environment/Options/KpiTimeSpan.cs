using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class KpiTimeSpan : Option<long>
    {
        public static Type Type => typeof(KpiTimeSpan);
        public KpiTimeSpan(long value)
        {
            _value = value;
        }

        public KpiTimeSpan()
        {
            Action = (config, argument) => {
                config.AddOption(o: new KpiTimeSpan(value: long.Parse(s: argument)));
            };
        }
    }
}
