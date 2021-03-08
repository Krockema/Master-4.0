using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.MasterTableInitializers;
using Master40.DataGenerator.Repository;
using Master40.DataGenerator.Util;
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

        public void StartGeneration(Approach approach, MasterDBContext dbContext, bool doVerify = false, double setupTimeFactor = double.NaN)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var amountOfWorker = 4;

            var rng = new XRandom(approach.Seed);

            var units = new MasterTableUnit();
            var unitCol = units.Init(dbContext);
            var articleTypes = new MasterTableArticleType();
            articleTypes.Init(dbContext);

            var productStructureGenerator = new ProductStructureGenerator();
            var productStructure = productStructureGenerator.GenerateProductStructure(approach.ProductStructureInput,
                articleTypes, units, unitCol, rng);
            ArticleInitializer.Init(productStructure.NodesPerLevel, dbContext);

            var articleTable = dbContext.Articles.ToArray();
            MasterTableStock.Init(dbContext, articleTable);

            var transitionMatrixGenerator = new TransitionMatrixGenerator();
            TransitionMatrix = transitionMatrixGenerator.GenerateTransitionMatrix(approach.TransitionMatrixInput,
                approach.ProductStructureInput, rng);

            List<ResourceProperty> resourceProperties = approach.TransitionMatrixInput.WorkingStations
                .Select(x => (ResourceProperty)x).ToList();

            var resourceCapabilities = ResourceInitializer.Initialize(dbContext, resourceProperties, amountOfWorker);

            var operationGenerator = new OperationGenerator();
            operationGenerator.GenerateOperations(productStructure.NodesPerLevel, TransitionMatrix,
                approach.TransitionMatrixInput, resourceCapabilities, rng);
            OperationInitializer.Init(productStructure.NodesPerLevel, dbContext);

            var billOfMaterialGenerator = new BillOfMaterialGenerator();
            billOfMaterialGenerator.GenerateBillOfMaterial(approach.BomInput, productStructure.NodesPerLevel,
                TransitionMatrix, units, rng);
            BillOfMaterialInitializer.Init(productStructure.NodesPerLevel, dbContext);

            var businessPartner = new MasterTableBusinessPartner();
            businessPartner.Init(dbContext);

            var articleToBusinessPartner = new ArticleToBusinessPartnerInitializer();
            articleToBusinessPartner.Init(dbContext, articleTable, businessPartner);


            if (doVerify)
            {
                var productStructureVerifier = new ProductStructureVerifier();
                productStructureVerifier.VerifyComplexityAndReutilizationRation(approach.ProductStructureInput,
                    productStructure);

                if (approach.TransitionMatrixInput.ExtendedTransitionMatrix)
                {
                    var transitionMatrixGeneratorVerifier = new TransitionMatrixGeneratorVerifier();
                    transitionMatrixGeneratorVerifier.VerifyGeneratedData(TransitionMatrix,
                        productStructure.NodesPerLevel, resourceCapabilities);
                }

                if (!double.IsNaN(setupTimeFactor))
                {
                    var capacityDemandVerifier = new CapacityDemandVerifier(setupTimeFactor);
                    capacityDemandVerifier.Verify(productStructure);
                }
            }
        }
    }
}