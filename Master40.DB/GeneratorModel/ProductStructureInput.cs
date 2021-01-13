using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Master40.DB.GeneratorModel
{
    public class ProductStructureInput
    {
        private PropertyInfo[] _propertyInfos = null;
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int EndProductCount { get; set; }
        public int DepthOfAssembly { get; set; }
        public double ReutilisationRatio { get; set; }
        public double ComplexityRatio { get; set; }
        public double MeanIncomingMaterialAmount { get; set; }
        public double VarianceIncomingMaterialAmount { get; set; }
        public int ApproachId { get; set; }
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
