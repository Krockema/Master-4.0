using Akka.Actor;
using Mate.DataCore.Nominal.Model;
using Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource;
using Mate.Production.Core.Helper;
using System.Collections.Generic;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.HubAgent.Types.Central
{
    public class WorkcenterDefinition : IDefinition, IResourceDefinition
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public IActorRef AgentRef { get; set; }
        public ResourceType ResourceType { get; set; }
        public List<string> GroupIds { get; set; }

        public WorkcenterDefinition(string name, string id, IActorRef actorRef, List<string> groupIds, ResourceType resourceType)
        {
            Name = name.ToActorName();
            Id = id;
            AgentRef = actorRef;
            GroupIds = groupIds;
            ResourceType = resourceType;
        }


    }
}
