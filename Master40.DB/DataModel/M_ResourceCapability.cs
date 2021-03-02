using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataModel
{
    /*
     * Previous called MachineGroup, now describes a Capability to do something
     * owns a list of Resources 
     */
    public class M_ResourceCapability : BaseEntity
    {
        public string Name { get; set; }
        public int? ParentResourceCapabilityId { get; set; }
        public double? MeanMachiningTime { get; set; }
        public double? VarianceMachiningTime { get; set; }
        public M_ResourceCapability ParentResourceCapability { get; set; }
        public ICollection<M_ResourceCapability> ChildResourceCapabilities { get; set; }
        public ICollection<M_ResourceCapabilityProvider> ResourceCapabilityProvider { get; set; }
    }
}
