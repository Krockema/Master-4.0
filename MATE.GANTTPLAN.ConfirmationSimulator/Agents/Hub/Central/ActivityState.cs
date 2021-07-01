using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    public class ActivityState
    {
        public GptblProductionorderOperationActivity Activity { get; private set; }
        
        public List<Confirmation> Confirmations { get; private set; } = new List<Confirmation>();
        public bool ActivityIsFinished() => Confirmations.TrueForAll(x => x.IsFinished);

        public ActivityState(GptblProductionorderOperationActivity activity, ResourceDefinition resourceDefinition)
        {
            var confirmation = new Confirmation(resourceDefinition);
            Confirmations.Add(confirmation);
            Activity = activity;
        }

        public void AddResource(ResourceDefinition resourceDefinition)
        {
            var confirmation = new Confirmation(resourceDefinition);
            Confirmations.Add(confirmation);
        }

        public void FinishForResource(int resourceId)
        {
            var confirmation = Confirmations.Single(x => x.ResourceDefinition.Id.Equals(resourceId));
            confirmation.State = GanttConfirmationState.Finished;
        }

        public bool ActivityIsFinishedDebug()
        {
            System.Diagnostics.Debug.WriteLine($"Check activities for {GetActivityAsString()} with {Confirmations.Count} required confirmations");
            
            foreach (var confirmation in Confirmations)
            {
                if (!confirmation.IsFinished)
                {
                    System.Diagnostics.Debug.WriteLine($"Confirmation for {GetActivityAsString()} {confirmation.ResourceDefinition.Name} not finished yet");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Confirmation for {GetActivityAsString()} {confirmation.ResourceDefinition.Name} finished");

            }

            System.Diagnostics.Debug.WriteLine($"All Confirmation for activity {GetActivityAsString()} have finished");

            return true;

        }

        public string GetActivityAsString()
        {
            return Activity.ProductionorderId + "|" + Activity.OperationId + "|" + Activity.ActivityId;
        }

    }
}