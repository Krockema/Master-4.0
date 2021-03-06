﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.Data.Context;
using Mate.DataCore.DataModel;

namespace Mate.DataCore.Data.Initializer.Tables
{
    internal class MasterTableResource
    {

        internal Dictionary<string, List<M_Resource>> CapabilityToResourceDict = new Dictionary<string, List<M_Resource>>();
        internal Dictionary<string, List<M_ResourceSetup>> CapabilityToSetupDict = new Dictionary<string, List<M_ResourceSetup>>();
        internal Dictionary<string, List<M_ResourceCapabilityProvider>> CapabilityProviderDict = new Dictionary<string, List<M_ResourceCapabilityProvider>>();
        private readonly MasterTableResourceCapability _capability;

        public MasterTableResource(MasterTableResourceCapability capability)
        {
            _capability = capability;
        }

        internal void CreateModel(int sawResource, int drillResource, int assemblyResource, bool createTestModel = false)
        {
            CreateResourceGroup(sawResource, _capability.CUTTING);
            CreateResourceGroup(drillResource, _capability.DRILLING);
            CreateResourceGroup(assemblyResource, _capability.ASSEMBLING);
            // For Testing Purpose
            if (createTestModel)
            {
                WaterJet();
            }
        }

        private void CreateResourceGroup(int numberOfResources, M_ResourceCapability capability)
        {
            List<M_Resource> resourceGroup = new List<M_Resource>();
            for (int i = 1; i <= numberOfResources; i++)
            {
                var resource = CreateNewResource(capability.Name, true, false, i);
                resourceGroup.Add(resource);
            }
            CapabilityToResourceDict.Add(capability.Name, resourceGroup);
        }
        private M_Resource CreateNewResource(string resourceName, bool isPhysical, bool isBiological, int? number = null, string groupName = null)
        {
            return new M_Resource() { 
                Name = resourceName + number?.ToString(),
                Capacity = 1,
                IsPhysical = isPhysical,
                IsBiological = isBiological,
                GroupName = groupName,
                Quantile = 0.975, //TODO: ATTENTION!!!
                NumberOfUses = 0
            };
        }

        internal void CreateResourceTools(int setupTimeCutting, int setupTimeDrilling, int setupTimeAssembling, int[] numberOfOperators, int numberOfWorkers, bool secondResource)
        {
            List<M_Resource> workers = new List<M_Resource>();
            for (int i = 1; i < 1 + numberOfWorkers; i++)
            {
                workers.Add(CreateNewResource("Worker " + i, true, true, null, "Worker"));
            }
            CapabilityToResourceDict.Add($"Worker", workers);

            List<M_Resource> drillingTools = new List<M_Resource>();

            // Add a second primary resource that is required for setup and processing
            if (secondResource)
            {
                foreach (var drillCapability in _capability.DRILLING.ChildResourceCapabilities)
                {
                    drillingTools.Add(CreateNewResource($"{drillCapability.Name}", false, false));
                }
                CapabilityToResourceDict.Add($"Tool", drillingTools);
            }

            CreateTools(_capability.CUTTING, setupTimeCutting, numberOfOperators[0], workers, new List<M_Resource>(), "OperatorZuschnitt");
            CreateTools(_capability.DRILLING, setupTimeDrilling, numberOfOperators[1], workers, drillingTools, "OperatorBohrer");
            CreateTools(_capability.ASSEMBLING, setupTimeAssembling, numberOfOperators[2], workers, new List<M_Resource>(), "OperatorMontage");
        }


