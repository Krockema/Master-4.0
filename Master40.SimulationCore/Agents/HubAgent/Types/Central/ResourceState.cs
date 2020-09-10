﻿using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Master40.DB.GanttPlanModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ResourceState
    {
        public string Name { get; private set;  }

        public string Id { get; private set; }

        public IActorRef AgentRef { get; private set; }

        public GptblProductionorderOperationActivity CurrentProductionOrderActivity { get; private set; }
        
        public bool IsWorking => CurrentProductionOrderActivity != null;

        public bool FinishedWork { get; private set; }

        public string GetCurrentProductionOperationActivity => CurrentProductionOrderActivity != null ? $"ProductionOrderId: {CurrentProductionOrderActivity.ProductionorderId} " +
                                                                                                        $"| Operation: {CurrentProductionOrderActivity.OperationId} " +
                                                                                                        $"| Activity {CurrentProductionOrderActivity.ActivityId}"  
                                                                                                        : null;

        public ResourceState(string name, string id, IActorRef agentRef)
        {
            Name = name;
            Id = id;
            AgentRef = agentRef;
            CurrentProductionOrderActivity = null;
        }

        internal void StartActivityAtResource(GptblProductionorderOperationActivity productionorderOperationActivity)
        {
            CurrentProductionOrderActivity = productionorderOperationActivity;
        }

        internal void FinishActivityAtResource()
        {
            FinishedWork = true;
        }

        internal void ResetActivityAtResource()
        {
            CurrentProductionOrderActivity = null;
        }


    }
}
