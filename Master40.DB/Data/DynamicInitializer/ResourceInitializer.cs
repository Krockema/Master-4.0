using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Data.DynamicInitializer.Tables;

namespace Master40.DB.Data.DynamicInitializer
{
    public class ResourceInitializer
    {
        public static MasterTableResourceCapability Initialize(MasterDBContext context,
            List<ResourceProperty> resourceProperties, bool infiniteTools)
        {
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.CreateCapabilities(context, resourceProperties);

            var resources = new MasterTableResource(resourceCapabilities, infiniteTools);
            resources.CreateModel(resourceProperties);
            resources.CreateResourceTools(resourceProperties);

            resources.SaveToDB(context);
            return resourceCapabilities;
        }
    }
}