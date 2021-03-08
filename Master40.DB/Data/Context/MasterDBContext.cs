using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Master40.DB.Data.Context
{
    public class MasterDBContext : DbContext
    {
        public static MasterDBContext GetContext(string connectionString)
        {
            return new MasterDBContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                .UseSqlServer(connectionString: connectionString)
                .Options);
        }
        public MasterDBContext(DbContextOptions<MasterDBContext> options) : base(options: options) { }
        [JsonIgnore]
        public bool InMemory { get; internal set; }
        public DbSet<M_Article> Articles { get; set; }
        public DbSet<M_ArticleBom> ArticleBoms { get; set; }
        public DbSet<M_ArticleType> ArticleTypes { get; set; }
        public DbSet<M_ArticleToBusinessPartner> ArticleToBusinessPartners { get; set; }
        public DbSet<M_BusinessPartner> BusinessPartners { get; set; }
        public DbSet<M_Resource> Resources { get; set; }
        public DbSet<M_ResourceCapability> ResourceCapabilities { get; set; }
        public DbSet<M_ResourceCapabilityProvider> ResourceCapabilityProviders { get; set; } //das
        public DbSet<M_ResourceSetup> ResourceSetups { get; set; } //und das
        public DbSet<M_Stock> Stocks { get; set; }
        public DbSet<M_Unit> Units { get; set; }
        public DbSet<M_Operation> Operations { get; set; }
        public DbSet<T_DemandToProvider> DemandToProviders { get; set; }
        public DbSet<T_ProviderToDemand> ProviderToDemand { get; set; }
        public DbSet<T_CustomerOrder> CustomerOrders { get; set; }
        public DbSet<T_CustomerOrderPart> CustomerOrderParts { get; set; }
        public DbSet<T_PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<T_ProductionOrder> ProductionOrders { get; set; }
        public DbSet<T_PurchaseOrderPart> PurchaseOrderParts { get; set; }
        public DbSet<T_ProductionOrderBom> ProductionOrderBoms { get; set; }
        public DbSet<T_ProductionOrderOperation> ProductionOrderOperations { get; set; }
        public DbSet<T_StockExchange> StockExchanges { get; set; }
        public DbSet<M_Attribute> Attributes { get; set; }
        public DbSet<M_Characteristic> Characteristics { get; set; }
        public DbSet<T_MeasurementValue> MeasurementValues { get; set; }
        public DbSet<M_ValueType> ValueTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M_Article>()
                .ToTable(name: "M_Article")
                .HasOne(navigationExpression: a => a.Stock)
                .WithOne(navigationExpression: s => s.Article)
                .HasForeignKey<M_Stock>(foreignKeyExpression: b => b.ArticleForeignKey);

            modelBuilder.Entity<M_ArticleToBusinessPartner>()
                .ToTable(name: "M_ArticleToBusinessPartner")
                .HasAlternateKey(keyExpression: x => new { x.ArticleId, x.BusinessPartnerId });

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable(name: "M_ArticleBom")
                .HasOne(navigationExpression: pt => pt.ArticleParent)
                .WithMany(navigationExpression: p => p.ArticleBoms)
                .HasForeignKey(foreignKeyExpression: pt => pt.ArticleParentId)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable(name: "M_ArticleBom")
                .HasOne(navigationExpression: pt => pt.ArticleChild)
                .WithMany(navigationExpression: t => t.ArticleChilds)
                .HasForeignKey(foreignKeyExpression: pt => pt.ArticleChildId);

            modelBuilder.Entity<M_ArticleBom>()
                .ToTable(name: "M_ArticleBom")
                .HasOne(navigationExpression: pt => pt.Operation)
                .WithMany(navigationExpression: t => t.ArticleBoms)
                .HasForeignKey(foreignKeyExpression: pt => pt.OperationId)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

            modelBuilder.Entity<M_Operation>()
                .ToTable(name: "M_Operation")
                .HasOne(navigationExpression: m => m.ResourceCapability);

            modelBuilder.Entity<M_ArticleType>()
                .ToTable(name: "M_ArticleType");

            modelBuilder.Entity<M_BusinessPartner>()
                .ToTable(name: "M_BusinessPartner");

            modelBuilder.Entity<M_ArticleToBusinessPartner>()
                .ToTable(name: "M_ArticleToBusinessPartner");

            modelBuilder.Entity<M_Resource>()
                .ToTable(name: "M_Resource");

           modelBuilder.Entity<M_ResourceCapability>()
               .ToTable(name: "M_ResourceCapability")
               .HasOne(navigationExpression: p => p.ParentResourceCapability)
               .WithMany(navigationExpression: pe => pe.ChildResourceCapabilities)
               .HasForeignKey(foreignKeyExpression: fk => fk.ParentResourceCapabilityId)
               .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

           //'FK_M_ResourceCapability_M_ResourceCapability_ParentResourceCapabilityId' on table 'M_ResourceCapability'
           //may cause cycles or multiple cascade paths.Specify ON DELETE NO ACTION or ON UPDATE NO ACTION, or modify other FOREIGN KEY constraints.

            modelBuilder.Entity<M_Stock>()
                .ToTable(name: "M_Stock");

            modelBuilder.Entity<M_Unit>()
                .ToTable(name: "M_Unit");

            modelBuilder.Entity<M_Operation>()
                .ToTable(name: "M_Operation");

            modelBuilder.Entity<T_PurchaseOrder>()
                .ToTable(name: "T_PurchaseOrder");

            modelBuilder.Entity<T_PurchaseOrderPart>()
                .ToTable(name: "T_PurchaseOrderPart");

            modelBuilder.Entity<T_CustomerOrder>()
                .ToTable(name: "T_CustomerOrder");

            modelBuilder.Entity<T_CustomerOrderPart>()
                .ToTable(name: "T_CustomerOrderPart");

            modelBuilder.Entity<T_ProductionOrder>()
                .ToTable(name: "T_ProductionOrder");

            modelBuilder.Entity<T_StockExchange>()
                .ToTable(name: "T_StockExchange");

            modelBuilder.Entity<M_ResourceCapability>()
                .ToTable(name: "M_ResourceCapability");

            modelBuilder.Entity<M_ResourceCapabilityProvider>()
                .ToTable(name: "M_ResourceCapabilityProvider");

            modelBuilder.Entity<M_ResourceSetup>()
                .ToTable(name: "M_ResourceSetup");


            modelBuilder.Entity<T_ProductionOrderBom>()
                .ToTable(name: "T_ProductionOrderBom")
                .HasOne(navigationExpression: p => p.ProductionOrderParent)
                .WithMany(navigationExpression: b => b.ProductionOrderBoms)
                .HasForeignKey(foreignKeyExpression: fk => fk.ProductionOrderParentId)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

            modelBuilder.Entity<T_ProductionOrderBom>()
                .ToTable(name: "T_ProductionOrderBom")
                .HasOne(navigationExpression: p => p.ProductionOrderOperation)
                .WithMany(navigationExpression: b => b.ProductionOrderBoms)
                .HasForeignKey(foreignKeyExpression: fk => fk.ProductionOrderOperationId)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

            modelBuilder.Entity<T_ProductionOrderOperation>()
                .ToTable(name: "T_ProductionOrderOperation");

            modelBuilder.Entity<T_DemandToProvider>()
                .ToTable("T_DemandToProvider");
            modelBuilder.Entity<T_ProviderToDemand>()
                .ToTable("T_ProviderToDemand");
            modelBuilder.Entity<M_Characteristic>()
                .ToTable("M_Characteristic")
                .HasOne(navigationExpression: c => c.Operation)
                .WithMany(o => o.Characteristics)
                .HasForeignKey(fk => fk.OperationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<M_Attribute>()
                .ToTable("M_Attribute");
            modelBuilder.Entity<T_MeasurementValue>()
                .ToTable("T_MeasurementValue");
            modelBuilder.Entity<M_ValueType>()
                .ToTable("M_ValueType");
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}