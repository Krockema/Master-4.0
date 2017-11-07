﻿using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.Tools.Simulation
{
    public static class CopyResults
    {
        public static void Copy(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            ExtractKpis(inMemmoryContext, productionDomainContext);
            ExtractWorkSchedules(inMemmoryContext, productionDomainContext);
            ExtractStockExchanges(inMemmoryContext, productionDomainContext);
            Kpi sim = ExtractSimulationOrders(inMemmoryContext, productionDomainContext);

            var simConfig = productionDomainContext.SimulationConfigurations.Single(s => s.Id == sim.SimulationConfigurationId);
            if (sim.SimulationType == SimulationType.Central) { simConfig.CentralRuns += 1; } else { simConfig.DecentralRuns += 1; }
            productionDomainContext.SaveChanges();



        }

        private static Kpi ExtractSimulationOrders(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<SimulationOrder> so = new List<SimulationOrder>();
            var sim = productionDomainContext.Kpis.Last(); // i know not perfect ...
            foreach (var item in inMemmoryContext.Orders)
            {
                SimulationOrder set = new SimulationOrder();
                item.CopyPropertiesTo<IOrder>(set);
                set.SimulationConfigurationId = sim.SimulationConfigurationId;
                set.SimulationNumber = sim.SimulationNumber;
                set.SimulationType = sim.SimulationType;
                set.OriginId = item.Id;
                so.Add(set);

            }
            productionDomainContext.SimulationOrders.AddRange(so);
            productionDomainContext.SaveChanges();
            return sim;
        }

        private static void ExtractKpis(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<Kpi> kpis = new List<Kpi>();
            foreach (var item in inMemmoryContext.Kpis)
            {
                kpis.Add(item.CopyDbPropertiesWithoutId());
            }
            productionDomainContext.Kpis.AddRange(kpis);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractWorkSchedules(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<SimulationWorkschedule> sw = new List<SimulationWorkschedule>();
            foreach (var item in inMemmoryContext.SimulationWorkschedules)
            {
                sw.Add(item.CopyDbPropertiesWithoutId());
            }

            productionDomainContext.SimulationWorkschedules.AddRange(sw);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractStockExchanges(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<StockExchange> se = new List<StockExchange>();
            foreach (var item in inMemmoryContext.StockExchanges)
            {
                se.Add(item.CopyDbPropertiesWithoutId());
            }
            productionDomainContext.StockExchanges.AddRange(se);
            productionDomainContext.SaveChanges();
        }
    }
}