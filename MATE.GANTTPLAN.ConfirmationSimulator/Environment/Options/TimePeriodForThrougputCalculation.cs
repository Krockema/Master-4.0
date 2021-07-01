using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Options
{
    public class TimePeriodForThroughputCalculation : Option<long>
    {
        /// <summary>
        /// Specifies the time period over wich the Througput KPI is calculatet during the simulation.
        /// This has direct influence on the feedback for start of new CustomerOrders.
        /// </summary>
        public TimePeriodForThroughputCalculation(long value)
        {
            _value = value;
        }

        public TimePeriodForThroughputCalculation()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TimePeriodForThroughputCalculation(value: long.Parse(s: argument)));
            };
        }
    }
}
