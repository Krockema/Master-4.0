using System;
using System.Collections.Generic;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    public class ConfirmationManager
    {
        private string _dbConnectionStringGanttPlan { get; }
        public List<GptblConfirmation> _confirmations { get; private set; } = new List<GptblConfirmation>();
        public List<GptblConfirmationResource> _confirmationsResources { get; private set; } = new List<GptblConfirmationResource>();

        public ConfirmationManager(string dbConnectionStringGanttPlan)
        {
            _dbConnectionStringGanttPlan = dbConnectionStringGanttPlan;
        }

        public void TransferConfirmations()
        {
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                localGanttPlanDbContext.GptblConfirmation.AddRange(_confirmations);
                localGanttPlanDbContext.GptblConfirmationResource.AddRange(_confirmationsResources);
                localGanttPlanDbContext.SaveChanges();
                
            }

            ResetConfirmations();
        }

        public void ResetConfirmations()
        {
            _confirmations.Clear();
            _confirmationsResources.Clear();
        }

        public void AddConfirmations(GptblProductionorderOperationActivity activity, GanttConfirmationState confirmationType, long currentTime, long activityStart)
        {
            var confirmationId = Guid.NewGuid().ToString();
            AddConfirmation(activity, confirmationType, confirmationId, currentTime, activityStart);
            // only finish !?
            /*if (confirmationType.NotEqual(GanttState.Finished))
                return;
            */
            foreach (var resource in activity.ProductionorderOperationActivityResources)
            {
                AddResourceConfirmation(resource, confirmationId);
            }
        }

        public void AddConfirmation(GptblProductionorderOperationActivity activity, GanttConfirmationState confirmationType, string confirmationId, long currentTime, long activityStart)
        {
            var confirmation = new GptblConfirmation();

            confirmation.ClientId = string.Empty;
            confirmation.ConfirmationId = confirmationId;
            confirmation.Info1 = string.Empty;
            confirmation.Info2 = string.Empty;
            confirmation.Info3 = string.Empty;
            confirmation.Name = activity.Name;
            confirmation.ActivityEnd = confirmationType == GanttConfirmationState.Finished ? currentTime.ToNullableDateTime() : null; 
            confirmation.ActivityStart = confirmationType == GanttConfirmationState.Finished ? activityStart.ToNullableDateTime() : currentTime.ToNullableDateTime();
            confirmation.ConfirmationType = (int)confirmationType;
            confirmation.ConfirmationDate = currentTime.ToNullableDateTime();
            confirmation.ProductionorderActivityId = activity.ActivityId;
            confirmation.ProductionorderId = activity.ProductionorderId;
            confirmation.ProductionorderOperationId = activity.OperationId;
            confirmation.ProductionorderSplitId = 0;
            confirmation.ProductionorderAlternativeId = string.Empty;
            confirmation.QuantityFinished = confirmationType == GanttConfirmationState.Finished ? 100 : 0;
            confirmation.QuantityFinishedUnitId = "%";
            confirmation.LastModified = currentTime.ToNullableDateTime();

          _confirmations.Add(confirmation);
        }

        public void AddResourceConfirmation(GptblProductionorderOperationActivityResource resource, string confirmationId)
        {
            var confirmationResource = new GptblConfirmationResource();
            confirmationResource.ClientId = string.Empty;
            confirmationResource.ConfirmationId = confirmationId;
            confirmationResource.ResourceId = resource.ResourceId;
            confirmationResource.ResourceType = resource.ResourceType;
            confirmationResource.GroupId = resource.GroupId;
            confirmationResource.Allocation = 100;

        _confirmationsResources.Add(confirmationResource);
        }

    }
}
