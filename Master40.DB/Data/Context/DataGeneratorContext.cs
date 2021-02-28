using Master40.DB.GeneratorModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Master40.DB.Data.Context
{
    public class DataGeneratorContext : DbContext
    {
        public static DataGeneratorContext GetContext(string connectionString)
        {
            return new DataGeneratorContext(new DbContextOptionsBuilder<DataGeneratorContext>()
                .UseSqlServer(connectionString: connectionString)
                .Options);
        }

        public DataGeneratorContext(DbContextOptions<DataGeneratorContext> options) : base(options: options)
        {
        }

        [JsonIgnore] public bool InMemory { get; internal set; }
        public DbSet<Approach> Approaches { get; set; }
        public DbSet<Simulation> Simulations { get; set; }
        public DbSet<BillOfMaterialInput> BomInputs { get; set; }
        public DbSet<MachiningTimeParameterSet> MachiningTimes { get; set; }
        public DbSet<ProductStructureInput> ProductStructureInputs { get; set; }
        public DbSet<TransitionMatrixInput> TransitionMatrixInputs { get; set; }
        public DbSet<WorkingStationParameterSet> WorkingStations { get; set; }
        public DbSet<EdgeWeightRoundMode> EdgeWeightRoundModes { get; set; }
        public DbSet<TransitionMatrixSettingOption> TransitionMatrixSettingOptions { get; set; }
        public DbSet<TransitionMatrixSettingConfiguration> TransitionMatrixSettingConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Approach>()
                .ToTable("Approach")
                .HasCheckConstraint("TransitionMatrixInput_CK_ResourcesData", "(UseExistingResourcesData = 0 AND ResourcesDataHash IS NULL) OR (UseExistingResourcesData = 1 AND ResourcesDataHash IS NOT NULL)");
            modelBuilder.Entity<Simulation>()
                .ToTable("Simulation")
                .HasOne(sim => sim.Approach)
                .WithMany(a => a.Simulations)
                .HasForeignKey(sim => sim.ApproachId);
            modelBuilder.Entity<Simulation>()
                .ToTable("Simulation")
                .Property(sim => sim.FinishTime)
                .HasDefaultValue(null);
            modelBuilder.Entity<Simulation>()
                .ToTable("Simulation")
                .Property(sim => sim.FinishedSuccessfully)
                .HasDefaultValue(false);
            modelBuilder.Entity<BillOfMaterialInput>()
                .ToTable("BomInput")
                .HasOne(bom => bom.Approach)
                .WithOne(a => a.BomInput)
                .HasForeignKey<BillOfMaterialInput>(bom => bom.ApproachId);
            modelBuilder.Entity<BillOfMaterialInput>()
                .ToTable("BomInput")
                .HasOne(bom => bom.EdgeWeightRoundMode)
                .WithMany(ewrm => ewrm.BomInputs)
                .HasForeignKey(bom => bom.EdgeWeightRoundModeId);
            modelBuilder.Entity<ProductStructureInput>()
                .ToTable("ProductStructureInput")
                .HasOne(psi => psi.Approach)
                .WithOne(a => a.ProductStructureInput)
                .HasForeignKey<ProductStructureInput>(psi => psi.ApproachId);
            modelBuilder.Entity<TransitionMatrixInput>()
                .ToTable("TransitionMatrixInput")
                .HasOne(tmi => tmi.Approach)
                .WithOne(a => a.TransitionMatrixInput)
                .HasForeignKey<TransitionMatrixInput>(tmi => tmi.ApproachId);
            modelBuilder.Entity<TransitionMatrixInput>()
                .ToTable("TransitionMatrixInput")
                .HasOne(tmi => tmi.GeneralMachiningTimeParameterSet)
                .WithOne(mt => mt.TransitionMatrix)
                .HasForeignKey<TransitionMatrixInput>(tmi => tmi.GeneralMachiningTimeId);
            modelBuilder.Entity<WorkingStationParameterSet>()
                .ToTable("WorkingStation")
                .HasOne(ws => ws.TransitionMatrixInput)
                .WithMany(tmi => tmi.WorkingStations)
                .HasForeignKey(ws => ws.TransitionMatrixInputId);
            modelBuilder.Entity<WorkingStationParameterSet>()
                .ToTable("WorkingStation")
                .HasOne(ws => ws.MachiningTimeParameterSet)
                .WithOne(mt => mt.WorkingStation)
                .HasForeignKey<WorkingStationParameterSet>(ws => ws.MachiningTimeId);
            modelBuilder.Entity<MachiningTimeParameterSet>()
                .ToTable("MachiningTime");
            modelBuilder.Entity<EdgeWeightRoundMode>()
                .ToTable("EdgeWeightRoundMode")
                .HasIndex(ewrm => ewrm.Name)
                .IsUnique();
            modelBuilder.Entity<TransitionMatrixSettingOption>()
                .ToTable("TransitionMatrixSettingOption")
                .HasIndex(tms => tms.Name)
                .IsUnique();
            modelBuilder.Entity<TransitionMatrixSettingConfiguration>()
                .ToTable("TransitionMatrixSettingConfiguration")
                .HasIndex(tmsc => new {tmsc.SettingOptionId, tmsc.TransitionMatrixId})
                .IsUnique();
            modelBuilder.Entity<TransitionMatrixSettingConfiguration>()
                .ToTable("TransitionMatrixSettingConfiguration")
                .HasOne(tmsc => tmsc.SettingOption)
                .WithMany(tmso => tmso.SettingConfigurations)
                .HasForeignKey(tmsc => tmsc.SettingOptionId);
            modelBuilder.Entity<TransitionMatrixSettingConfiguration>()
                .ToTable("TransitionMatrixSettingConfiguration")
                .HasOne(tmsc => tmsc.TransitionMatrix)
                .WithMany(tmi => tmi.SettingConfiguration)
                .HasForeignKey(tmsc => tmsc.TransitionMatrixId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.EnableSensitiveDataLogging();
    }
}