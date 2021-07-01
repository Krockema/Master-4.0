using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.Hub.Central.Resource
{
    public interface IDefinition
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}
