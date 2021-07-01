using System;
using Mate.Ganttplan.ConfirmationSimulator.Environment;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Options;
using MathNet.Numerics.Distributions;

namespace Mate.Ganttplan.ConfirmationSimulator.DistributionProvider
{
    public class WorkTimeGenerator
    {
        public static WorkTimeGenerator Create(Configuration configuration, int salt = 0)
        {
            return new WorkTimeGenerator(
                seed: configuration.GetOption<Seed>().Value + salt
                , deviation: configuration.GetOption<WorkTimeDeviation>().Value
                , simNumber: configuration.GetOption<SimulationNumber>().Value);
        }

        public WorkTimeGenerator(int seed, double deviation, int simNumber)
        {
            var source = new Random(Seed: seed 
                                          //TODO WARUM?
                                          //+ simNumber
                                          );
            _distribution = new LogNormal(mu: 0, sigma: deviation, randomSource: source);
        }

        private readonly LogNormal _distribution;
        /// <summary>
        /// Returns LogNormal-distributed worktime.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public long GetRandomWorkTime(long duration)
        {
            if (_distribution.Sigma == 0)
                return duration;

            long newDuration;
            while (true)
            {
                newDuration = (long)Math.Round(value: duration * _distribution.Sample(), mode: MidpointRounding.AwayFromZero);
                if (newDuration <= 3 * duration) break;
            }
            return newDuration > 1 ? newDuration : 1;
        }
    }
}
