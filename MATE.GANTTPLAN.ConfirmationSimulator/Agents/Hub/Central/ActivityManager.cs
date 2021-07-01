using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal.Model;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    /// <summary>
    /// Trackes Activities and Confirmations
    /// Confirmations: general and foreach resource
    /// Confirmations States: Start, Partial, Finished
    /// </summary>
    public class ActivityManager
    {

        public List<ActivityState> Activities = new List<ActivityState>();

        public void AddOrUpdateActivity(GptblProductionorderOperationActivity activity, ResourceDefinition resourceDefinition)
        {
            var currentActivityState = Activities.SingleOrDefault(x => x.Activity.GetKey.Equals(activity.GetKey));

            //if this is the forst activity
            if (currentActivityState == null)
            {
                currentActivityState = new ActivityState(activity, resourceDefinition);
                Activities.Add(currentActivityState);
                return;
            }

            currentActivityState.AddResource(resourceDefinition);

        }

        public void FinishActivityForResource(GptblProductionorderOperationActivity activity, int resourceId)
        {
            Activities.SingleOrDefault(x => x.Activity.GetKey.Equals(activity.GetKey))?.FinishForResource(resourceId);
        }

        public bool ActivityIsFinished(string activityKey)
        {
            return Activities.Single(x => x.Activity.GetKey.Equals(activityKey)).ActivityIsFinishedDebug();
        }

        public GptblProductionorderOperationActivity GetActivity(string activityKey)
        {
            return Activities.SingleOrDefault(x => x.Activity.GetKey.Equals(activityKey))?.Activity;

        }

        internal bool HasPreconditionsFullfilled(GptblProductionorderOperationActivity activity, List<ResourceState> resourceStates)
        {
            if (Int32.Parse(activity.OperationId) > 10)
            {
                var predecessor = Activities.SingleOrDefault(x =>
                    x.Activity.GetKey.Equals(activity.ProductionorderId + "|" + (Int32.Parse(activity.OperationId) - 10) + "|" + "3"));

                if (predecessor == null || !predecessor.ActivityIsFinished())
                {
                    return false;
                }

            }

            // check if Production Precondition is fulfilled to start setup.
            if (activity.ActivityType.Equals(2))
            {
                var activityQueue = resourceStates.Single(x => x.ResourceDefinition.ResourceType == ResourceType.Workcenter).ActivityQueue;
                activity = activityQueue.Single(x => x.GetKey.Equals(activity.ProductionorderId + "|" + activity.OperationId + "|" + "3"))
                                            .ProductionorderOperationActivityResource
                                                .ProductionorderOperationActivity;
            }

            foreach (var requiredPrecondition in activity.ProductionorderOperationActivityMaterialrelation)
            {

                switch (requiredPrecondition.MaterialrelationType)
                {
                    //ProductionOrder
                    case 2:

                        System.Diagnostics.Debug.WriteLine(
                            $"{activity.ProductionorderId}|{activity.OperationId}|{activity.ActivityId} require {requiredPrecondition.ChildId}|{requiredPrecondition.ChildOperationId}|{requiredPrecondition.ChildActivityId} ");
                      

                        if (!Activities.Exists(x => x.Activity.GetKey == requiredPrecondition.GetChildKey)
                            || !ActivityIsFinished(requiredPrecondition.GetChildKey))
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Precondition {requiredPrecondition.ChildId}|{requiredPrecondition.ChildOperationId}|{requiredPrecondition.ChildActivityId} for {activity.ProductionorderId}|{activity.OperationId}|{activity.ActivityId} is not fulfilled");

                            return false;
                        }

                        break;
                    case 8:
                        // ingore buy meterials for now
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Materialtype does not exits!");
                        break;
                }

            }

            System.Diagnostics.Debug.WriteLine(
                $"All preconditions for {activity.ProductionorderId}|{activity.OperationId}|{activity.ActivityId} fulfilled");

            return true;
        }
    }
}
