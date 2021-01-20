using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class ProductStructureInput
    {
        private PropertyInfo[] _propertyInfos = null;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public int EndProductCount { get; set; }

        [Range(2, int.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public int DepthOfAssembly { get; set; }

        [Range(1.0, double.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public double ReutilisationRatio { get; set; }

        [Range(1.0, double.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public double ComplexityRatio { get; set; }

        [JsonProperty(Required = Required.Always)]
        public double MeanIncomingMaterialAmount { get; set; }

        [Range(0.0, double.MaxValue)]
        [JsonProperty(Required = Required.Always)]
        public double VarianceIncomingMaterialAmount { get; set; }

        [JsonIgnore]
        public int ApproachId { get; set; }

        [JsonIgnore]
        public Approach Approach { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder("[" + base.GetType().ToString() + "]" + "\n");
            _propertyInfos ??= this.GetType().GetProperties();

            foreach (var info in _propertyInfos)
            {
                var value = info.GetValue(this, null) ?? "(null)";
                sb.AppendLine("\t" + info.Name + ": " + value.ToString());
            }

            sb.AppendLine("");
            return sb.ToString();
        }
    }

}
