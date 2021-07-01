using System.Collections.Generic;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public class WorkcenterGroupDefinition : IDefinition, IGroupDefinition
    {
        public WorkcenterGroupDefinition(string name, string id, List<IResourceDefinition> resourceDefinitions)
        {
            Name = name;
            Id = id;
            ResourceDefinitions = resourceDefinitions;
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public List<IResourceDefinition> ResourceDefinitions { get; set; }

    }
}
