using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class BillOfMaterialInput
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int EdgeWeightRoundModeId { get; set; }

        [Range(0.000001, double.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public double WeightEpsilon { get; set; }

        [JsonIgnore]
        public int ApproachId { get; set; }

        [JsonIgnore]
        public Approach Approach { get; set; }

        [JsonProperty(Required = Required.Always)]
        public EdgeWeightRoundMode EdgeWeightRoundMode { get; set; }
    }
}