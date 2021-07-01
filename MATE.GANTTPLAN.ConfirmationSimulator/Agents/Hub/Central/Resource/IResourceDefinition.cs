using Akka.Actor;
using Mate.DataCore.Nominal.Model;
using System.Collections.Generic;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public interface IResourceDefinition
    {
        public IActorRef AgentRef { get; set; }
        public List<string> GroupIds { get; set; }
        public ResourceType ResourceType { get; set; }
        public string Name => ((IDefinition)this).Name;
        public string Id => ((IDefinition)this).Id;
    }
}
