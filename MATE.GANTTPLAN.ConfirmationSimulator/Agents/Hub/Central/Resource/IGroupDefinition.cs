using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public interface IGroupDefinition
    {
        public List<IResourceDefinition> ResourceDefinitions { get; set; }

        public string Name => ((IDefinition)this).Name;
        public string Id => ((IDefinition)this).Id;
    }
}
