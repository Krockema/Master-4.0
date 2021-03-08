using System;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
using Master40.DB.ReportingModel;
using Master40.Simulation;
using Master40.Simulation.CLI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Environment.Options;
using Master40.Tools.Connectoren.Ganttplan;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment;
using Master40.Tools.SignalR;

namespace Master40.Controllers
{
    public class AgentLiveController : Controller
    {
        private readonly AgentCore _agentSimulator;
        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultCtx;
        private readonly IMessageHub _messageHub;
        private const string GanttPlanCtxString = "SERVER=(localdb)\\MSSQLLocalDB;DATABASE=DBGP;Trusted_connection=Yes;UID=;PWD=;MultipleActiveResultSets=true";

        public AgentLiveController(AgentCore agentSimulator, ProductionDomainContext context, ResultContext resultCtx, IMessageHub messageHub)
        {
            _agentSimulator = agentSimulator;
            _resultCtx = resultCtx;
            _context = context;
            _messageHub = messageHub;
        }
        public async Task<IActionResult> Index()
        {
            var resources = await _context.Resources.Where(x => x.IsPhysical)
                                              .Include(navigationPropertyPath: a => a.ResourceSetups)
                                              .ThenInclude(x => x.ResourceCapabilityProvider)
                                              .ThenInclude(x => x.ResourceCapability).ToListAsync();

            ViewData[index: "machines"] = resources.Select(selector: x => x.Name).ToList();
            return View(model: resources);
        }

        [HttpGet(template: "[Controller]/RunAsync/{simulationType}/orderAmount/{orderAmount}/arivalRate/{arivalRate}/estimatedThroughputTime/{estimatedThroughputTime}")]
        public async void RunAsync(int simulationType, int orderAmount, double arivalRate,int estimatedThroughputTime)
        {
            _context.ClearCustomerOrders();

            var simConfig = ArgumentConverter.ConfigurationConverter(resultCtx: _resultCtx, 1);
            // update customized Items
            simConfig.AddOption(new DBConnectionString(_resultCtx.Database.GetDbConnection().ConnectionString));
            //simConfig.ReplaceOption(new KpiTimeSpan(240));
            //simConfig.ReplaceOption(new TimeToAdvance(value: TimeSpan.FromMilliseconds(50)));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new OrderArrivalRate(value: 1/192d));
            simConfig.ReplaceOption(new OrderQuantity(value: 2000));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: 2000));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Seed(value: 345758734));
            simConfig.ReplaceOption(new SettlingStart(value: 0));
            simConfig.ReplaceOption(new SimulationEnd(value: 10080));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.0));
            simConfig.ReplaceOption(new MaxDeliveryTime(240));
            simConfig.ReplaceOption(new MinDeliveryTime(144));
            simConfig.ReplaceOption(new MaxBucketSize(value: 240));



            switch (simulationType)
            {
                case 1: RunAgentSimulation(simConfig); return;
                case 2: RunGanttSimulation(simConfig); return;
                default: return;
            }

        }

        public async void RunAgentSimulation(Configuration simConfig)
        {
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Default));
            simConfig.ReplaceOption(new SimulationNumber(2));
            await _agentSimulator.RunAkkaSimulation(configuration: simConfig);
        }

        public async void RunGanttSimulation(Configuration simConfig)
        {
            GanttPlanDBContext.ClearDatabase(GanttPlanCtxString);
            
            //Synchronisation GanttPlan
            GanttPlanOptRunner.Inizialize();

            var simContext = new GanttSimulation(GanttPlanCtxString, _context.Database.GetDbConnection().ConnectionString, messageHub: _messageHub);
            
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Central));
            simConfig.ReplaceOption(new DebugSystem(value: false));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            //emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());
            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
            }
        }


        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        /// <summary>
        /// TODO : Change from uri to json Body to post any parameter (more Dynamic)
        /// </summary>
        /// <param name="items"></param>
        /*
         Sample Json for Postman Body:
         
        [{ Id: '1', Property: 'SimNumber', PropertyValue: '1', Description: 'SimNumber' },
         { Id: '2', Property: 'SimType', PropertyValue: 'Bucket', Description: 'SimType' }]

        Header: 
        Content-Type 'application/json'
        */
        [HttpPost(template: "[Controller]/StartSim")]
        public void StartSim([FromBody] List<ConfigurationItem> items)
        {
            if (ModelState.IsValid)
            {

            }
        }



        [HttpGet(template: "[Controller]/ReloadGantt/{orderId}/{stateId}")]
        public IActionResult ReloadGantt(int orderId, int stateId)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent(componentName: "SimulationTimeline", arguments: new List<int> { orderId, stateId });
        }

        [HttpGet(template: "[Controller]/MachineWorkload/{Machine}")]
        public IActionResult MachineWorkload(string machine)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent(componentName: "MachineWorkload", arguments: new { machine });
        }

        [HttpGet(template: "[Controller]/MachineBreakdown/{Machine}")]
        public void MachineBreakdown(string machine)
        {
            _agentSimulator.ResourceBreakDown(name: machine);
        }
    }
}