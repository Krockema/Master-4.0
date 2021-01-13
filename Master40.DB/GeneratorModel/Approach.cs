using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.GeneratorModel
{
    public class Approach
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        [Required]
        public int? Seed { get; set; }
        public virtual ICollection<Simulation> Simulations { get; set; }
        public BillOfMaterialInput BomInput { get; set; }
        public ProductStructureInput ProductStructureInput { get; set; }
        public TransitionMatrixInput TransitionMatrixInput { get; set; }

        public int GetSeed()
        {
            return Seed ?? 0;
        }
    }
}