using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.Data.Context;
using Mate.DataCore.ReportingModel;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment
{
    public static class ArgumentConverter
    {
        public static bool IsArg(List<ICommand> validCommands, string argument, ref ICommand command)
        {
            if (null != (command = validCommands.SingleOrDefault(predicate: x => argument
                    .Equals(value: "-" + x.ArgShort, comparisonType: StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            if (null != (command = validCommands.SingleOrDefault(predicate: x => argument
                    .Equals(value: "--" + x.ArgLong, comparisonType: StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            return false;
        }

        public static Configuration ConfigurationConverter(MateResultDb resultCtx, int id)
        {
            var validCommands = Commands.GetAllValidCommands;
            var config = new Configuration();
            var command = validCommands.Single(x => x.ArgLong == "SimulationId");
            command.Action(config, id.ToString());

            var configurationItems = resultCtx.ConfigurationRelations
                                    .Include(x => x.ChildItem)
                                    .Where(x => x.Id == id).Select(x => x.ChildItem).ToList();

            if (id != 1)
            {
                configurationItems = AddDefaultItems(resultCtx, configurationItems);
            }

            foreach (var item in configurationItems)
            {
                if (!IsArg(validCommands: validCommands, argument: "--" + item.Property, command: ref command))
                    throw new InvalidOperationException("No command found that is equal to:" + item.Property);
                //else
                command.Action(arg1: config, arg2: item.PropertyValue);
            }

            return config;
        }

        private static List<ConfigurationItem> AddDefaultItems(MateResultDb resultCtx, List<ConfigurationItem> configurationSpecificItems)
        {
            var configurationItems = resultCtx.ConfigurationRelations
                .Include(x => x.ChildItem)
                .Where(x => x.Id == 1).Select(x => x.ChildItem).ToList();

            foreach (var specificItem in configurationSpecificItems)
            {
                var itemToRemove =
                    configurationItems.SingleOrDefault(x => x.Property == specificItem.Property);
                if (itemToRemove != null)
                {
                    configurationItems.Remove(itemToRemove);
                }
            }

            configurationItems.AddRange(configurationSpecificItems);
            return configurationItems;
        }

        public static void ConvertBackAndSave(MateResultDb resultCtx, Configuration config, int simulationNumber)
        {
            var configs = new List<SimulationConfig>();
            foreach (var item in config)
            {
                configs.Add(new SimulationConfig()
                {
                    Property = item.Key.Name,
                    PropertyValue = ((dynamic)item.Value).Value.ToString(),
                    SimulationNumber = simulationNumber
                });
            }
            resultCtx.SimulationConfigs.AddRange(configs);
            resultCtx.SaveChanges();
        }
    }
}
