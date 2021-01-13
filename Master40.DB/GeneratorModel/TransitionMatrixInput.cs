using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.GeneratorModel
{
    public class TransitionMatrixInput
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public double DegreeOfOrganization { get; set; }
        public double Lambda { get; set; }
        public bool ExtendedTransitionMatrix { get; set; }
        public int? GeneralMachiningTimeId { get; set; }
        public MachiningTimeParameterSet GeneralMachiningTimeParameterSet { get; set; }
        public virtual ICollection<WorkingStationParameterSet> WorkingStations { get; set; }
        public bool InfiniteTools { get; set; }
        public int ApproachId { get; set; }
        public double? MeanWorkPlanLength { get; set; }
        public double? VarianceWorkPlanLength { get; set; }
        public Approach Approach { get; set; }
        public virtual ICollection<TransitionMatrixSettingConfiguration> SettingConfiguration { get; set; }
    }
}