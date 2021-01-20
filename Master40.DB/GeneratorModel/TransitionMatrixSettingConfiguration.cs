using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class TransitionMatrixSettingConfiguration
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int SettingOptionId { get; set; }

        [JsonIgnore]
        public int TransitionMatrixId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public double SettingValue { get; set; }

        [JsonProperty(Required = Required.Always)]
        public TransitionMatrixSettingOption SettingOption { get; set; }

        [JsonIgnore]
        public TransitionMatrixInput TransitionMatrix { get; set; }
    }
}