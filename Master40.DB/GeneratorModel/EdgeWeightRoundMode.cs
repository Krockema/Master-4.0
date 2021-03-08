using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class EdgeWeightRoundMode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<BillOfMaterialInput> BomInputs { get; set; }
    }
}