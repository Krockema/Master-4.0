using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public class WorkerGroupDefinition : IDefinition, IGroupDefinition
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<IResourceDefinition> ResourceDefinitions { get; set; }
    }
}
