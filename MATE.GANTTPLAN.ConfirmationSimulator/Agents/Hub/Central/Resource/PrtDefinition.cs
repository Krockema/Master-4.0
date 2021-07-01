using Akka.Actor;
using Mate.DataCore.Nominal.Model;
using System.Collections.Generic;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public class PrtDefinition : IDefinition, IResourceDefinition
    {

        //Interface
        public string Name { get; set; }
        public string Id { get; set; }
        public IActorRef AgentRef { get; set; }
        public List<string> GroupIds { get; set; }
        public ResourceType ResourceType { get; set; }
        
        //Own
        public int IsInfinite { get; set; }

        public PrtDefinition(string name, string id, bool isGroup, IActorRef agentRef, List<string> groupIds, ResourceType resourceType, int isInfinite)
        {
            Name = name;
            Id = id;
            AgentRef = agentRef;
            GroupIds = groupIds;
            ResourceType = resourceType;
            IsInfinite = isInfinite;
        }


    }
}
