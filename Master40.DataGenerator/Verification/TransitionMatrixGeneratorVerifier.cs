using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Master40.DB.Data.Context;
using Master40.DB.Data.DynamicInitializer.Tables;
using Master40.DB.GeneratorModel;
using Master40.DB.Util;

namespace Master40.DataGenerator.Verification
{
    public class TransitionMatrixGeneratorVerifier
    {

        public double? ActualOrganizationDegree;
        public double? GeneratedOrganizationDegree;

        public void VerifyGeneratedData(TransitionMatrix transitionMatrix, List<List<Node>> nodesPerLevel,
            MasterTableResourceCapability capabilities, TransitionMatrixInput transitionMatrixInput, bool noOutput = false)
        {

            var matrixSizeCorrection = 0;
            if (transitionMatrixInput.ExtendedTransitionMatrix)
            {
                matrixSizeCorrection++;
            }

            var actualTransitionMatrix = new TransitionMatrix
            {
                Pi = new double[capabilities.ParentCapabilities.Count + matrixSizeCorrection, capabilities.ParentCapabilities.Count + matrixSizeCorrection]
            };
            foreach (var article in nodesPerLevel.SelectMany(_ => _).Where(x => x.AssemblyLevel < nodesPerLevel.Count))
            {
                var operationCount = 0;
                var lastCapPos = 0;
                do
                {
                    var capPos = capabilities.ParentCapabilities.FindIndex(x =>
                        object.ReferenceEquals(x,
                            article.Operations[operationCount].MOperation.ResourceCapability.ParentResourceCapability));
                    if (transitionMatrixInput.ExtendedTransitionMatrix || operationCount != 0)
                    {
                        actualTransitionMatrix.Pi[lastCapPos, capPos]++;
                    }
                    lastCapPos = capPos + matrixSizeCorrection;
                    operationCount++;
                } while (operationCount < article.Operations.Count);

                if (transitionMatrixInput.ExtendedTransitionMatrix)
                {
                    actualTransitionMatrix.Pi[lastCapPos, capabilities.ParentCapabilities.Count]++;
                }
            }

            for (var i = 0; i < capabilities.ParentCapabilities.Count + matrixSizeCorrection; i++)
            {
                var sum = 0.0;
                for (var j = 0; j < capabilities.ParentCapabilities.Count + matrixSizeCorrection; j++)
                {
                    sum += actualTransitionMatrix.Pi[i, j];
                }

                for (var j = 0; j < capabilities.ParentCapabilities.Count + matrixSizeCorrection; j++)
                {
                    actualTransitionMatrix.Pi[i, j] /= sum;
                }
            }

            var transitionMatrixGenerator = new TransitionMatrixGenerator();
            ActualOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(actualTransitionMatrix.Pi,
                capabilities.ParentCapabilities.Count + matrixSizeCorrection);
            GeneratedOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(transitionMatrix.Pi,
                capabilities.ParentCapabilities.Count + matrixSizeCorrection);

            if (!noOutput)
            {
                System.Diagnostics.Debug.WriteLine("################################# Generated work plans have an organization degree of " + ActualOrganizationDegree + " (transition matrix has " + GeneratedOrganizationDegree + "; input was " + transitionMatrixInput.DegreeOfOrganization + ")");

                System.Diagnostics.Debug.WriteLine("################################# Generated transition matrix from input:");
                transitionMatrixGenerator.OutputMatrixForExcel(transitionMatrix.Pi, capabilities.ParentCapabilities.Count + matrixSizeCorrection);
                System.Diagnostics.Debug.WriteLine("################################# Actual transition matrix from generated work plans:");
                transitionMatrixGenerator.OutputMatrixForExcel(actualTransitionMatrix.Pi, capabilities.ParentCapabilities.Count + matrixSizeCorrection);

                System.Diagnostics.Debug.WriteLine("################################# Lambda: " + transitionMatrixInput.Lambda);
            }
        }

