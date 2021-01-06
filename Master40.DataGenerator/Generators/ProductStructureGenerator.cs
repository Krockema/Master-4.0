using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Util;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.DataModel;
using Master40.DB.GeneratorModel;
using MathNet.Numerics.Distributions;

namespace Master40.DataGenerator.Generators
{
    public class ProductStructureGenerator
    {
        // Wie könnte man Testen, ob der Algorithmus dem aus SYMTEP enspricht (keine Fehler enthält)
        public ProductStructure GenerateProductStructure(ProductStructureInput inputParameters,
            BillOfMaterialInput bomInput, MasterTableArticleType articleTypes, MasterTableUnit units, M_Unit[] unitCol,
            Random rng)
        {
            var productStructure = new ProductStructure();
            var availableNodes = new List<HashSet<int>>();
            GenerateParts(inputParameters, productStructure, availableNodes, articleTypes, units, unitCol, rng);

            GenerateEdges(inputParameters, productStructure, rng, availableNodes);

            DeterminationOfEdgeWeights(inputParameters, bomInput, productStructure, units, rng);

            return productStructure;
        }

        private List<KeyValuePair<int, double>> GetCumulatedProbabilitiesPk1(int i)
        {
            var pk = new List<KeyValuePair<int, double>>();
            for (var k = 1; k < i; k++)
            {
                pk.Add(new KeyValuePair<int, double>(k, 2 * k / Convert.ToDouble(i * (i - 1))));
            }
            pk.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
            {
                if (o1.Value > o2.Value) return -1;
                return 1;
            });
            return pk;
        }

        private Dictionary<int, List<KeyValuePair<int, double>>> GetSetOfCumulatedProbabilitiesPk1(int depthOfAssembly)
        {
            var pkPerI = new Dictionary<int, List<KeyValuePair<int, double>>>();
            for (var i = 2; i <= depthOfAssembly; i++)
            {
                pkPerI[i] = GetCumulatedProbabilitiesPk1(i);
            }
            return pkPerI;
        }

