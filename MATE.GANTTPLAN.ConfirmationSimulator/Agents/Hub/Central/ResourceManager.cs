using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource;
using Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent.Types.Central;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central
{
    public class ResourceManager
    {
        //Each Resource (workcenter, prt and worker) has their unique Id 
        public List<ResourceState> resourceStateList { get; } = new List<ResourceState>();

        public ResourceManager()
        {

        }

        public void Add(IResourceDefinition resourceDefinition)
        {
            var resourceState = new ResourceState(resourceDefinition);
            resourceStateList.Add(resourceState);
        }

        public bool ResourceIsWorking(int resourceId)
        {
            return resourceStateList.Single(x => ((IDefinition)x.ResourceDefinition).Id.Equals(resourceId)).IsWorking;
        }

        public bool StartActivityAtResource(int resourceId,
            GptblProductionorderOperationActivity productionorderOperationActivity)
        {
            var resource = resourceStateList.Single(x => ((IDefinition)x.ResourceDefinition).Id.Equals(resourceId));
            if (resource.IsWorking)
            {
                return false;
            }

            resource.StartActivityAtResource(productionorderOperationActivity);

            return resource.IsWorking;
        }

        public void FinishActivityAtResource(int resourceId)
        {
            resourceStateList.Single(x => ((IDefinition)x.ResourceDefinition).Id.Equals(resourceId)).FinishActivityAtResource();
        }

        public GptblProductionorderOperationActivity GetCurrentActivity(int resourceId)
        {
            return resourceStateList.Single(x => ((IDefinition)x.ResourceDefinition).Id.Equals(resourceId))
                .CurrentProductionOrderActivity;
        }

        public static void ShowCurrentWork(List<ResourceState> temresourceStateList)
        {
            var t = temresourceStateList.Where(x => x.GetCurrentProductionOperationActivity != null).ToList();
            foreach (var x in t)
            {
                System.Diagnostics.Debug.WriteLine("" + ((IDefinition)x.ResourceDefinition).Name +
                                                   x.GetCurrentProductionOperationActivity);
            }
        }

        public static void ShowNextWork(List<ResourceState> temresourceStateList)
        {
            foreach (var x in temresourceStateList)
            {
                if (x.GetCurrentProductionOperationActivity != null)
                {

                    System.Diagnostics.Debug.WriteLine("Has activity in currentWork" + x.ResourceDefinition.Name +
                                                       x.GetCurrentProductionOperationActivity);
                }
                else
                {
                    if (x.ActivityQueue.Count < 1)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"NextQueueItem {x.ResourceDefinition.Name} has no activity in queue");

                        continue;
                    }
                    var nextQueueItem = x.ActivityQueue.Peek();
                    
                    System.Diagnostics.Debug.WriteLine(
                        $"NextQueueItem {x.ResourceDefinition.Name}|{nextQueueItem.ProductionorderId}|{nextQueueItem.OperationId}|{nextQueueItem.ActivityId}");
                   
                }
            }
        }

    }
}