        private void CreateTools(M_ResourceCapability capability, long setupTime, int numberOfOperators, List<M_Resource> workerToAssign, List<M_Resource> resourceToolsToAssign, String workergroup)
        {
            List<M_Resource> tools = new List<M_Resource>();
            List<M_ResourceSetup> setups = new List<M_ResourceSetup>();
            List<M_ResourceCapabilityProvider> capabilityProviders = new List<M_ResourceCapabilityProvider>();
            List<M_Resource> operators = new List<M_Resource>();

            for (int i = 1; i < 1 + numberOfOperators; i++)
            {
                operators.Add(CreateNewResource(capability.Name + " Operator " + i, true, true, null, workergroup));
            }

            foreach (var resource in CapabilityToResourceDict.Single(x => x.Key == capability.Name).Value)
            {
                // With operators 
                if (operators.Count > 0)
                {
                    foreach (var op in operators)
                    {
                        if (workerToAssign.Any()) // with workers and setups
                        {
                            foreach (var worker in workerToAssign)
                            {
                                foreach (var subCapability in _capability.Capabilities
                                    .Single(x => x.Name == capability.Name)
                                    .ChildResourceCapabilities)
                                {
                                    var capabilityProvider = new M_ResourceCapabilityProvider()
                                    {
                                        Name = $"Provides {subCapability.Name} {resource.Name} {worker.Name}",
                                        ResourceCapabilityId = subCapability.Id,
                                    };

                                    var toolName = resource.Name + " " + subCapability.Name;
                                    var tool = tools.SingleOrDefault(x => x.Name.Equals(toolName));
                                    if (tool == null)
                                    {
                                        tool = CreateNewResource($"{resource.Name} {subCapability.Name}", false, false);
                                        tools.Add(tool);
                                    }

                                    if (resourceToolsToAssign.Any())
                                    {
                                        setups.Add(CreatePhysicalToolResource(resourceToolsToAssign, subCapability, setups, capabilityProvider));
                                    }

                                    // CreateNewSetup(resource, capabilityProvider, usedInProcessing,usedInSetup, setupTime)
                                    setups.Add(CreateNewSetup(op, capabilityProvider, false, true, 0));
                                    setups.Add(CreateNewSetup(tool, capabilityProvider, true, true, setupTime));
                                    setups.Add(CreateNewSetup(resource, capabilityProvider, true, true, 0));
                                    setups.Add(CreateNewSetup(worker, capabilityProvider, true, false, 0));
                                    capabilityProviders.Add(capabilityProvider);
                                }
                            }
                        }
                        else
                        { // no worker but setup
                            foreach (var subCapability in _capability.Capabilities
                                .Single(x => x.Name == capability.Name)
                                .ChildResourceCapabilities)
                            {
                                var capabilityProvider = new M_ResourceCapabilityProvider()
                                {
                                    Name = $"Provides {subCapability.Name} {resource.Name}",
                                    ResourceCapabilityId = subCapability.Id,
                                };

                                var toolName = resource.Name + " " + subCapability.Name;
                                var tool = tools.SingleOrDefault(x => x.Name.Equals(toolName));
                                if (tool == null)
                                {
                                    tool = CreateNewResource($"{resource.Name} {subCapability.Name}", false, false);
                                    tools.Add(tool);
                                }

                                if (resourceToolsToAssign.Any())
                                {
                                    setups.Add(CreatePhysicalToolResource(resourceToolsToAssign, subCapability, setups, capabilityProvider));
                                }
                                setups.Add(CreateNewSetup(op, capabilityProvider, false, true, 0));
                                setups.Add(CreateNewSetup(tool, capabilityProvider, true, true, setupTime));
                                setups.Add(CreateNewSetup(resource, capabilityProvider, true, true, 0));
                                capabilityProviders.Add(capabilityProvider);
                            }
                        }
                    }
                }
                else // without operators  
                {
                    if (workerToAssign.Any())
                    {
                        foreach (var worker in workerToAssign)
                        {
                            foreach (var subCapability in _capability.Capabilities
                                .Single(x => x.Name == capability.Name)
                                .ChildResourceCapabilities)
                            {
                                var capabilityProvider = new M_ResourceCapabilityProvider()
                                {
                                    Name = $"Provides {subCapability.Name} {resource.Name} {worker.Name}",
                                    ResourceCapabilityId = subCapability.Id,
                                };
                                // Tool
                                var toolName = resource.Name + " " + subCapability.Name;
                                var tool = tools.SingleOrDefault(x => x.Name.Equals(toolName));
                                if (tool == null)
                                {
                                    tool = CreateNewResource($"{resource.Name} {subCapability.Name}", false, false);
                                    tools.Add(tool);
                                }

                                if (resourceToolsToAssign.Any())
                                {
                                    setups.Add(CreatePhysicalToolResource(resourceToolsToAssign, subCapability, setups, capabilityProvider));
                                }

                                setups.Add(CreateNewSetup(tool, capabilityProvider, true, true, setupTime));
                                setups.Add(CreateNewSetup(resource, capabilityProvider, true, true, 0));
                                setups.Add(CreateNewSetup(worker, capabilityProvider, true, false, 0));
                                capabilityProviders.Add(capabilityProvider);
                            }
                        }
                    }
                    else
                    {
                        foreach (var subCapability in _capability.Capabilities
                            .Single(x => x.Name == capability.Name)
                            .ChildResourceCapabilities)
                        {
                            var capabilityProvider = new M_ResourceCapabilityProvider()
                            {
                                Name = $"Provides {subCapability.Name} {resource.Name}",
                                ResourceCapabilityId = subCapability.Id,
                            };
                            var toolName = resource.Name + " " + subCapability.Name;
                            var tool = tools.SingleOrDefault(x => x.Name.Equals(toolName));
                            if (tool == null)
                            {
                                tool = CreateNewResource($"{resource.Name} {subCapability.Name}", false, false);
                                tools.Add(tool);
                            }

                            if (resourceToolsToAssign.Any())
                            {
                                setups.Add(CreatePhysicalToolResource(resourceToolsToAssign, subCapability, setups, capabilityProvider));
                            }

                            setups.Add(CreateNewSetup(tool, capabilityProvider, true, true, setupTime));
                            setups.Add(CreateNewSetup(resource, capabilityProvider, true, true, 0));
                            capabilityProviders.Add(capabilityProvider);
                        }
                    }
                }
                CapabilityProviderDict.Add($"{resource.Name} Tooling", capabilityProviders);
                CapabilityToSetupDict.Add($"{resource.Name} Tooling", setups);
                CapabilityToResourceDict.Add($"{resource.Name} Tooling", tools);
                CapabilityToResourceDict.Add($"{resource.Name} Operators", operators);
            }
        }