        private List<KeyValuePair<int, double>> GetCumulatedProbabilitiesPk2(int i, int depthOfAssembly)
        {
            var pk = new List<KeyValuePair<int, double>>();
            for (var k = i + 1; k <= depthOfAssembly; k++)
            {
                pk.Add(new KeyValuePair<int, double>(k,
                    2 * (k - i) / Convert.ToDouble((depthOfAssembly - i) * (depthOfAssembly - i + 1))));
            }
            pk.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
            {
                if (o1.Value > o2.Value) return -1;
                return 1;
            });
            return pk;
        }

        private void GenerateParts(ProductStructureInput inputParameters, ProductStructure productStructure,
            List<HashSet<int>> availableNodes, MasterTableArticleType articleTypes, MasterTableUnit units,
            M_Unit[] unitCol, Random rng)
        {
            bool sampleWorkPlanLength = inputParameters.MeanWorkPlanLength != null &&
                                        inputParameters.VarianceWorkPlanLength != null;
            TruncatedDiscreteNormal truncatedDiscreteNormalDistribution = null;
            if (sampleWorkPlanLength)
            {
                truncatedDiscreteNormalDistribution = new TruncatedDiscreteNormal(1, null,
                    Normal.WithMeanVariance((double) inputParameters.MeanWorkPlanLength,
                        (double) inputParameters.VarianceWorkPlanLength, rng));
            }
            for (var i = 1; i <= inputParameters.DepthOfAssembly; i++)
            {
                productStructure.NodesCounter += GeneratePartsForEachLevel(inputParameters, productStructure,
                    availableNodes, articleTypes, units, unitCol, rng, i, sampleWorkPlanLength,
                    truncatedDiscreteNormalDistribution);
            }
        }

        private static int GeneratePartsForEachLevel(ProductStructureInput inputParameters,
            ProductStructure productStructure, List<HashSet<int>> availableNodes, MasterTableArticleType articleTypes,
            MasterTableUnit units, M_Unit[] unitCol, Random rng, int i, bool sampleWorkPlanLength,
            TruncatedDiscreteNormal truncatedDiscreteNormalDistribution)
        {
            //Problem mit Algorithmus aus SYMTEP: bei ungünstigen Eingabeparametern gibt es auf manchen Fertigungsstufen keine Teile (0 Knoten)
            //-> Es fehlt wohl Nebenbedingung, dass Anzahl an Teilen auf jeden Fertigungsstufe mindestens 1 sein darf
            //-> Entsprechend wurde das hier angepasst
            var nodeCount = Math.Max(1, (int) CalculateAmountOfPartsForGivenLevel(i, inputParameters));
            var nodesCurrentLevel = new List<Node>();
            productStructure.NodesPerLevel.Add(nodesCurrentLevel);
            var availableNodesOnThisLevel = new HashSet<int>();
            availableNodes.Add(availableNodesOnThisLevel);

            bool toPurchase, toBuild;
            M_Unit unit = null;
            M_ArticleType articleType;
            if (i == 1)
            {
                toPurchase = false;
                toBuild = true;
                unit = units.PIECES;
                articleType = articleTypes.PRODUCT;
            }
            else if (i == inputParameters.DepthOfAssembly)
            {
                toPurchase = true;
                toBuild = false;
                articleType = articleTypes.MATERIAL;
            }
            else
            {
                toPurchase = false;
                toBuild = true;
                unit = units.PIECES;
                articleType = articleTypes.ASSEMBLY;
            }

            for (var j = 0; j < nodeCount; j++)
            {
                unit = GeneratePartsForCurrentLevel(inputParameters, unitCol, rng, i, sampleWorkPlanLength,
                    truncatedDiscreteNormalDistribution, availableNodesOnThisLevel, j, unit, articleType, toPurchase,
                    toBuild, nodesCurrentLevel);
            }

            return nodeCount;
        }

        private static M_Unit GeneratePartsForCurrentLevel(ProductStructureInput inputParameters, M_Unit[] unitCol,
            Random rng, int i, bool sampleWorkPlanLength, TruncatedDiscreteNormal truncatedDiscreteNormalDistribution,
            HashSet<int> availableNodesOnThisLevel, int j, M_Unit unit, M_ArticleType articleType, bool toPurchase,
            bool toBuild, List<Node> nodesCurrentLevel)
        {
            availableNodesOnThisLevel.Add(j);

            if (i == inputParameters.DepthOfAssembly)
            {
                var pos = rng.Next(unitCol.Length);
                unit = unitCol[pos];
            }

            var node = new Node
            {
                AssemblyLevel = i,
                Article = new M_Article
                {
                    Name = "Material " + i + "." + (j + 1),
                    ArticleTypeId = articleType.Id,
                    CreationDate = DateTime.Now,
                    DeliveryPeriod = 5,
                    UnitId = unit.Id,
                    Price = 10,
                    ToPurchase = toPurchase,
                    ToBuild = toBuild
                }
            };
            nodesCurrentLevel.Add(node);
            if (sampleWorkPlanLength && i != inputParameters.DepthOfAssembly)
            {
                node.WorkPlanLength = truncatedDiscreteNormalDistribution.Sample();
            }

            return unit;
        }

        private void GenerateEdges(ProductStructureInput inputParameters, ProductStructure productStructure, Random rng,
            List<HashSet<int>> availableNodes)
        {
            var nodesOfLastAssemblyLevelCounter =
                productStructure.NodesPerLevel[inputParameters.DepthOfAssembly - 1].Count;
            var edgeCount = (int) Math.Round(Math.Max(
                inputParameters.ReutilisationRatio * (productStructure.NodesCounter - inputParameters.EndProductCount),
                inputParameters.ComplexityRatio * (productStructure.NodesCounter - nodesOfLastAssemblyLevelCounter)));
            var pkPerI = GetSetOfCumulatedProbabilitiesPk1(inputParameters.DepthOfAssembly);
            if (inputParameters.ReutilisationRatio < inputParameters.ComplexityRatio)
            {
                GenerateFirstSetOfEdgesForConvergingMaterialFlow(inputParameters, rng, pkPerI, availableNodes, productStructure);
            }
            else
            {
                GenerateFirstSetOfEdgesForDivergingMaterialFlow(inputParameters, productStructure, rng, availableNodes);
            }

            //scheinbar können hierbei Multikanten entstehen. ist das in Erzeugnisstruktur erlaubt? -> stellt kein Problem dar
            GenerateSecondSetOfEdges(inputParameters, productStructure, rng, edgeCount, pkPerI);
        }

        private void GenerateFirstSetOfEdgesForConvergingMaterialFlow(ProductStructureInput inputParameters, Random rng,
            Dictionary<int, List<KeyValuePair<int, double>>> pkPerI, List<HashSet<int>> availableNodes,
            ProductStructure productStructure)
        {
            for (var i = 1; i <= inputParameters.DepthOfAssembly - 1; i++)
            {
                for (var j = 1; j <= productStructure.NodesPerLevel[i - 1].Count; j++)
                {
                    var startNodePos = rng.Next(availableNodes[i].Count);
                    var startNode = availableNodes[i].ToArray()[startNodePos];
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[i][startNode],
                        End = productStructure.NodesPerLevel[i - 1][j - 1]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                    availableNodes[i].Remove(startNode);
                }
            }

            for (var i = inputParameters.DepthOfAssembly; i >= 2; i--)
            {
                foreach (var j in availableNodes[i - 1])
                {
                    var u = rng.NextDouble();
                    var sum = 0.0;
                    var k = 0;
                    while (k < pkPerI[i].Count - 1)
                    {
                        sum += pkPerI[i][k].Value;
                        if (u < sum)
                        {
                            break;
                        }

                        k++;
                    }

                    var assemblyLevelOfEndNode = pkPerI[i][k].Key;
                    var posOfNode = rng.Next(productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1].Count);
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[i - 1][j],
                        End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][posOfNode]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                }
            }
        }

        private void GenerateFirstSetOfEdgesForDivergingMaterialFlow(ProductStructureInput inputParameters,
            ProductStructure productStructure, Random rng, List<HashSet<int>> availableNodes)
        {
            for (var i = inputParameters.DepthOfAssembly; i >= 2; i--)
            {
                for (var j = 1; j <= productStructure.NodesPerLevel[i - 1].Count; j++)
                {
                    var endNodePos = rng.Next(availableNodes[i - 2].Count);
                    var endNode = availableNodes[i - 2].ToArray()[endNodePos];
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[i - 1][j - 1],
                        End = productStructure.NodesPerLevel[i - 2][endNode]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                    availableNodes[i - 2].Remove(endNode);
                }
            }

            for (var i = 1; i < inputParameters.DepthOfAssembly; i++)
            {
                var pk = GetCumulatedProbabilitiesPk2(i, inputParameters.DepthOfAssembly);
                foreach (var j in availableNodes[i - 1])
                {
                    var u = rng.NextDouble();
                    var sum = 0.0;
                    var k = 0;
                    while (k < pk.Count - 1)
                    {
                        sum += pk[k].Value;
                        if (u < sum)
                        {
                            break;
                        }

                        k++;
                    }

                    var assemblyLevelOfStartNode = pk[k].Key;
                    var posOfNode = rng.Next(productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].Count);
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][posOfNode],
                        End = productStructure.NodesPerLevel[i - 1][j]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                }
            }
        }

        private static void GenerateSecondSetOfEdges(ProductStructureInput inputParameters, ProductStructure productStructure,
            Random rng, int edgeCount, Dictionary<int, List<KeyValuePair<int, double>>> pkPerI)
        {
            var possibleStartNodes = productStructure.NodesCounter - inputParameters.EndProductCount;
            for (var j = productStructure.Edges.Count + 1; j <= edgeCount; j++)
            {
                var startNodePos = rng.Next(possibleStartNodes) + 1;
                var assemblyLevelOfStartNode = 2;
                while (assemblyLevelOfStartNode < inputParameters.DepthOfAssembly)
                {
                    if (startNodePos <= productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].Count)
                    {
                        break;
                    }

                    startNodePos -= productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].Count;
                    assemblyLevelOfStartNode++;
                }

                var u = rng.NextDouble();
                var sum = 0.0;
                var k = 0;
                while (k < pkPerI[assemblyLevelOfStartNode].Count - 1)
                {
                    sum += pkPerI[assemblyLevelOfStartNode][k].Value;
                    if (u < sum)
                    {
                        break;
                    }

                    k++;
                }

                var assemblyLevelOfEndNode = pkPerI[assemblyLevelOfStartNode][k].Key;
                var endNodePos = rng.Next(productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1].Count);
                var edge = new Edge
                {
                    Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][startNodePos - 1],
                    End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][endNodePos]
                };
                edge.End.IncomingEdges.Add(edge);
                productStructure.Edges.Add(edge);
            }
        }

        private static void DeterminationOfEdgeWeights(ProductStructureInput inputParameters,
            BillOfMaterialInput bomInput, ProductStructure productStructure, MasterTableUnit units, Random rng)
        {
            var logNormalDistribution = LogNormal.WithMeanVariance(inputParameters.MeanIncomingMaterialAmount,
                Math.Pow(inputParameters.StdDevIncomingMaterialAmount, 2), rng);
            var edgeWeightRoundModes = new DataGeneratorTableEdgeWeightRoundMode();
            foreach (var edge in productStructure.Edges)
            {
                var weight = logNormalDistribution.Sample();
                if (edgeWeightRoundModes.ROUND_ALWAYS.Name == bomInput.EdgeWeightRoundMode.Name)
                {
                    edge.Weight = Math.Max(1, Math.Round(weight));
                }
                else if (edgeWeightRoundModes.ROUND_IF_IT_MAKES_SENSE.Name == bomInput.EdgeWeightRoundMode.Name)
                {
                    if (edge.Start.Article.UnitId == units.PIECES.Id)
                    {
                        edge.Weight = Math.Max(1, Math.Round(weight));
                    }
                    else
                    {
                        edge.Weight = Math.Max(bomInput.WeightEpsilon, weight);
                    }
                }
                else if (edgeWeightRoundModes.ROUND_NEVER.Name == bomInput.EdgeWeightRoundMode.Name)
                {
                    edge.Weight = Math.Max(bomInput.WeightEpsilon, weight);
                }
            }
        }

        public static bool DeterminateMaxDepthOfAssemblyAndCheckLimit(ProductStructureInput input)
        {
            var total = BigInteger.Zero;
            for (var i = 1; i <= input.DepthOfAssembly; i++)
            {
                var result = new BigInteger(CalculateAmountOfPartsForGivenLevel(i, input));
                //wegen double beginnen die Werte, wenn sie etwa 17-stellig werden, vom richtigen Ergebnis abzuweichen
                //das ist aber nicht so schlimm, da die Größenordnung in etwa gewahrt wird und overflows nicht auftreten
                if (result.Equals(BigInteger.Zero))
                {
                    input.DepthOfAssembly = i - 1;
                    break;
                }

                total += result;
            }

            return total <= 100000;
        }

        public static double CalculateAmountOfPartsForGivenLevel(int level, ProductStructureInput input)
        {
            return Math.Round(Math.Pow(input.ComplexityRatio / input.ReutilisationRatio, level - 1) *
                              input.EndProductCount);
        }
    }
}
