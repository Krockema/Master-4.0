using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.MasterTableInitializers;
using Master40.DataGenerator.Verification;
using Master40.DB.Data.Context;
using Master40.DB.Data.DynamicInitializer;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.GeneratorModel;

namespace Master40.DataGenerator.Generators
{
    public class MainGenerator
    {

        public TransitionMatrix TransitionMatrix { get; set; }
        public ProductStructure ProductStructur { get; set; }
        public Master40.DB.Data.DynamicInitializer.Tables.MasterTableResourceCapability ResourceCapabilities { get; set; }


        public void StartGeneration(Approach approach, MasterDBContext dbContext, bool doVerify = false, double setupTimeFactor = double.NaN)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var rng = new Random(approach.GetSeed());

            var units = new MasterTableUnit();
            var unitCol = units.Init(dbContext);
            var articleTypes = new MasterTableArticleType();
            articleTypes.Init(dbContext);

            var productStructureGenerator = new ProductStructureGenerator();
            ProductStructur = productStructureGenerator.GenerateProductStructure(approach.ProductStructureInput,
                approach.BomInput, articleTypes, units, unitCol, rng, approach.TransitionMatrixInput);
            ArticleInitializer.Init(ProductStructur.NodesPerLevel, dbContext);

            var articleTable = dbContext.Articles.ToArray();
            MasterTableStock.Init(dbContext, articleTable);

            var transitionMatrixGenerator = new TransitionMatrixGenerator();
            TransitionMatrix = transitionMatrixGenerator.GenerateTransitionMatrix(approach.TransitionMatrixInput,
                approach.ProductStructureInput, rng);

            List<ResourceProperty> resourceProperties = approach.TransitionMatrixInput.WorkingStations
                .Select(x => (ResourceProperty)x).ToList();

            ResourceCapabilities = ResourceInitializer.Initialize(dbContext, resourceProperties);

            var operationGenerator = new OperationGenerator();
            operationGenerator.GenerateOperations(ProductStructur.NodesPerLevel, TransitionMatrix,
                approach.TransitionMatrixInput, ResourceCapabilities, rng);
            OperationInitializer.Init(ProductStructur.NodesPerLevel, dbContext);

            var billOfMaterialGenerator = new BillOfMaterialGenerator();
            billOfMaterialGenerator.GenerateBillOfMaterial(ProductStructur.NodesPerLevel, rng);
            BillOfMaterialInitializer.Init(ProductStructur.NodesPerLevel, dbContext);

            var businessPartner = new MasterTableBusinessPartner();
            businessPartner.Init(dbContext);

            var articleToBusinessPartner = new ArticleToBusinessPartnerInitializer();
            articleToBusinessPartner.Init(dbContext, articleTable, businessPartner);


            if (doVerify)
            {
                var productStructureVerifier = new ProductStructureVerifier();
                productStructureVerifier.VerifyComplexityAndReutilizationRation(approach.ProductStructureInput,
                    ProductStructur);

                var transitionMatrixGeneratorVerifier = new TransitionMatrixGeneratorVerifier();
                transitionMatrixGeneratorVerifier.VerifyGeneratedData(TransitionMatrix, ProductStructur.NodesPerLevel,
                    ResourceCapabilities, approach.TransitionMatrixInput);

                if (!double.IsNaN(setupTimeFactor))
                {
                    var capacityDemandVerifier = new CapacityDemandVerifier(setupTimeFactor,
                        approach.TransitionMatrixInput.WorkingStations.Count);
                    capacityDemandVerifier.Verify(ProductStructur, approach.TransitionMatrixInput);
                }

                //TEMP BEGIN
                System.Diagnostics.Debug.WriteLine("################################# Generated transition matrix from input:");
                transitionMatrixGenerator.OutputMatrixForExcel(TransitionMatrix.Pi, ResourceCapabilities.ParentCapabilities.Count + (approach.TransitionMatrixInput.ExtendedTransitionMatrix ? 1 : 0 ));
                //TEMP END
            }
        }
    }
}