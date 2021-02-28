using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class Approach
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public DateTime CreationDate { get; set; }

        [JsonIgnore]
        public int Seed { get; set; }

        [NotMapped]
        [Range(0, int.MaxValue)]
        [JsonProperty(Required = Required.AllowNull)]
        public int? PresetSeed { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool UseExistingResourcesData { get; set; }

        [CanBeNull]
        public string ResourcesDataHash { get; set; }

        [JsonIgnore]
        public virtual ICollection<Simulation> Simulations { get; set; }

        [JsonProperty(Required = Required.Always)]
        public BillOfMaterialInput BomInput { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ProductStructureInput ProductStructureInput { get; set; }

        [JsonProperty(Required = Required.Always)]
        public TransitionMatrixInput TransitionMatrixInput { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if (TransitionMatrixInput.WorkingStations.Count(x => x == null) != 0)
            {
                throw new Exception("Elements of TransitionMatrixInput.WorkingStations must not be null");
            }

            if (TransitionMatrixInput.SettingConfiguration.Count(x => x == null) != 0)
            {
                throw new Exception("Elements of TransitionMatrixInput.SettingConfiguration must not be null");
            }

            if ((TransitionMatrixInput.WorkingStations.Count(x => x.MachiningTimeParameterSet == null) > 0 ||
                 UseExistingResourcesData) && TransitionMatrixInput.GeneralMachiningTimeParameterSet == null)
            {
                if (UseExistingResourcesData)
                {
                    throw new Exception("You need to set a general machining time as a backup to use existing resources data in case these parameters aren't provided there");
                }
                throw new Exception(
                    "You need to set an individual machining time for each working station or set a general machining time");
            }

            if (!TransitionMatrixInput.ExtendedTransitionMatrix && (TransitionMatrixInput.MeanWorkPlanLength == null ||
                                                                    TransitionMatrixInput.VarianceWorkPlanLength ==
                                                                    null))
            {
                throw new Exception(
                    "You need to set mean and variance of work plan length if you want to use the non extended transition matrix");
            }
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            PresetSeed = Seed;
        }
    }
}