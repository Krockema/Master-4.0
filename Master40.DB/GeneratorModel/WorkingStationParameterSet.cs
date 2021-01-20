using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Data.DynamicInitializer;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class WorkingStationParameterSet : ResourceProperty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int? MachiningTimeId { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public MachiningTimeParameterSet MachiningTimeParameterSet { get; set; }

        [JsonIgnore]
        public int TransitionMatrixInputId { get; set; }

        [JsonIgnore]
        public TransitionMatrixInput TransitionMatrixInput { get; set; }

    }
}