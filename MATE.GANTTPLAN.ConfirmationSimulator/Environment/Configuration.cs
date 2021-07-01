using System;
using System.Collections.Generic;
using AkkaSim.Definitions;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Options;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment
{
    public class Configuration : Dictionary<Type, object>
    {
        public static Configuration Create(object[] args)
        {
            var s = new Configuration();
            foreach (var item in args)
            {
                s.AddOption(o: item);
            }
            return s;
        }
        public bool AddOption(object o)
        {
            return this.TryAdd(key: o.GetType(), value: o);
        }

        public T GetOption<T>()
        {
            this.TryGetValue(key: typeof(T), value: out object value);
            return (T)value;
        }

        public bool ReplaceOption(object o)
        {
            this.Remove(o.GetType());
            return AddOption(o);
        }

        public SimulationConfig GetContextConfiguration()
        {
            try
            {
                var config = new SimulationConfig(
                    debugAkka: false // Debug Akka Core System
                    , debugAkkaSim: this.GetOption<DebugSystem>().Value // set AkkaSim in Debug Mode
                    , interruptInterval: this.GetOption<KpiTimeSpan>().Value
                    , timeToAdvance: this.GetOption<TimeToAdvance>().Value);
                return config;
            }
            catch (Exception)
            {
                throw new Exception(message: "Configuration Error.");
            }
        }
    }
}
