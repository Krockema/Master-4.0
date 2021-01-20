using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.GeneratorModel;

namespace Master40.DataGenerator.Verification
{
    public class CapacityDemandVerifier
    {
        private readonly double _setupTimeFactor;
        private readonly int _machineGroupCount;
        private readonly Dictionary<int, Tuple<double, double[]>> _calculatedCapacityDemandByArticleId = new Dictionary<int, Tuple<double, double[]>>();

        public CapacityDemandVerifier(double setupTimeFactor, int machineGroupCount)
        {
            _setupTimeFactor = setupTimeFactor;
            _machineGroupCount = machineGroupCount;
        }

        public void Verify(ProductStructure productStructure, TransitionMatrixInput transitionMatrixInput)
        {
            var sumCapacity = 0.0;
            var utilizationPerMachineGroup = new double[_machineGroupCount];
            foreach (var endProduct in productStructure.NodesPerLevel[0])
            {
                var resultForEndProduct = CalculateCapacityDemandForArticle(endProduct);
                System.Diagnostics.Debug.WriteLine("Endprodukt " + endProduct.Article.Name + ": " + resultForEndProduct);
                sumCapacity += resultForEndProduct.Item1;
                for (var i = 0; i < utilizationPerMachineGroup.Length; i++)
                {
                    utilizationPerMachineGroup[i] += resultForEndProduct.Item2[i];
                }
            }

            var workingStationsArr = transitionMatrixInput.WorkingStations.ToArray();
            var averageCapacityDemandPerEndProduct = sumCapacity / productStructure.NodesPerLevel[0].Count;
            System.Diagnostics.Debug.WriteLine("################################# Actual average capacity demand per end product is " + averageCapacityDemandPerEndProduct);
            var utilizationSum = 0.0;
            for (var i = 0; i < utilizationPerMachineGroup.Length; i++)
            {
                utilizationPerMachineGroup[i] = utilizationPerMachineGroup[i] / (workingStationsArr[i].ResourceCount * sumCapacity);
                utilizationSum += utilizationPerMachineGroup[i];
            }

            var utilizationPerMachineGroupArr = utilizationPerMachineGroup.Select(x => x / utilizationSum).ToArray();
            System.Diagnostics.Debug.WriteLine("################################# Relative utilization for each machine group: " + string.Join(" # ", utilizationPerMachineGroupArr));
        }

        private Tuple<double, double[]> CalculateCapacityDemandForArticle(Node article)
        {
            if (_calculatedCapacityDemandByArticleId.ContainsKey(article.Article.Id))
            {
                return _calculatedCapacityDemandByArticleId[article.Article.Id];
            }

            var utilizationPerMachineGroup = new double[_machineGroupCount];

            var timeToProduce = 0.0;
            foreach (var op in article.Operations)
            {
                var localResult = op.MOperation.Duration + (op.SetupTimeOfCapability * _setupTimeFactor);
                timeToProduce += localResult;
                utilizationPerMachineGroup[op.InternMachineGroupIndex] += localResult;
            }

            foreach (var incEdge in article.IncomingEdges)
            {
                if (incEdge.Start.Article.ToBuild)
                {
                    var resultOfRecursion = CalculateCapacityDemandForArticle(incEdge.Start);
                    timeToProduce += incEdge.Weight * resultOfRecursion.Item1;
                    for (var i = 0; i < utilizationPerMachineGroup.Length; i++)
                    {
                        utilizationPerMachineGroup[i] += incEdge.Weight * resultOfRecursion.Item2[i];
                    }
                }
            }

            var result = new Tuple<double, double[]>(timeToProduce, utilizationPerMachineGroup);
            _calculatedCapacityDemandByArticleId.Add(article.Article.Id, result);
            return result;
        }

    }
}