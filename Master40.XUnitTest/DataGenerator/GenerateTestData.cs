using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Master40.DataGenerator.Util;
using Master40.DataGenerator.Verification;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.GeneratorModel;
using Master40.DB.Util;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Xunit;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateTestData
    {
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testGeneratorCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestGeneratorContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string pathToJsonFile = "path/to/JSON/file";

        [Fact]
        public void SetInputViaJsonFile()
        {
            var jsonFileContent = File.ReadAllText(pathToJsonFile);

            var generator = new JSchemaGenerator();
            var schema = generator.Generate(typeof(List<Approach>));
            var reader = new JsonTextReader(new StringReader(jsonFileContent));
            var validatingReader = new JSchemaValidatingReader(reader)
            {
                Schema = schema
            };
            var messages = new List<string>();
            validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);
            var serializer = new JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            var approaches = serializer.Deserialize<List<Approach>>(validatingReader);
            if (messages.Count > 0)
            {
                throw new Exception(messages[0]);
            }
            approaches.RemoveAll(_ => _ == null);
            if (approaches.Count == 0)
            {
                throw new Exception("Approaches array must not have 0 (not null) elements");
            }

            var success = true;
            var generatorDbCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);
            foreach (var approach in approaches)
            {
                var presetConf =
                    approach.TransitionMatrixInput.SettingConfiguration.ToDictionary(_ => _.SettingOption.Name);
                var optionsById = new DataGeneratorTableTransitionMatrixSettingOption().AsList()
                    .ToDictionary(_ => _.Id);
                var transitionMatrixSettings = new DataGeneratorTableTransitionMatrixSettingConfiguration();
                approach.TransitionMatrixInput.SettingConfiguration = transitionMatrixSettings.AsList();
                foreach (var conf in approach.TransitionMatrixInput.SettingConfiguration)
                {
                    var name = optionsById[conf.SettingOptionId].Name;
                    if (presetConf.ContainsKey(name))
                    {
                        conf.SettingValue = presetConf[name].SettingValue;
                    }
                }

                var edgeWeightRoundMode = DataGeneratorTableEdgeWeightRoundMode.GetEdgeWeightRoundModeByName(
                    generatorDbCtx,
                    approach.BomInput.EdgeWeightRoundMode.Name);
                if (edgeWeightRoundMode == null)
                {
                    throw new Exception("There is no edge weight round mode with the name \"" +
                                        approach.BomInput.EdgeWeightRoundMode.Name + "\" in the system");
                }
                approach.BomInput.EdgeWeightRoundMode = null;
                approach.BomInput.EdgeWeightRoundModeId = edgeWeightRoundMode.Id;

                success &= checkAndValidateInput(approach, generatorDbCtx);
            }

            safeInput(generatorDbCtx, success, approaches);
            Assert.True(success);
        }

        //Es gibt wohl eine Diskripanz zwischen Master40 und SYMTEP was Operationen und Stücklisten (BOM) angeht (Struktur und Zeitpunkt)
        [Fact]
        public void SetInput()
        {
            var iterations = 1;

            var success = true;
            var generatorDbCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);
            var createdApproaches = new List<Approach>();
            var edgeWeightRoundModes = new DataGeneratorTableEdgeWeightRoundMode();
            edgeWeightRoundModes.Load(generatorDbCtx);

            for (var i = 0; i < iterations; i++)
            {
                var approach = new Approach()
                {
                    PresetSeed = null
                };
                createdApproaches.Add(approach);

                //Nebenbedingung lautet, dass Fertigungstiefe mindestens 1 sein muss, es macht aber wenig Sinn, wenn sie gleich 1 ist, da es dann keine Fertigungen gibt
                //-> Anpassung der Nebenbedingung: Fertigungstiefe muss mindestens 2 sein
                //KG und MV nicht größer 5; FT nicht größer 20; Anzahl Endprodukte nicht größer 50
                var randomGeneratedInputValues = false;
                var rng = new Random();
                approach.ProductStructureInput = new ProductStructureInput
                {
                    EndProductCount = !randomGeneratedInputValues ? 30 : rng.Next(9) + 2,
                    DepthOfAssembly = !randomGeneratedInputValues ? 4 : rng.Next(10) + 1,
                    ComplexityRatio = !randomGeneratedInputValues ? 2 : rng.NextDouble() + 1,
                    ReutilisationRatio = !randomGeneratedInputValues ? 15 : rng.NextDouble() + 1,
                    MeanIncomingMaterialAmount = 1,
                    VarianceIncomingMaterialAmount = 0.0
                };
                //System.Diagnostics.Debug.WriteLine(approach.ProductStructureInput.ToString());

                //Limit für Lambda und Anzahl Bearbeitungsstationen jeweils 100
                var individualMachiningTime = true;
                double? doubleNull = null;
                var extendedTransitionMatrix = false;
                approach.TransitionMatrixInput = new TransitionMatrixInput
                {
                    DegreeOfOrganization = 1.0,
                    Lambda = 1.0,
                    InfiniteTools = true,
                    ExtendedTransitionMatrix = extendedTransitionMatrix,
                    MeanWorkPlanLength = extendedTransitionMatrix ? doubleNull : 4.0,
                    VarianceWorkPlanLength = extendedTransitionMatrix ? doubleNull : 0.0,
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

                approach.BomInput = new BillOfMaterialInput
                {
                    EdgeWeightRoundModeId = edgeWeightRoundModes.ROUND_ALWAYS.Id,
                    WeightEpsilon = 0.001
                };

                success &= checkAndValidateInput(approach, generatorDbCtx);
            }

            safeInput(generatorDbCtx, success, createdApproaches);
            Assert.True(success);
        }

        private bool checkAndValidateInput(Approach approach, DataGeneratorContext generatorDbCtx)
        {
            if (ProductStructureGenerator.DeterminateMaxDepthOfAssemblyAndCheckLimit(approach.ProductStructureInput))
            {
                approach.Seed = approach.PresetSeed ?? new Random().Next();
                approach.CreationDate = DateTime.Now;

                var minPossibleOG = TransitionMatrixGenerator.CalcMinPossibleDegreeOfOrganization(
                    approach.TransitionMatrixInput.WorkingStations.Count,
                    approach.TransitionMatrixInput.ExtendedTransitionMatrix);
                approach.TransitionMatrixInput.DegreeOfOrganization = Math.Max(minPossibleOG,
                    approach.TransitionMatrixInput.DegreeOfOrganization);

                generatorDbCtx.Approaches.AddRange(approach);
                return true;
            }
            return false;
        }

        private void safeInput(DataGeneratorContext generatorDbCtx, bool success, List<Approach> approaches)
        {
            if (success)
            {
                generatorDbCtx.SaveChanges();
                var generatedIds = string.Join(", ", approaches.Select(x => x.Id));
                System.Diagnostics.Debug.WriteLine(
                    "################################# Generated test data have the following approach ids: " +
                    generatedIds);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("################################# Input was not valid!");
            }
        }

        [Fact]
        public void ExportInputParametersToJsonFile()
        {
            int approachId = 2;

            var generatorDbCtx = DataGeneratorContext.GetContext(testGeneratorCtxString);
            var approach = ApproachRepository.GetApproachById(generatorDbCtx, approachId);
            var jsonOutput = JsonConvert.SerializeObject(approach);
            File.WriteAllText(pathToJsonFile, jsonOutput);
            Assert.True(true);
        }

        [Fact]
        public void ExportSchemaForJsonFile()
        {
            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(typeof(List<Approach>));
            System.Diagnostics.Debug.WriteLine(schema.ToString(new JSchemaWriterSettings()));
            Assert.True(true);
        }

        [Fact]
        public void GenerateData() //Generierung für Simulation direkt im Testfall, wo Simulation durchgeführt wird
        {
            var approachRangeStart = 1;
            var approachRangeEnd = 1;
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
            var simNumberEnd = 117;
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

        //maximale Anzahl an Bearbeitungsstationen: 21
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
