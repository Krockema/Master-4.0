using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Master40.DB.Data.DynamicInitializer
{
    public class ResourceProperty
    {
        [Range(1, int.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public int ToolCount { get; set; }
        [Range(1, int.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public int ResourceCount { get; set; }
        [Range(1, int.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public int SetupTime { get; set; }
        [Range(0, int.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public int OperatorCount { get; set; }
    }
}