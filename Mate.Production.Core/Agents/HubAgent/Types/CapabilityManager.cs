﻿using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    public class CapabilityManager
    {
        private List<CapabilityDefinition> _capabilityDefinitions = new List<CapabilityDefinition>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceSetups"></param>
      
        public CapabilityDefinition GetResourcesByCapability(M_ResourceCapability resourceCapability)
        {
            return _capabilityDefinitions.Single(x => x.HasCapability(resourceCapability));
        }

        internal CapabilityDefinition GetCapabilityDefinition(M_ResourceCapability capability)
        {
            var capabilityDefinition =
                _capabilityDefinitions.SingleOrDefault(x => x.ResourceCapability.Id == capability.Id);
            if (capabilityDefinition != null)
            {
                foreach (var capabilityProvider in capability.ResourceCapabilityProvider)
                {
                    capabilityDefinition.AddCapabilityProvider(capabilityProvider);
                }
                return capabilityDefinition;
            }
            // else create a new one
            capabilityDefinition = new CapabilityDefinition(capability);
            _capabilityDefinitions.Add(capabilityDefinition);
            return capabilityDefinition;
        }

        public List<M_ResourceCapabilityProvider> GetAllCapabilityProvider(M_ResourceCapability capability)
        {
            return _capabilityDefinitions.Single(x => x.ResourceCapability.Id == capability.Id).GetAllCapabilityProvider();

        }
        

    }
}
