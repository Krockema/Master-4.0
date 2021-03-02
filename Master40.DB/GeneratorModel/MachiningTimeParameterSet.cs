using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class MachiningTimeParameterSet
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [Range(1.0, double.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public double MeanMachiningTime { get; set; }

        [JsonProperty(Required = Required.Always)]
        [Range(0.0, double.MaxValue)]
        public double VarianceMachiningTime { get; set; }

        [JsonIgnore]
        public WorkingStationParameterSet WorkingStation { get; set; }

        [JsonIgnore]
        public TransitionMatrixInput TransitionMatrix { get; set; }
    }
}