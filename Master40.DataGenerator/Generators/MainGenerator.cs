using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.MasterTableInitializers;
using Master40.DataGenerator.Repository;
using Master40.DataGenerator.Util;
using Master40.DataGenerator.Verification;
using Master40.DB.Data.Context;
using Master40.DB.Data.DynamicInitializer;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.GeneratorModel;
using Newtonsoft.Json;
using MasterTableResourceCapability = Master40.DB.Data.DynamicInitializer.Tables.MasterTableResourceCapability;

namespace Master40.DataGenerator.Generators
{
    public class MainGenerator
    {

        public TransitionMatrix TransitionMatrix { get; set; }
        public ProductStructure ProductStructure { get; set; }
        public MasterTableResourceCapability ResourceCapabilities { get; set; }


        public bool StartGeneration(Approach approach, MasterDBContext dbContext, bool doVerify = false,
            double setupTimeFactor = double.NaN)
        {
            if (approach.UseExistingResourcesData)
            {
                GlobalMasterRepository.DeleteAllButResourceData(dbContext);
                if (ResourceCapabilityRepository.GetParentResourceCapabilitiesCount(dbContext) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("################################# No resource data found although required");
                    return false;
                }

                var resourceCapabilities = ResourceCapabilityRepository.GetParentResourceCapabilities(dbContext);
                approach.TransitionMatrixInput.WorkingStations = new List<WorkingStationParameterSet>();
                foreach (var resource in resourceCapabilities)
                {
                    MachiningTimeParameterSet machiningTime = null;
                    if (resource.MeanMachiningTime != null && resource.VarianceMachiningTime != null)
                    {
                        machiningTime = new MachiningTimeParameterSet
                        {
                            MeanMachiningTime = resource.MeanMachiningTime.Value,
                            VarianceMachiningTime = resource.VarianceMachiningTime.Value
                        };
                    }
                    var workingStation = new WorkingStationParameterSet
                    {
                        MachiningTimeParameterSet = machiningTime
                    };

                    approach.TransitionMatrixInput.WorkingStations.Add(workingStation);
                }
            }
            else
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }

            var rng = new Random(approach.Seed);

            var units = new MasterTableUnit();
            var unitCol = units.Init(dbContext);
            var articleTypes = new MasterTableArticleType();
            articleTypes.Init(dbContext);

            var productStructureGenerator = new ProductStructureGenerator();
            ProductStructure = productStructureGenerator.GenerateProductStructure(approach.ProductStructureInput,
                approach.BomInput, articleTypes, units, unitCol, rng, approach.TransitionMatrixInput);
            ArticleInitializer.Init(ProductStructure.NodesPerLevel, dbContext);

            var articleTable = dbContext.Articles.ToArray();
            MasterTableStock.Init(dbContext, articleTable);

            var transitionMatrixGenerator = new TransitionMatrixGenerator();
            TransitionMatrix = transitionMatrixGenerator.GenerateTransitionMatrix(approach.TransitionMatrixInput,
                approach.ProductStructureInput, rng);
            
            if (approach.UseExistingResourcesData)
            {
                ResourceCapabilities = new MasterTableResourceCapability
                {
                    Capabilities = ResourceCapabilityRepository.GetAllResourceCapabilities(dbContext),
                    ParentCapabilities = ResourceCapabilityRepository.GetParentResourceCapabilities(dbContext)
                };
            }
            else
            {
                List<ResourceProperty> resourceProperties = approach.TransitionMatrixInput.WorkingStations
                    .Select(x => (ResourceProperty)x).ToList();
                ResourceCapabilities = ResourceInitializer.Initialize(dbContext, resourceProperties, approach.TransitionMatrixInput.InfiniteTools);
            }

            var operationGenerator = new OperationGenerator();
            operationGenerator.GenerateOperations(ProductStructure.NodesPerLevel, TransitionMatrix,
                approach.TransitionMatrixInput, ResourceCapabilities, rng);
            OperationInitializer.Init(ProductStructure.NodesPerLevel, dbContext);

            var billOfMaterialGenerator = new BillOfMaterialGenerator();
            billOfMaterialGenerator.GenerateBillOfMaterial(ProductStructure.NodesPerLevel, rng);
            BillOfMaterialInitializer.Init(ProductStructure.NodesPerLevel, dbContext);

            var businessPartner = new MasterTableBusinessPartner();
            businessPartner.Init(dbContext);

            var articleToBusinessPartner = new ArticleToBusinessPartnerInitializer();
            articleToBusinessPartner.Init(dbContext, articleTable, businessPartner);


            if (doVerify)
            {
                var productStructureVerifier = new ProductStructureVerifier();
                productStructureVerifier.VerifyComplexityAndReutilizationRation(approach.ProductStructureInput,
                    ProductStructure);

                var transitionMatrixGeneratorVerifier = new TransitionMatrixGeneratorVerifier();
                transitionMatrixGeneratorVerifier.VerifyGeneratedData(TransitionMatrix, ProductStructure.NodesPerLevel,
                    ResourceCapabilities, approach.TransitionMatrixInput);

                if (!double.IsNaN(setupTimeFactor) && !approach.UseExistingResourcesData)
                {
                    var capacityDemandVerifier = new CapacityDemandVerifier(setupTimeFactor,
                        approach.TransitionMatrixInput.WorkingStations.Count);
                    capacityDemandVerifier.Verify(ProductStructure, approach.TransitionMatrixInput);
                }
                else if (approach.UseExistingResourcesData)
                {
                    System.Diagnostics.Debug.WriteLine("################################# CapacityDemandVerifier not available if existing resources data are used");
                }

                //TEMP BEGIN
                System.Diagnostics.Debug.WriteLine("################################# Generated transition matrix from input:");
                transitionMatrixGenerator.OutputMatrixForExcel(TransitionMatrix.Pi, ResourceCapabilities.ParentCapabilities.Count + (approach.TransitionMatrixInput.ExtendedTransitionMatrix ? 1 : 0 ));
                //TEMP END
            }

            if (approach.UseExistingResourcesData)
            {
                var resourcesData = ResourceSetupRepository.GetAllResourceSetups(dbContext);
                var jsonOutput = JsonConvert.SerializeObject(resourcesData);
                var hash = Sha256Hasher.ComputeSha256Hash(jsonOutput);
                if (!hash.Equals(approach.ResourcesDataHash))
                {
                    System.Diagnostics.Debug.WriteLine("################################# !!! Currently used resources data differs highly likely to those, that were used to create this approach !!!");
                }
            }

            return true;
        }
    }
}