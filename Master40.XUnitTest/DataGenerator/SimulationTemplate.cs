using System;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using System.Threading.Tasks;
using AkkaSim.Logging;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Microsoft.Extensions.Logging;
using Xunit;
using LogLevel = NLog.LogLevel;
using Master40.SimulationCore.Helper;

namespace Master40.XUnitTest.DataGenerator
{
    public class SimulationTemplate
    {
        // local TEST Context
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testGeneratorCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestGeneratorContext;Trusted_Connection=True;MultipleActiveResultSets=true";

        // Definition for Simulation runs each Call returns
        // TODO: return complete config objects to avoid errors, and separate Data Generator / Simulation configurations
        public static IEnumerable<object[]> GetTestData()
        {
            // Simulation run 1
            yield return new object[]
            {
                1      // approach id (test data generator input parameter set id)
                , 240   // order Quantity
                , 240   // max bucket size
                , 60    // throughput time
                , null  // Random seed
                , 1/60d // arrival rate
                , 15000 // simulation end
                , 55    // min delivery time
                , 65    // max delivery time
                , 1     // test iteration number
            };
            /*yield return new object[]
            {
                18      // approach id (test data generator input parameter set id)
                , 240   // order Quantity
                , 240   // max bucket size
                , 60    // throughput time
                , null  // Random seed
                , 1/60d // arrival rate
                , 15000 // simulation end
                , 55    // min delivery time
                , 65    // max delivery time
                , 3     // test iteration number
            };*/
            // Simulation run 2
            /*yield return new object[]
            {
                8       // approach id (test data generator input parameter set id)
                , 2     // order Quantity
                , 240   // max bucket size
                , 1400  // throughput time
                , 552   // Random seed
                , 1/1300d// arrival rate
                , 2880  // simulation end
                , 1400  // min delivery time
                , 1440  // max delivery time
                , 2     // test iteration number
            };*/
        }

        /// <summary>
        /// To Run this test the Database must have been filled with Master data
        /// </summary>
        /// <param name="approachId"></param>
        /// <param name="orderQuantity"></param>
        /// <param name="maxBucketSize"></param>
        /// <param name="throughput"></param>
        /// <param name="seed"></param>
        /// <param name="arrivalRate"></param>
        /// <param name="simulationEnd"></param>
        /// <param name="minDeliveryTime"></param>
        /// <param name="maxDeliveryTime"></param
        /// <param name="testIterationNumber"></param>
        /// <returns></returns>
        [Theory]

        [MemberData(nameof(GetTestData))]
        public async Task SystemTestAsync(int approachId
                                        , int orderQuantity
                                        , int maxBucketSize
                                        , long throughput
                                        , int? seed
                                        , double arrivalRate
                                        , long simulationEnd
                                        , int minDeliveryTime
                                        , int maxDeliveryTime
                                        , int testIterationNumber)
        {
            _ = testIterationNumber;
            ResultContext ctxResult = ResultContext.GetContext(resultCon: testResultCtxString);
            ProductionDomainContext masterCtx = ProductionDomainContext.GetContext(testCtxString);
            DataGeneratorContext dataGenCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);

            var approach = ApproachRepository.GetApproachById(dataGenCtx, approachId);
            var generator = new MainGenerator();
            await Task.Run(() =>
                generator.StartGeneration(approach, masterCtx));

            var simContext = new AgentSimulation(DBContext: masterCtx, messageHub: new ConsoleHub());
            var simConfig = ArgumentConverter.ConfigurationConverter(ctxResult, 1);

            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Trace, LogLevel.Trace);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.PRIORITY, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.SCHEDULING, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.DISPOPRODRELATION, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.PROPOSAL, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.INITIALIZE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.JOB, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.ENQUEUE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.JOBSTATE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace, LogLevel.Trace);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AKKA, LogLevel.Warn);

            var dataGenSim = new DB.GeneratorModel.Simulation();
            dataGenSim.ApproachId = approachId;
            dataGenSim.StartTime = DateTime.Now;
            await Task.Run(() =>
            {
                dataGenCtx.Simulations.AddRange(dataGenSim);
                dataGenCtx.SaveChanges();
            });

            if (seed == null)
            {
                seed = new Random().Next();
            }

            // update customized Configuration
            simConfig.AddOption(new DBConnectionString(testResultCtxString));  
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: orderQuantity)); 
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Seed(value: (int) seed));
            simConfig.ReplaceOption(new SettlingStart(value: 0)); 
            simConfig.ReplaceOption(new SimulationEnd(value: simulationEnd));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: maxBucketSize));
            simConfig.ReplaceOption(new SimulationNumber(value: dataGenSim.Id));
            simConfig.ReplaceOption(new DebugSystem(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.0));
            // anpassen der Lieferzeiten anhand der Erwarteten Durchlaufzeit. 
            simConfig.ReplaceOption(new MinDeliveryTime(value: minDeliveryTime));
            simConfig.ReplaceOption(new MaxDeliveryTime(value: maxDeliveryTime));

            await Task.Run(() => 
                ArgumentConverter.ConvertBackAndSave(ctxResult, simConfig, dataGenSim.Id));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            
            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
                dataGenSim.FinishTime = DateTime.Now;
                dataGenSim.FinishedSuccessfully = sim.IsCompletedSuccessfully;
                await Task.Run(() =>
                        dataGenCtx.SaveChanges());
                System.Diagnostics.Debug.WriteLine("################################# Simulation has finished with number " + dataGenSim.Id);
                Assert.True(condition: sim.IsCompleted);
            }
        }

        /* CTE on MasterResultContext (SQL Querry)
         
                DECLARE @simNr BIGINT;
                SET @simNr = 0;

                WITH DirectReports(ProductionOrderId, JobName, Parent,CapabilityProvider, CapabilityName, HirachyNumber, BomLevel) 
                AS
                (       SELECT jobs.ProductionOrderId, jobs.JobName, jobs.ParentId, jobs.CapabilityProvider, jobs.CapabilityName, HierarchyNumber, 0 AS BomLevel
                        FROM SimulationJobs as jobs
		                WHERE jobs.ProductionOrderId = 'ProductionAgent(0)' and jobs.SimulationNumber = @simNr
                        
	                UNION ALL
                        
		                SELECT e.ProductionOrderId, e.JobName, e.Parent, e.CapabilityProvider, e.CapabilityName, e.HierarchyNumber, BomLevel + 1
                        FROM SimulationJobs as e
                        INNER JOIN DirectReports AS dr ON dr.ProductionOrderId = e.Parent and e.SimulationNumber =  @simNr
                )
                    SELECT distinct *
                    FROM DirectReports
	                order by BomLevel, ProductionOrderId ,HirachyNumber

         */

    }
}