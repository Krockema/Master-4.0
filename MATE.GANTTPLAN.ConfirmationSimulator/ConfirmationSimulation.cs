using Akka.Actor;
using Akka.Event;
using AkkaSim;
using AkkaSim.SpecialActors;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.Ganttplan.ConfirmationSimulator.Agents;
using Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent;
using Mate.Ganttplan.ConfirmationSimulator.DistributionProvider;
using Mate.Ganttplan.ConfirmationSimulator.Environment;
using Mate.Ganttplan.ConfirmationSimulator.Environment.Options;
using Mate.Ganttplan.ConfirmationSimulator.Interfaces;
using Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent.Types.Central;
using Mate.Production.Core.Reporting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource;
using System.Linq;

namespace Mate.Ganttplan.ConfirmationSimulator
{
    public class ConfirmationSimulation : BaseSimulation, ISimulation
    {
        public DataBase<GanttPlanDBContext> _dbGantt { get; }
        private ResourceDictionary _resourceDictionary { get; }
        private IActorRef _hubActor { get; set; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public ConfirmationSimulation(string ganttPlanDbName) : base(ganttPlanDbName)
        {
            _dbGantt = Dbms.GetGanttDataBase(ganttPlanDbName);
            _resourceDictionary = new ResourceDictionary();
        }
        public override Task<Simulation> InitializeSimulation(Configuration configuration)
        {
            return Task.Run(function: () =>
            {
                // Init Simulation
                SimulationConfig = configuration.GetContextConfiguration();
                DebugAgents = configuration.GetOption<DebugAgents>().Value;
                SimulationType = configuration.GetOption<SimulationKind>().Value;
                Simulation = new Simulation(simConfig: SimulationConfig);
                ActorPaths = new ActorPaths(simulationContext: Simulation.SimulationContext
                                              , systemMailBox: SimulationConfig.Inbox.Receiver);

            
                //if (DebugAgents) 
                AddDeadLetterMonitor();
                AddTimeMonitor();

                // Extract Resources and Groups from Ganttplan
                ExtractResourcesFromGanttplan();

                // Create Hub Agent
                CreateHubAgent(configuration);

                // Create Resources
                CreateResourceAgents(configuration: configuration);

                // Finally Initialize StateManger
                StateManager = new GanttStateManager(new List<IActorRef> { this.StorageCollector
                                                                         , this.JobCollector
                                                                         , this.ContractCollector
                                                                         , this.ResourceCollector
                                                                     }
                                                        , SimulationConfig.Inbox);

                return Simulation;
            });
        }
       

        private void CreateHubAgent(Configuration configuration)
        {

            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration, 0);

            _hubActor = Simulation.ActorSystem.ActorOf(Hub.Props(actorPaths: ActorPaths
                       , configuration: configuration
                       , simtype: SimulationType
                       , time: 0L //TODO: Okay?
                       , dbConnectionStringGanttPlan: _dbGantt.ConnectionString.ToString()
                       , workTimeGenerator: randomWorkTime
                       , debug: DebugAgents
                       , principal: Simulation.SimulationContext)
                   , name: "CentralHub");

        }

        /// <summary>
        /// Creates an Time Agent that is listening to Clock AdvanceTo messages and serves the frontend with current time updates
        /// </summary>
        private void AddTimeMonitor()
        {
            Action<long> tm = (timePeriod) => MessageHub.SendToClient(listener: "clockListener", msg: timePeriod.ToString());
            var timeMonitor = Props.Create(factory: () => new TimeMonitor((timePeriod) => tm(timePeriod)));
            Simulation.ActorSystem.ActorOf(props: timeMonitor, name: "TimeMonitor");
        }

        private void AddDeadLetterMonitor()
        {
            var deadletterWatchMonitorProps = Props.Create(factory: () => new DeadLetterMonitor());
            var deadletterWatchActorRef = Simulation.ActorSystem.ActorOf(props: deadletterWatchMonitorProps, name: "DeadLetterMonitoringActor");
            //subscribe to the event stream for messages of type "DeadLetter"
            Simulation.ActorSystem.EventStream.Subscribe(subscriber: deadletterWatchActorRef, channel: typeof(DeadLetter));
        }

       
        /// <summary>
        /// Creates ResourcesAgents based on current Model and add them to the SimulationContext
        /// </summary>
        /// <param name="configuration">Environment.Configuration</param>
        private void CreateResourceAgents(Configuration configuration)
        {
            // randomWorkTime generator on is generated on hub
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration);

            foreach (var resource in _resourceDictionary)
            {
                System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Value.Name}");

                //var resourceDefinition = new FCentralResourceDefinitions.FCentralResourceDefinition(resourceId: resource.Key, resourceName: resource.Value.Name, resource.Value.GroupId, (int)resource.Value.ResourceType);

                /*Simulation.SimulationContext
                    .Tell(message: Directory.Instruction.Central
                                            .CreateMachineAgents
                                            .Create(message: resourceDefinition, target: _hubActor)
                        , sender: Simulation.SimulationContext);
                */
            }
        }

        private void ExtractResourcesFromGanttplan()
        {
            //Crawl all workcenters
            var workcenters = _dbGantt.DbContext.GptblWorkcenter;
            foreach(var workcenter in workcenters)
            {
                var resourceElement = new WorkcenterDefinition(
                    name: workcenter.Name,
                    id: workcenter.WorkcenterId,
                    actorRef: ActorRefs.Nobody,
                    groupIds: null,
                    resourceType: DataCore.Nominal.Model.ResourceType.Workcenter
                    );
                _resourceDictionary.Add(int.Parse(resourceElement.Id), resourceElement);
            }

            //Crawl all workcentergroups
            var workcentergroups = _dbGantt.DbContext.GptblWorkcentergroup;
            foreach (var workcentergroup in workcentergroups)
            {
                var workcentersForGroup = _dbGantt.DbContext.GptblWorkcentergroupWorkcenter;
                
                List<IResourceDefinition> resourceDefinitions = new();
                var list = workcentersForGroup.Where(x => x.WorkcentergroupId.Equals(workcentergroup.WorkcentergroupId)).Select(y => y.WorkcenterId).ToList();
                foreach(var resource in list)
                {
                    var definition = _resourceDictionary.GetValueOrDefault(int.Parse(resource));

                    resourceDefinitions.Add((IResourceDefinition)definition);                
                }

                var groupelement = new WorkcenterGroupDefinition(
                    name: workcentergroup.Name,
                    id: workcentergroup.WorkcentergroupId,
                    resourceDefinitions: resourceDefinitions
                    );
            }

            //crawl all workers
            var worker = _dbGantt.DbContext.GptblWorker;


            
            //crawl all workergroups

            //make a resource entry for each resource (workcenter, worker, betriebshilfsmittel)

        }

    }
}

