using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.Repository
{
    public class ResourceCapabilityRepository
    {
        public static List<M_ResourceCapability> GetAllResourceCapabilities(MasterDBContext ctx)
        {
            return ctx.ResourceCapabilities.ToList();
        }
        public static List<M_ResourceCapability> GetParentResourceCapabilities(MasterDBContext ctx)
        {
            return ctx.ResourceCapabilities.Include(x => x.ChildResourceCapabilities)
                .Where(x => x.ParentResourceCapabilityId == null).ToList();
        }

        public static int GetParentResourceCapabilitiesCount(MasterDBContext ctx)
        {
            return ctx.ResourceCapabilities.Count(x => x.ParentResourceCapabilityId == null);
        }

    }
}