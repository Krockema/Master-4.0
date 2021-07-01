using System.Collections.Generic;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent.Types.Central
{
    public class ResourceState
    {
        public IResourceDefinition ResourceDefinition { get; private set; }
        public GptblProductionorderOperationActivity CurrentProductionOrderActivity { get; private set; }
        
        public bool IsWorking => CurrentProductionOrderActivity != null;
        
        public Queue<GptblProductionorderOperationActivityResourceInterval> ActivityQueue { get; set; }

        public string GetCurrentProductionOperationActivity => CurrentProductionOrderActivity != null ? $"ProductionOrderId: {CurrentProductionOrderActivity.ProductionorderId} " +
                                                                                                        $"| Operation: {CurrentProductionOrderActivity.OperationId} " +
                                                                                                        $"| Activity {CurrentProductionOrderActivity.ActivityId}"  
                                                                                                        : null;

        public ResourceState(IResourceDefinition resourceDefinition)
        {
            ResourceDefinition = resourceDefinition;
            CurrentProductionOrderActivity = null;
        }

        internal void StartActivityAtResource(GptblProductionorderOperationActivity productionorderOperationActivity)
        { 
            CurrentProductionOrderActivity = productionorderOperationActivity;
        }

        internal void FinishActivityAtResource()
        {
            ResetActivityAtResource();
        }

        internal void ResetActivityAtResource()
        {
            CurrentProductionOrderActivity = null;
        }


    }
}
