using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Util;
using Master40.DB.Data.DynamicInitializer.Tables;
using Master40.DB.Data.Helper.Types;
using Master40.DB.DataModel;
using Master40.DB.GeneratorModel;
using MathNet.Numerics.Distributions;

namespace Master40.DataGenerator.Generators
{
    public class OperationGenerator
    {

        private readonly List<List<KeyValuePair<int, double>>> _cumulatedProbabilities = new List<List<KeyValuePair<int, double>>>();
        private int _matrixSize;
        private readonly List<TruncatedDiscreteNormal> _machiningTimeDistributions = new List<TruncatedDiscreteNormal>();
        private WorkingStationParameterSet[] _workingStations;

        public void GenerateOperations(List<List<Node>> nodesPerLevel, TransitionMatrix transitionMatrix,
            TransitionMatrixInput inputTransitionMatrix, MasterTableResourceCapability resourceCapabilities, Random rng)
        {
            Prepare(transitionMatrix, inputTransitionMatrix, rng);

            List<TEnumerator<M_ResourceCapability>> tools = resourceCapabilities.ParentCapabilities.Select(x =>
                    new TEnumerator<M_ResourceCapability>(x.ChildResourceCapabilities.ToArray())).ToList();

            foreach (var article in nodesPerLevel.SelectMany(_ => _).Where(x => x.AssemblyLevel < nodesPerLevel.Count))
            {
                var hierarchyNumber = 0;
                var currentWorkingMachine = inputTransitionMatrix.ExtendedTransitionMatrix
                    ? DetermineNextWorkingMachine(0, rng)
                    : rng.Next(tools.Count);
                var lastOperationReached = false;
                var operationCount = 0;
                var correction = inputTransitionMatrix.ExtendedTransitionMatrix ? 1 : 0;

                do
                {
                    hierarchyNumber += 10;
                    var operation = new M_Operation
                    {
                        ArticleId = article.Article.Id,
                        Name = "Operation " + (operationCount + 1) +  " for [" + article.Article.Name + "]",
                        Duration = _machiningTimeDistributions[currentWorkingMachine].Sample(),
                        ResourceCapabilityId = tools[currentWorkingMachine].GetNext().Id,
                        HierarchyNumber = hierarchyNumber,
                    };
                    article.Operations.Add(new Operation
                    {
                        MOperation = operation,
                        SetupTimeOfCapability = _workingStations[currentWorkingMachine].SetupTime,
                        InternMachineGroupIndex = currentWorkingMachine
                    });

                    currentWorkingMachine = DetermineNextWorkingMachine(currentWorkingMachine + correction, rng);
                    operationCount++;
                    if (inputTransitionMatrix.ExtendedTransitionMatrix)
                    {
                        lastOperationReached = _matrixSize == currentWorkingMachine + 1;
                    }
                    else
                    {
                        lastOperationReached = article.WorkPlanLength == operationCount;
                    }
                } while (!lastOperationReached);
            }
        }

        private int DetermineNextWorkingMachine(int currentMachine, Random rng)
        {
            var u = rng.NextDouble();
            var sum = 0.0;
            var k = 0;
            while (k < _cumulatedProbabilities[currentMachine].Count - 1)
            {
                sum += _cumulatedProbabilities[currentMachine][k].Value;
                if (u < sum)
                {
                    break;
                }

                k++;
            }
            return _cumulatedProbabilities[currentMachine][k].Key;
        }

        private void Prepare(TransitionMatrix transitionMatrix, TransitionMatrixInput inputTransitionMatrix, Random rng)
        {
            _matrixSize = inputTransitionMatrix.WorkingStations.Count;
            TruncatedDiscreteNormal unifyingDistribution = null;
            //darf lowerBound (also Mindestdauer einer Operation) 0 sein? -> wenn 0 selten vorkommt (also z.B. Zeiteinheit nicht Minuten, sondern Sekunden sind), dann ok
            if (inputTransitionMatrix.GeneralMachiningTimeParameterSet != null)
            {
                var normalDistribution = Normal.WithMeanVariance(
                    inputTransitionMatrix.GeneralMachiningTimeParameterSet.MeanMachiningTime,
                    inputTransitionMatrix.GeneralMachiningTimeParameterSet.VarianceMachiningTime, rng);
                unifyingDistribution = new TruncatedDiscreteNormal(1, null, normalDistribution);
            }

            _workingStations = inputTransitionMatrix.WorkingStations.ToArray();
            for (var i = 0; i < _matrixSize; i++)
            {
                TruncatedDiscreteNormal truncatedDiscreteNormalDistribution;
                if (unifyingDistribution != null)
                {
                    truncatedDiscreteNormalDistribution = unifyingDistribution;
                }
                else
                {
                    var machiningTime = _workingStations[i].MachiningTimeParameterSet;
                    var normalDistribution = Normal.WithMeanVariance(machiningTime.MeanMachiningTime,
                        machiningTime.VarianceMachiningTime, rng);
                    truncatedDiscreteNormalDistribution = new TruncatedDiscreteNormal(1, null, normalDistribution);
                }

                _machiningTimeDistributions.Add(truncatedDiscreteNormalDistribution);
            }
            if (inputTransitionMatrix.ExtendedTransitionMatrix)
            {
                _matrixSize++;
            }

            for (var i = 0; i < _matrixSize; i++)
            {
                var row = new List<KeyValuePair<int, double>>();
                _cumulatedProbabilities.Add(row);
                for (var j = 0; j < _matrixSize; j++)
                {
                    row.Add(new KeyValuePair<int, double>(j, transitionMatrix.Pi[i,j]));
                }

                row.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
                {
                    if (o1.Value > o2.Value) return -1;
                    return 1;
                });
            }
        }

    }
}