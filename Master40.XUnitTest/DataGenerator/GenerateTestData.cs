using System;
using System.Collections.Generic;
using Master40.DataGenerator.Configuration;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Master40.DataGenerator.Util;
using Master40.DataGenerator.Verification;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.GeneratorModel;
using Master40.DB.Util;
using MathNet.Numerics.Distributions;
using Xunit;
using Xunit.Abstractions;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateTestData
    {
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testGeneratorCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestGeneratorContext;Trusted_Connection=True;MultipleActiveResultSets=true";

        //Es gibt wohl eine Diskripanz zwischen Master40 und SYMTEP was Operationen und Stücklisten (BOM) angeht (Struktur und Zeitpunkt)
        [Fact]
        public void SetInput()
        {
            var success = true;
            var iterations = 1;

            for (var i = 0; i < iterations; i++)
            {
                var usePresetSeed = false;
                var rng = new Random();
                int seed = usePresetSeed ? 2083793265 : rng.Next();

                var generatorDbCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);
                var approach = new Approach()
                {
                    CreationDate = DateTime.Now,
                    Seed = seed
                };

                //Limit für Lambda und Anzahl Bearbeitungsstationen jeweils 100
                var individualMachiningTime = true;
                approach.TransitionMatrixInput = new TransitionMatrixInput
                {
                    DegreeOfOrganization = 1.0,
                    Lambda = 1.0,
                    InfiniteTools = true,
                    ExtendedTransitionMatrix = false,
                    GeneralMachiningTimeParameterSet = individualMachiningTime ? null : new MachiningTimeParameterSet
                    {
                        MeanMachiningTime = 10,
                        VarianceMachiningTime = 0
                    },
                    WorkingStations = new List<WorkingStationParameterSet>()
                    {
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 16, VarianceMachiningTime = 0.0
                            },
                            ResourceCount = 4,
                            ToolCount = 8,
                            SetupTime = 32,
                            OperatorCount = 0
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 12, VarianceMachiningTime = 0.0
                            },
                            ResourceCount = 3,
                            ToolCount = 6,
                            SetupTime = 24,
                            OperatorCount = 0
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 20, VarianceMachiningTime = 0.0
                            },
                            ResourceCount = 5,
                            ToolCount = 10,
                            SetupTime = 40,
                            OperatorCount = 0
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 8, VarianceMachiningTime = 0.0
                            },
                            ResourceCount = 2,
                            ToolCount = 4,
                            SetupTime = 16,
                            OperatorCount = 0
                        }
                    }
                };
                var transitionMatrixSettings = new DataGeneratorTableTransitionMatrixSettingConfiguration();
                //Änderungen der Standardkonfiguration:
                transitionMatrixSettings.BALANCED_PI_B_INIT.SettingValue = 1.0;
                transitionMatrixSettings.BALANCED_PI_A_INIT.SettingValue = 1.0;
                approach.TransitionMatrixInput.SettingConfiguration = transitionMatrixSettings.AsList();
                var minPossibleOG = TransitionMatrixGenerator.CalcMinPossibleDegreeOfOrganization(
                    approach.TransitionMatrixInput.WorkingStations.Count,
                    approach.TransitionMatrixInput.ExtendedTransitionMatrix);
                approach.TransitionMatrixInput.DegreeOfOrganization = Math.Max(minPossibleOG, approach.TransitionMatrixInput.DegreeOfOrganization);

                //Nebenbedingung lautet, dass Fertigungstiefe mindestens 1 sein muss, es macht aber wenig Sinn, wenn sie gleich 1 ist, da es dann keine Fertigungen gibt
                //-> Anpassung der Nebenbedingung: Fertigungstiefe muss mindestens 2 sein
                //KG und MV nicht größer 5; FT nicht größer 20; Anzahl Endprodukte nicht größer 50
                var randomGeneratedInputValues = false;
                double? doubleNull = null;
                approach.ProductStructureInput = new ProductStructureInput
                {
                    EndProductCount = !randomGeneratedInputValues ? 30 : rng.Next(9) + 2,
                    DepthOfAssembly = !randomGeneratedInputValues ? 4 : rng.Next(10) + 1,
                    ComplexityRatio = !randomGeneratedInputValues ? 2 : rng.NextDouble() + 1,
                    ReutilisationRatio = !randomGeneratedInputValues ? 15 : rng.NextDouble() + 1,
                    MeanIncomingMaterialAmount = 1,
                    StdDevIncomingMaterialAmount = 0.0,
                    MeanWorkPlanLength = approach.TransitionMatrixInput.ExtendedTransitionMatrix ? doubleNull : 4.0,
                    VarianceWorkPlanLength = approach.TransitionMatrixInput.ExtendedTransitionMatrix ? doubleNull : 0.0
                };
                if (ProductStructureGenerator.DeterminateMaxDepthOfAssemblyAndCheckLimit(approach.ProductStructureInput))
                {
                    //System.Diagnostics.Debug.WriteLine(approach.ProductStructureInput.ToString());

                    var edgeWeightRoundModes = new DataGeneratorTableEdgeWeightRoundMode();
                    edgeWeightRoundModes.Load(generatorDbCtx);

                    approach.BomInput = new BillOfMaterialInput
                    {
                        EdgeWeightRoundModeId = edgeWeightRoundModes.ROUND_ALWAYS.Id,
                        WeightEpsilon = 0.001
                    };

                    generatorDbCtx.Approaches.AddRange(approach);
                    generatorDbCtx.SaveChanges();

                    System.Diagnostics.Debug.WriteLine(
                        "################################# Generated test data have the approach id of " + approach.Id);
                }
                else
                {
                    success = false;
                    System.Diagnostics.Debug.WriteLine(
                        "################################# Eingaben für Erzeugnisstruktur waren ungültig!");
                }
            }

            Assert.True(success);
        }

        [Fact]
        public void GenerateData() //Generierung für Simulation direkt im Testfall, wo Simulation durchgeführt wird
        {
            var approachRangeStart = 76;
            var approachRangeEnd = 76;
            for (var i = approachRangeStart; i < approachRangeEnd + 1; i++)
            {
                var approachId = i;
                var generatorDbCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);
                var approach = ApproachRepository.GetApproachById(generatorDbCtx, approachId);

                /*var parameterSet = ParameterSet.Create(new object[] { Dbms.GetNewMasterDataBase(false, "Master40") });
                var dataBase = parameterSet.GetOption<DataBase<ProductionDomainContext>>();*/

                var dbContext = MasterDBContext.GetContext(testCtxString);

                var generator = new MainGenerator();
                generator.StartGeneration(approach, dbContext, true, 1.0);
            }
            Assert.True(true);

        }

        [Fact]
        public void CheckOrganizationDegreeFromResults()
        {
            var simNumberStart = 109;
            var simNumberEnd = 109;
            var simNumberSkip = new HashSet<int> {};
            var dbContext = MasterDBContext.GetContext(testCtxString);
            var dbResultCtx = ResultContext.GetContext(testResultCtxString);
            var dbGeneratorCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);

            for (var i = simNumberStart; i < simNumberEnd + 1; i++)
            {
                if (simNumberSkip.Contains(i))
                {
                    continue;
                }
                var transitionMatrixGeneratorVerifier = new TransitionMatrixGeneratorVerifier();
                transitionMatrixGeneratorVerifier.VerifySimulatedData(dbContext, dbGeneratorCtx, dbResultCtx, i);
            }

            Assert.True(true);
        }

        //maximale Anzahl an Bearbeitungsstationen (Maschinengruppen): 21
        [Fact]
        public void Test1()
        {
            var lintMax = Int32.MaxValue;
            var longMax = Int64.MaxValue;
            var doubleMax = Double.MaxValue;
            var doubleMaxPlusALot = doubleMax + 1e+307d;
            System.Diagnostics.Debug.WriteLine(lintMax.ToString());
            System.Diagnostics.Debug.WriteLine(longMax.ToString());
            System.Diagnostics.Debug.WriteLine(doubleMax.ToString());
            var faculty = new Faculty();
            //var f1 = faculty.Calc(200);
            //var f2 = faculty.Calc(20);
            //var r = Math.Round(f2);
            //var p1 = Math.Pow(100.537, 100);
            //var p2 = Math.Pow(10.1, 20);
            var x1 = Math.Round(Math.Pow(5.0 / 1.0, 19) * 50);
            var x2 = Convert.ToInt64(x1);
            var sum1 = Convert.ToInt64(0);
            var sum2 = 0.0;
            for (int i = 0; i < 20; i++)
            {
                var result = Convert.ToInt64(Math.Round(Math.Pow(5.0 / 1.0, i) * 50));
                sum1 += result;
                sum2 += result;
            }

            sum1 *= 5;
            sum2 *= 5.0;
            var x3 = Convert.ToInt64(Math.Round(sum2));

            var x4 = Math.Round(5.4343454359);

            var n1 = AlphabeticNumbering.GetAlphabeticNumbering(0);
            var n2 = AlphabeticNumbering.GetAlphabeticNumbering(25);
            var n3 = AlphabeticNumbering.GetAlphabeticNumbering(26);
            var n4 = AlphabeticNumbering.GetAlphabeticNumbering(52);
            var n5 = AlphabeticNumbering.GetAlphabeticNumbering(454);
            var n6 = AlphabeticNumbering.GetAlphabeticNumbering(1);
            var n7 = AlphabeticNumbering.GetAlphabeticNumbering(2);
            var n8 = AlphabeticNumbering.GetAlphabeticNumbering(3);

            var n11 = AlphabeticNumbering.GetNumericRepresentation(n1);
            var n12 = AlphabeticNumbering.GetNumericRepresentation(n2);
            var n13 = AlphabeticNumbering.GetNumericRepresentation(n3);
            var n14 = AlphabeticNumbering.GetNumericRepresentation(n4);
            var n15 = AlphabeticNumbering.GetNumericRepresentation(n5);
            var n16 = AlphabeticNumbering.GetNumericRepresentation(n6);
            var n17 = AlphabeticNumbering.GetNumericRepresentation(n7);
            var n18 = AlphabeticNumbering.GetNumericRepresentation(n8);

            var list1 = new List<TruncatedDiscreteNormal>();
            var truncatedDiscreteNormalDistribution =
                new TruncatedDiscreteNormal(9, 11, Normal.WithMeanVariance(5.0, 2.0));
            list1.Add(truncatedDiscreteNormalDistribution);
            list1.Add(truncatedDiscreteNormalDistribution);
            list1.Add(truncatedDiscreteNormalDistribution);
            list1.Add(truncatedDiscreteNormalDistribution);
            var x5 = list1[1].Sample();

            Assert.True(true);
        }
    }
}