        private M_ResourceSetup CreatePhysicalToolResource(List<M_Resource> resourceToolsToAssign, M_ResourceCapability subCapability, List<M_ResourceSetup> setups,
            M_ResourceCapabilityProvider capabilityProvider)
        {
            var toolObject =
                resourceToolsToAssign.Single(x => x.Name.Equals(subCapability.Name));
            return CreateNewSetup(toolObject, capabilityProvider, true, true, 0);
        }

        private M_ResourceSetup CreateNewSetup(M_Resource resource, M_ResourceCapabilityProvider capabilityProvider, bool usedInProcessing, bool usedInSetup, long setupTime)
        {
            return new M_ResourceSetup
            {
                ResourceCapabilityProviderId = capabilityProvider.Id,
                ResourceId = resource.Id,
                Name = $"Setup {capabilityProvider.Name} {resource.Name}",
                UsedInProcess = usedInProcessing,
                UsedInSetup = usedInSetup,
                SetupTime = setupTime
            };
        }
        private void WaterJet()
        {
            var waterJet = CreateNewResource("WaterJetCutter", true, false);
            List<M_ResourceCapabilityProvider> capabilityProviders = new List<M_ResourceCapabilityProvider>();
            List<M_ResourceSetup> setups = new List<M_ResourceSetup>();
            foreach (var capability in _capability.Capabilities
                .Where(x => x.Name == _capability.CUTTING.Name
                                          || x.Name == _capability.DRILLING.Name)
                .Select(x => x.ChildResourceCapabilities))
            {

                foreach (var subCapability in capability)
                {
                    var capabilityProvider = new M_ResourceCapabilityProvider()
                    {
                        Name = $"Provides {subCapability.Name} {waterJet.Name}",
                        ResourceCapabilityId = subCapability.Id,
                    };
                    setups.Add(CreateNewSetup(waterJet, capabilityProvider, true, true, 5));
                    capabilityProviders.Add(capabilityProvider);
                }
            }
            CapabilityToSetupDict.Add($"{waterJet.Name} WaterJetCutter", setups);
            CapabilityProviderDict.Add($"{waterJet.Name} Resource", capabilityProviders);
            CapabilityToResourceDict.Add($"{waterJet.Name} Resource", new List<M_Resource> { waterJet });
        }

        internal void InitSmall(MateDb context)
        {
            CreateModel(1, 1, 1);
            //SaveToDB(context);
        }
        internal void InitMedium(MateDb context)
        {
            CreateModel(2, 1, 2);
            //SaveToDB(context);
        }

        internal void InitLarge(MateDb context)
        {
            CreateModel(4, 2, 4);
            //SaveToDB(context);
        }

        internal void InitXLarge(MateDb context)
        {
            CreateModel(5, 3, 5);
            //SaveToDB(context);
        }

        internal void InitMediumTest(MateDb context)
        {
            CreateModel(2, 1, 2, true);
            //SaveToDB(context);
        }

        internal void SaveToDB(MateDb context)
        {
            foreach (var item in CapabilityToResourceDict)
            {
                context.Resources.AddRange(entities: item.Value);
            }
            context.SaveChanges();

            foreach (var item in CapabilityProviderDict)
            {
                context.ResourceCapabilityProviders.AddRange(entities: item.Value);
            }
            context.SaveChanges();

            foreach (var item in CapabilityToSetupDict)
            {
                context.ResourceSetups.AddRange(entities: item.Value);
            }
            context.SaveChanges();
        }
    }
}