using Akka.Actor;
using Mate.DataCore.Nominal.Model;
using System.Collections.Generic;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public class WorkerDefinition : IDefinition, IResourceDefinition
    {
        public WorkerDefinition(string name, string id, IActorRef agentRef, List<string> groupIds, ResourceType resourceType)
        {
            Name = name;
            Id = id;
            AgentRef = agentRef;
            GroupIds = groupIds;
            ResourceType = resourceType;
        }
        public string Name { get; set; }
        public string Id { get; set; }
        public IActorRef AgentRef { get; set; }
        public List<string> GroupIds { get; set; }
        public ResourceType ResourceType { get; set; }
    }
}
