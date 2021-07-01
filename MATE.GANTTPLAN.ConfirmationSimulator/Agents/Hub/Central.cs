using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Ganttplan.ConfirmationSimulator.DistributionProvider;
using Mate.Ganttplan.ConfirmationSimulator.Environment;
using Mate.Production.Core.Agents.HubAgent.Types.Central;
using Mate.Production.Core.Helper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static FCentralActivities;
using Resource = Mate.Production.Core.Agents.ResourceAgent.Resource;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent.Behaviour
{
    class Central : ConfirmationSimulator.Types.Behaviour
    {
        private Dictionary<string, FCentralActivity> _scheduledActivities { get; } = new Dictionary<string, FCentralActivity>();
        private string _dbConnectionStringGanttPlan { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        private PlanManager _planManager { get; } = new PlanManager();
        private ResourceManager _resourceManager { get; } = new ResourceManager();
        private ActivityManager _activityManager { get; } = new ActivityManager();
        private ConfirmationManager _confirmationManager { get; }
        private StockPostingManager _stockPostingManager { get; } 
        private List<GptblSalesorder> _salesOrder { get; set; } = new List<GptblSalesorder>();
        private List<GptblMaterial> _products { get; } = new List<GptblMaterial>();
        private List<GptblSalesorderMaterialrelation> _SalesorderMaterialrelations { get; set;  } = new List<GptblSalesorderMaterialrelation>();
        private Dictionary<string, string> _prtResources { get; set; } = new Dictionary<string, string>();

        public Central(string dbConnectionStringGanttPlan, WorkTimeGenerator workTimeGenerator, Configuration configuration, SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
            _dbConnectionStringGanttPlan = dbConnectionStringGanttPlan;
            _confirmationManager = new ConfirmationManager(dbConnectionStringGanttPlan);
            _stockPostingManager = new StockPostingManager(dbConnectionStringGanttPlan);

            using (var localganttplanDB = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                _products = localganttplanDB.GptblMaterial.Where(x => x.Info1.Equals("Product")).ToList();

                var resources = localganttplanDB.GptblWorkcenter; 

            }

        }

            public override bool Action(object message)
            {
                switch (message)
                {
                    case Hub.Instruction.Central.AddResourceToHub msg: AddResourceToHub(msg.GetResourceRegistration); break;
                    case Hub.Instruction.Central.LoadProductionOrders msg: LoadProductionOrders(msg.GetInboxActorRef); break;
                    case Hub.Instruction.Central.StartActivities msg: StartActivities(); break;
                    case Hub.Instruction.Central.ScheduleActivity msg: ScheduleActivity(msg.GetActivity); break;
                    case Hub.Instruction.Central.ActivityFinish msg: FinishActivity(msg.GetObjectFromMessage); break;
                    default: return false;
                }
                return true;
            }

        
        private void AddResourceToHub(FCentralResourceRegistrations.FCentralResourceRegistration resourceRegistration)
        {
            var resourceDefintion = new ResourceDefinition(resourceRegistration.ResourceName, resourceRegistration.ResourceId, resourceRegistration.ResourceActorRef, resourceRegistration.ResourceGroupId, (ResourceType)resourceRegistration.ResourceType);
            _resourceManager.Add(resourceDefintion);
        }



        private void LoadProductionOrders(IActorRef inboxActorRef)
        {
            //For reporting
            var timeStamps = new Dictionary<string, long>();
            long lastStep = 0;
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            //Write confirmaations and stock postings to Ganttplan 
            _stockPostingManager.TransferStockPostings();
            _confirmationManager.TransferConfirmations();

            //Advance time step at Ganttplan
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                var modelparameter = localGanttPlanDbContext.GptblModelparameter.FirstOrDefault();
                if (modelparameter != null)
                {
                    modelparameter.ActualTime = Agent.CurrentTime.ToDateTime();
                    localGanttPlanDbContext.SaveChanges();
                }
            }

            //Reporting
            lastStep = stopwatch.ElapsedMilliseconds;
            timeStamps.Add("Write Confirmations", lastStep);

            //Start Ganttplan replan and export for write to GanttplanDB
            Debug.WriteLine("Start GanttPlan");
            GanttPlanOptRunner.RunOptAndExport("Continuous"); //changed to Init - merged configs
            Debug.WriteLine("Finish GanttPlan");

            //Reporting
            lastStep = stopwatch.ElapsedMilliseconds - lastStep;
            timeStamps.Add("Gantt Plan Execution", lastStep);

            //Refresh queues for resources
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                foreach (var resourceState in _resourceManager.resourceStateList)
                {
                    // put to an sorted Queue on each Resource
                    resourceState.ActivityQueue = new Queue<GptblProductionorderOperationActivityResourceInterval>(
                        localGanttPlanDbContext.GptblProductionorderOperationActivityResourceInterval
                            .Include(x => x.ProductionorderOperationActivityResource)
                                .ThenInclude(x => x.ProductionorderOperationActivity)
                                    .ThenInclude(x => x.ProductionorderOperationActivityMaterialrelation)
                            .Include(x => x.ProductionorderOperationActivityResource)
                                .ThenInclude(x => x.ProductionorderOperationActivity)
                                    .ThenInclude(x => x.Productionorder)
                            .Include(x => x.ProductionorderOperationActivityResource)
                                .ThenInclude(x => x.ProductionorderOperationActivity)
                                    .ThenInclude(x => x.ProductionorderOperationActivityResources)
                            .Where(x => x.ResourceId.Equals(resourceState.ResourceDefinition.Id.ToString())
                                        && x.IntervalAllocationType.Equals(1) 
                                        && (x.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Status != (int)GanttActivityState.Finished 
                                            && x.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Status != (int)GanttActivityState.Started))
                            .OrderBy(x => x.DateFrom)
                            .ToList());
                } // filter Done and in Progress?

            }


            Agent.DebugMessage($"GanttPlanning number {_planManager.PlanVersion} incremented {_planManager.IncrementPlaningNumber()}");
            
            //Clear scheudules Activies because of refreshed queues on resources
            _scheduledActivities.Clear();

            //Send start activities to each unbusy resource 
            StartActivities();

            //Reporting finish all tasks
            lastStep = stopwatch.ElapsedMilliseconds - lastStep;
            timeStamps.Add("Load Gantt Results", lastStep);
            timeStamps.Add("TimeStep", Agent.CurrentTime);
            stopwatch.Stop();
            
            //Return to simulation
            inboxActorRef.Tell(new FCentralGanttPlanInformations.FCentralGanttPlanInformation(JsonConvert.SerializeObject(timeStamps), "Plan"), this.Agent.Context.Self);
           
        }

        private void StartActivities()
        {
            //Check to start activity on each resource
            foreach (var resourceState in _resourceManager.resourceStateList)
            {
                //Skip if resource is working
                if(resourceState.IsWorking) continue;
                if (resourceState.ActivityQueue.IsNull() || resourceState.ActivityQueue.Count < 1) continue;

                //Peek first
                var interval = resourceState.ActivityQueue.Peek();

                //CheckMaterial
                var featureActivity = new FCentralActivity(resourceState.ResourceDefinition.Id
                    , interval.ProductionorderId
                    , interval.OperationId
                    , interval.ActivityId
                    , Agent.CurrentTime
                    , interval.ConvertedDateTo - interval.ConvertedDateFrom
                    , interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity.Name
                    , _planManager.PlanVersion
                    , Agent.Context.Self
                    , string.Empty
                    , interval.ActivityId.Equals(2) ? JobType.SETUP : JobType.OPERATION); // may not required.
                
                // Feature Activity
                if (interval.ConvertedDateFrom > Agent.CurrentTime)
                {
                   // only schedule activities that have not been scheduled
                    if(_scheduledActivities.TryAdd(featureActivity.Key, featureActivity))
                    {   
                        var waitFor = interval.ConvertedDateFrom - Agent.CurrentTime;
                        Agent.DebugMessage($"{featureActivity.Key} has been scheduled to {Agent.CurrentTime + waitFor} as planning interval {_planManager.PlanVersion}");
                        Agent.Send(instruction: Hub.Instruction.Central.ScheduleActivity.Create(featureActivity, Agent.Context.Self)
                                            , waitFor);
                    }
                }
                else
                {
                    if (interval.ConvertedDateFrom < Agent.CurrentTime)
                    {
                        Agent.DebugMessage($"Activity {featureActivity.Key} at {resourceState.ResourceDefinition.Name} is delayed {interval.ConvertedDateFrom}");
                    }
                    
                    // Activity is scheduled for now
                    TryStartActivity(interval, featureActivity);
                }
            }
        }

        /// <summary>
        /// Check if Feature is still active.
        /// 
        /// </summary>
        /// <param name="featureActivity"></param>
        private void ScheduleActivity(FCentralActivity featureActivity)
        {
            if (featureActivity.GanttPlanningInterval < _planManager.PlanVersion)
            {
                Agent.DebugMessage($"{featureActivity.Key} has new schedule from more recent planning interval {featureActivity.GanttPlanningInterval}");
                return;
            }

            if (_activityManager.Activities.Any(x => x.Activity.GetKey.Equals(featureActivity.Key)))
            {
                Agent.DebugMessage($"Actvity {featureActivity.Key} already in progress");
                return;
            }

            //Agent.DebugMessage($"TryStart scheduled activity {featureActivity.Key}");
            if (_scheduledActivities.ContainsKey(featureActivity.Key))
            {

                var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == featureActivity.ResourceId);
                if (NextActivityIsEqual(resource, featureActivity.Key))
                {
                    //Agent.DebugMessage($"Activity {featureActivity.Key} is equal start now after been scheduled by {featureActivity.ResourceId}");
                    _scheduledActivities.Remove(featureActivity.Key);
                    TryStartActivity(resource.ActivityQueue.Peek(), featureActivity);
                }
            }
        }

        private void TryStartActivity(GptblProductionorderOperationActivityResourceInterval interval,FCentralActivity fActivity)
        {
            // not sure if this works or if we just loop through resources and check even activities
            var resourcesForActivity = interval.ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity
                                                .ProductionorderOperationActivityResources;

            var resourceId = resourcesForActivity.SingleOrDefault(x => x.ResourceType.Equals(5))?.ResourceId;

            var requiredCapability = _prtResources[resourceId];

            var resource = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(fActivity.ResourceId));

            //Agent.DebugMessage($"{resource.ResourceDefinition.Name} try start activity {fActivity.Key}!");

            var requiredResources = new List<ResourceState>();
            var resourceCount = 0;
            //activity can be ignored as long any resource is working -> after finish work of the resource it will trigger anyways
            foreach (var resourceForActivity in resourcesForActivity)
            {
                if (_prtResources.ContainsKey(resourceForActivity.ResourceId))
                {
                    //do not request, because tool is infinity
                    continue;
                }
                var resourceState = _resourceManager.resourceStateList.Single(x => x.ResourceDefinition.Id == int.Parse(resourceForActivity.ResourceId));

                //Agent.DebugMessage($"Try to start activity {fActivity.Key} at {resourceState.ResourceDefinition.Name}");
                if (_resourceManager.ResourceIsWorking(int.Parse(resourceForActivity.ResourceId)))
                {
                    //Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has current work{resourceState.GetCurrentProductionOperationActivity}. Stop TryStartActivity!");
                    return;
                }

                if (!NextActivityIsEqual(resourceState, fActivity.Key))
                {
                    var nextActivity = resourceState.ActivityQueue.Peek();
                    //Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has different work next {nextActivity.ProductionorderId}|{nextActivity.OperationId}|{nextActivity.ActivityId}. Stop TryStartActivity!");
                    return;

                }

                resourceCount++;
                Agent.DebugMessage($"{resourceCount} of {resourcesForActivity.Count} Resource for Activity {fActivity.Key} are ready");
                requiredResources.Add(resourceState);
            }

            //Check if all preconditions are fullfilled
            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;
            
            /*
             * if (!_activityManager.HasPreconditionsFullfilled(activity, requiredResources))
            {
                Agent.DebugMessage($"Preconditions for {fActivity.Key} are not fulfilled!");
                return;

            }
            */

            StartActivity(requiredResources: requiredResources
                                , interval: interval
                                , featureActivity: fActivity
                                , requiredCapability: requiredCapability);
        }

        private bool NextActivityIsEqual(ResourceState resourceState, string activityKey)
        {
            var nextActivity = resourceState.ActivityQueue.Peek();
            if (nextActivity == null)
            {
                Agent.DebugMessage($"No next activity for {resourceState.ResourceDefinition.Name} in resource activity queue");
                return false;
            }
            Agent.DebugMessage($"Next activity is {nextActivity.GetKey} but should be {activityKey} ");
            return nextActivity.GetKey == activityKey;
        }

        
        private void StartActivity(List<ResourceState> requiredResources
            , GptblProductionorderOperationActivityResourceInterval interval
            , FCentralActivity featureActivity
            , string requiredCapability)
        {
            if (_scheduledActivities.ContainsKey(featureActivity.Key))
            {

                Agent.DebugMessage($"Activity {featureActivity.Key} removed from _scheduledActivities");
                _scheduledActivities.Remove(featureActivity.Key);
            }

            Agent.DebugMessage($"Start activity {featureActivity.ProductionOrderId}|{featureActivity.OperationId}|{featureActivity.ActivityId}!");

            var activity = interval.ProductionorderOperationActivityResource.ProductionorderOperationActivity;


            _confirmationManager.AddConfirmations(activity, GanttConfirmationState.Started, Agent.CurrentTime, Agent.CurrentTime);

            //Capability
            

            var randomizedDuration = _workTimeGenerator.GetRandomWorkTime(featureActivity.Duration);

            CreateSimulationJob(activity, Agent.CurrentTime, randomizedDuration, requiredCapability);

            foreach (var resourceState in requiredResources)
            {

                var fCentralActivity = new FCentralActivity(resourceId: resourceState.ResourceDefinition.Id
                                                    ,productionOrderId: featureActivity.ProductionOrderId
                                                          ,operationId:featureActivity.OperationId
                                                    ,activityId:featureActivity.ActivityId
                                                    , activityType: featureActivity.ActivityType
                                                    , start: Agent.CurrentTime
                                                    , duration: randomizedDuration
                                                    , name:featureActivity.Name
                                                    , ganttPlanningInterval:featureActivity.GanttPlanningInterval
                                                    , hub:featureActivity.Hub
                                                    , capability: requiredCapability);

                var startActivityInstruction = Resource.Instruction.Central
                                                .ActivityStart.Create(activity: fCentralActivity
                                                                       , target: resourceState.ResourceDefinition.AgentRef);
                if(resourceState.ActivityQueue.Peek().GetKey == featureActivity.Key)
                {
                    resourceState.ActivityQueue.Dequeue();
                    _activityManager.AddOrUpdateActivity(activity, resourceState.ResourceDefinition);
                    Agent.Send(startActivityInstruction);
                }
                else
                {
                    Agent.DebugMessage($"{resourceState.ResourceDefinition.Name} has another activity");
                }

            }

        }
        
        public void FinishActivity(FCentralActivity fActivity)
        {
            // Get Resource
            System.Diagnostics.Debug.WriteLine($"Finish {fActivity.ProductionOrderId}|{fActivity.OperationId}|{fActivity.ActivityId} with Duration: {fActivity.Duration} from {Agent.Sender.ToString()}");

            var activity = _resourceManager.GetCurrentActivity(fActivity.ResourceId);
           
            // Add Confirmation
            _activityManager.FinishActivityForResource(activity, fActivity.ResourceId);

            if (_activityManager.ActivityIsFinished(activity.GetKey))
            {
                //Check if productionorder finished
                _confirmationManager.AddConfirmations(activity, GanttConfirmationState.Finished, Agent.CurrentTime, fActivity.Start);

                ProductionOrderFinishCheck(activity);
            }

            // Finish ResourceState
            _resourceManager.FinishActivityAtResource(fActivity.ResourceId);

            //Check If new activities are available
            StartActivities();
        }

        private void ProductionOrderFinishCheck(GptblProductionorderOperationActivity activity )
        {
            var productionOrder = activity.Productionorder;

            foreach (var activityOfProductionOrder in productionOrder.ProductionorderOperationActivities)
            {
                var prodactivity = _activityManager.Activities.SingleOrDefault(x => x.Activity.Equals(activityOfProductionOrder));

                if (prodactivity == null || !prodactivity.ActivityIsFinishedDebug())
                {
                    return;
                }
            }

            Agent.DebugMessage($"Productionorder {productionOrder.ProductionorderId} with MaterialId {productionOrder.MaterialId} finished!");


            // Insert Material
            var storagePosting = new FCentralStockPostings.FCentralStockPosting(productionOrder.MaterialId, (double)productionOrder.QuantityNet);

            var product = _products.SingleOrDefault(x => x.MaterialId.Equals(productionOrder.MaterialId));
            if (product != null)
            {
                //_stockPostingManager.AddInsertStockPosting(product.MaterialId,productionOrder.QuantityNet.Value,productionOrder.QuantityUnitId,GanttStockPostingType.Relatively,Agent.CurrentTime);

                var salesorderId = _SalesorderMaterialrelations.Single(x => x.ChildId.Equals(productionOrder.ProductionorderId)).SalesorderId;

                var salesorder = _salesOrder.Single(x => x.SalesorderId.Equals(salesorderId));

                CreateLeadTime(product, long.Parse(salesorder.Info1));
                
                salesorder.Status = 8;
                salesorder.QuantityDelivered = 1;

                var provideOrder = new FCentralProvideOrders.FCentralProvideOrder(productionOrderId: productionOrder.ProductionorderId
                                                                 ,materialId : product.MaterialId
                                                                ,materialName: product.Name
                                                               , salesOrderId: salesorderId
                                                         , materialFinishedAt: Agent.CurrentTime);
            }

        }

        public override bool AfterInit()
        {
            Agent.Send(Hub.Instruction.Central.StartActivities.Create(Agent.Context.Self), 1);
            return true;
        }

        #region Reporting

        public void CreateSimulationJob(GptblProductionorderOperationActivity activity, long start, long duration, string requiredCapabilityName)
        {
            var pub = activity.ToSimulationJob(start, duration, requiredCapabilityName);
            Agent.Context.System.EventStream.Publish(@event: pub);
        }

        private void CreateLeadTime(GptblMaterial material, long creationTime)
        {
                var pub = new FThroughPutTimes.FThroughPutTime(articleKey: Guid.Empty
                    , articleName: material.Name
                    , start: creationTime  // TODO  ADD CreationTime
                    , end: Agent.CurrentTime);
                Agent.Context.System.EventStream.Publish(@event: pub);
        }
        #endregion

    }
}
