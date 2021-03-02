using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class TransitionMatrixSettingOption
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [Required]
        [JsonIgnore]
        public string Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<TransitionMatrixSettingConfiguration> SettingConfigurations { get; set; }
    }
}