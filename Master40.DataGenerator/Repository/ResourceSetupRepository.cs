using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DataGenerator.Repository
{
    public class ResourceSetupRepository
    {

        public static List<M_ResourceSetup> GetAllResourceSetups(MasterDBContext ctx)
        {
            var result = ctx.ResourceSetups
                .Include(rs => rs.Resource)
                .Include(rs => rs.ResourceCapabilityProvider)
                    .ThenInclude(rcp => rcp.ResourceCapability)
                        .ThenInclude(crc => crc.ParentResourceCapability)
                .ToList();
            result.ForEach(x =>
            {
                x.Resource.ResourceSetups.Clear();
                x.ResourceCapabilityProvider.ResourceSetups.Clear();
                x.ResourceCapabilityProvider.ResourceCapability.ResourceCapabilityProvider.Clear();
                x.ResourceCapabilityProvider.ResourceCapability.ParentResourceCapability.ChildResourceCapabilities.Clear();
            });
            return result;
        }

    }
}