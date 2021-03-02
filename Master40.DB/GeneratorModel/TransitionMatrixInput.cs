using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class TransitionMatrixInput
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [Range(0.0,1.0)]
        [JsonProperty(Required = Required.Always)]
        public double DegreeOfOrganization { get; set; }

        [Range(0.000001, double.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public double Lambda { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool ExtendedTransitionMatrix { get; set; }

        [JsonIgnore]
        public int? GeneralMachiningTimeId { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public MachiningTimeParameterSet GeneralMachiningTimeParameterSet { get; set; }

        [MinLength(1)]
        [JsonProperty(Required = Required.Always)]
        public virtual ICollection<WorkingStationParameterSet> WorkingStations { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool InfiniteTools { get; set; }

        [JsonIgnore]
        public int ApproachId { get; set; }

        [Range(1.0, double.MaxValue)]
        [JsonProperty(Required = Required.AllowNull)]
        public double? MeanWorkPlanLength { get; set; }

        [Range(0.0, double.MaxValue)]
        [JsonProperty(Required = Required.AllowNull)]
        public double? VarianceWorkPlanLength { get; set; }

        [JsonIgnore]
        public Approach Approach { get; set; }

        [JsonProperty(Required = Required.Always)]
        public virtual ICollection<TransitionMatrixSettingConfiguration> SettingConfiguration { get; set; }
    }
}