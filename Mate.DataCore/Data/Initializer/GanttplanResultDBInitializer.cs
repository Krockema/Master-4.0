using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;

namespace Mate.DataCore.Data.Initializer
{
    public static class GanttplanResultDBInitializer
    {
        //[ThreadStatic] 
        private static int _simulationId = 1;
        public static void DbInitialize(MateResultDb context)
        {
            context.Database.EnsureCreated();

            if (context.ConfigurationItems.Any())
            {
                return; // DB has been seeded
            }

            var configurationItems = new List<ConfigurationItem>()
            {
                new ConfigurationItem
                    {Property = "SimulationId", PropertyValue = _simulationId.ToString(), Description = "selfOrganizing with normal Queue"},
                new ConfigurationItem {Property = "SimulationNumber", PropertyValue = "1", Description = "Default"},
                new ConfigurationItem {Property = "SimulationKind", PropertyValue = "Default", Description = "Default"},
                new ConfigurationItem {Property = "DebugAgents", PropertyValue = "false", Description = "Default"},
                new ConfigurationItem {Property = "DebugSystem", PropertyValue = "false", Description = "Default"},
                new ConfigurationItem {Property = "KpiTimeSpan", PropertyValue = "480", Description = "Default"},
                new ConfigurationItem {Property = "TimePeriodForThroughputCalculation", PropertyValue = "1920", Description = "Default"},
                new ConfigurationItem {Property = "Seed", PropertyValue = "1337", Description = "Default"},
                new ConfigurationItem {Property = "SettlingStart", PropertyValue = "2880", Description = "Default"},
                new ConfigurationItem {Property = "SimulationEnd", PropertyValue = "40320", Description = "Default"},
                new ConfigurationItem {Property = "WorkTimeDeviation", PropertyValue = "0.2", Description = "Default"},
                new ConfigurationItem {Property = "TimeToAdvance", PropertyValue = "0", Description = "Default"},
            };
            context.ConfigurationItems.AddRange(entities: configurationItems);
            context.SaveChanges();
            AssertConfigurations(context, configurationItems, _simulationId);

            _simulationId = 1;
        }

        private static void CreateSimulation(MateResultDb context, List<ConfigurationItem> items, string description)
        {
            _simulationId++;
            var configurationItems = new List<ConfigurationItem>
            {
                new ConfigurationItem {Property = "SimulationId", PropertyValue = _simulationId.ToString(), Description = description },
            };
            items.ForEach(x => configurationItems.Add(x));

            context.ConfigurationItems.AddRange(configurationItems);
            context.SaveChanges();
            AssertConfigurations(context, configurationItems, _simulationId);
        }


        private static void AssertConfigurations(MateResultDb context, List<ConfigurationItem> configurationItems, int simulationId)
        {
            var configurationRelations = new List<ConfigurationRelation>();
            var simId = int.Parse(configurationItems.Single(x => x.Property == "SimulationId").PropertyValue);
            foreach (var item in configurationItems)
            {
                if (item.Id != 1)
                {
                    configurationRelations.Add(new ConfigurationRelation {ParentItemId = simId, ChildItemId = item.Id, Id = simulationId });
                }
            }

            context.ConfigurationRelations.AddRange(entities: configurationRelations);
            context.SaveChanges();
        }

    }
}