        public void VerifySimulatedData(MasterDBContext dbContext, DataGeneratorContext dbGeneratorCtx,
            ResultContext dbResultCtx, int simNumber)
        {
            var simulation = SimulationRepository.GetSimulationById(simNumber, dbGeneratorCtx);
            if (simulation != null)
            {
                var approach = ApproachRepository.GetApproachById(dbGeneratorCtx, simulation.ApproachId);
                var generator = new MainGenerator();
                if (generator.StartGeneration(approach, dbContext))
                {

                    System.Diagnostics.Debug.WriteLine("################################# Verifying simulation #" + simNumber + " (approach " + approach.Id + ")");
                    System.Diagnostics.Debug.WriteLine("################################# Predefined reutilization ratio: " + approach.ProductStructureInput.ReutilisationRatio);

                    var articleCount =
                        ArticleRepository.GetArticleNamesAndCountForEachUsedArticleInSimulation(dbResultCtx, simNumber);

                    var articlesByNames =
                        ArticleRepository.GetArticlesByNames(articleCount.Keys.ToHashSet(), dbContext);
                    var capabilities = ResourceCapabilityRepository.GetParentResourceCapabilities(dbContext);

                    var matrixSizeCorrection = 0;
                    if (approach.TransitionMatrixInput.ExtendedTransitionMatrix)
                    {
                        matrixSizeCorrection++;
                    }

                    var actualTransitionMatrix = new TransitionMatrix
                    {
                        Pi = new double[capabilities.Count + matrixSizeCorrection,
                            capabilities.Count + matrixSizeCorrection]
                    };

                    var capPosByCapId = new Dictionary<int, int>();
                    foreach (var cap in capabilities)
                    {
                        var number = cap.Name.Substring(0, cap.Name.IndexOf(" "));
                        var pos = AlphabeticNumbering.GetNumericRepresentation(number);
                        capPosByCapId.Add(cap.Id, pos);
                    }

                    foreach (var a in articlesByNames)
                    {
                        var operations = a.Value.Operations.ToList();
                        operations.Sort((o1, o2) => o1.HierarchyNumber.CompareTo(o2.HierarchyNumber));

                        var operationCount = 0;
                        var lastCapPos = 0;
                        do
                        {
                            var capPos =
                                capPosByCapId[
                                    operations[operationCount].ResourceCapability.ParentResourceCapability.Id];
                            if (approach.TransitionMatrixInput.ExtendedTransitionMatrix || operationCount != 0)
                            {
                                actualTransitionMatrix.Pi[lastCapPos, capPos] += articleCount[a.Key];
                            }

                            lastCapPos = capPos + matrixSizeCorrection;
                            operationCount++;
                        } while (operationCount < operations.Count);

                        if (approach.TransitionMatrixInput.ExtendedTransitionMatrix)
                        {
                            actualTransitionMatrix.Pi[lastCapPos, capabilities.Count] += articleCount[a.Key];
                        }
                    }

                    for (var i = 0; i < capabilities.Count + matrixSizeCorrection; i++)
                    {
                        var sum = 0.0;
                        for (var j = 0; j < capabilities.Count + matrixSizeCorrection; j++)
                        {
                            sum += actualTransitionMatrix.Pi[i, j];
                        }

                        for (var j = 0; j < capabilities.Count + matrixSizeCorrection; j++)
                        {
                            actualTransitionMatrix.Pi[i, j] /= sum;
                        }
                    }

                    var transitionMatrixGenerator = new TransitionMatrixGenerator();
                    ActualOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(
                        actualTransitionMatrix.Pi,
                        capabilities.Count + matrixSizeCorrection);
                    GeneratedOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(
                        generator.TransitionMatrix.Pi,
                        capabilities.Count + matrixSizeCorrection);

                    var generatedOG = new TransitionMatrixGeneratorVerifier();
                    generatedOG.VerifyGeneratedData(generator.TransitionMatrix, generator.ProductStructure.NodesPerLevel,
                        generator.ResourceCapabilities, approach.TransitionMatrixInput, true);

                    System.Diagnostics.Debug.WriteLine(
                        "################################# Executed work plans have an organization degree of " +
                        ActualOrganizationDegree + " (generated work plans have " +
                        generatedOG.ActualOrganizationDegree + "; transition matrix has " +
                        GeneratedOrganizationDegree + "; input was " +
                        approach.TransitionMatrixInput.DegreeOfOrganization + ")");

                    System.Diagnostics.Debug.WriteLine(
                        "################################# Actual transition matrix from executed work plans in simulation:");
                    transitionMatrixGenerator.OutputMatrixForExcel(actualTransitionMatrix.Pi,
                        capabilities.Count + matrixSizeCorrection);
                }
            }
        }
    }
}