﻿using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Agents.JobAgent;
using static FBuckets;
using static FOperations;
using static FProposals;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(long maxBucketSize, SimulationType simulationType = SimulationType.BucketScope)
            : base(simulationType: simulationType)
        {
            _bucketManager = new BucketManager(maxBucketSize: maxBucketSize);
        }
        private List<FOperation> _unassigendOperations { get; } = new List<FOperation>();
        private BucketManager _bucketManager { get; } 

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Hub.Instruction.Default.EnqueueJob msg: AssignJob(msg.GetObjectFromMessage); break;
                //case Hub.Instruction.BucketScope.EnqueueOperation msg: EnqueueOperation(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.EnqueueBucket msg: EnqueueBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.SetBucketFix msg: SetBucketFix(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.FinishBucket msg: FinishBucket(msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        private void ResetBucket(Guid bucketKey)
        {
            var bucket = _bucketManager.GetBucketByBucketKey(bucketKey);

            if (bucket == null)
                return;

            var successRemove = _bucketManager.Remove(bucket.Key, Agent);
            
            if (successRemove)
            {
                _proposalManager.Remove(bucket.Key);
                _unassigendOperations.AddRange(bucket.Operations);
                RequeueOperations(bucket.Operations.ToList());
            }
            //TODO multiple reset from one setupdefinition?
            else
            {
                new Exception($"something went wrong with reset Bucket");
            }

        }

        private void AssignJob(IJob job)
        {
            var operation = (FOperation)job;

            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} {operation.Key} | with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}");

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            _unassigendOperations.Add(operation);

            _bucketManager.AddOrUpdateBucketSize(job.RequiredCapability, job.Duration);

            EnqueueOperation(operation);

        }

        internal void EnqueueOperation(FOperation fOperation)
        {
            var operationInBucket = _bucketManager.GetBucketByOperationKey(fOperation.Key);

            if (operationInBucket != null)
            {
                Agent.DebugMessage($"{fOperation.Operation.Name} {fOperation.Key} is already in bucket");
                return;
            }

            var bucket = _bucketManager.AddToBucket(fOperation);
            if (bucket != null) return;//if no bucket to add exists create a new one
            
            var jobConfirmation = _bucketManager.CreateBucket(fOperation: fOperation, Agent);
            _unassigendOperations.Remove(fOperation);
            EnqueueBucket(jobConfirmation.Job.Key);

            //after creating new bucket, modify subsequent buckets
            ModifyBucket(fOperation);
        }
        
        private void ModifyBucket(FOperation operation)
        {
            var bucketsToModify = _bucketManager.FindAllBucketsLaterForwardStart(operation);

            if (bucketsToModify.Count > 0)
            {
                foreach (var modBucket in bucketsToModify)
                {
                    if (modBucket.IsConfirmed)
                    {
                        //Send to first resource
                        foreach(var setup in modBucket.CapabilityProvider.ResourceSetups)
                        {
                            if (setup.Resource.IResourceRef == null) continue;
                            
                            Agent.Send(
                            Resource.Instruction.BucketScope.RequeueBucket
                                .Create(modBucket.Job.Key, setup.Resource.IResourceRef as IActorRef));

                        }
                        
                    }
                    else
                    {
                        ResetBucket(modBucket.Job.Key);
                    }

                }
            }
        }

        internal void EnqueueBucket(Guid bucketKey)
        {
            //TODO maybe ID

            //delete all proposals if exits
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey);
            if (jobConfirmation == null)
            {
                //if bucket already deleted in BucketManager, also delete bucket in proposalmanager
                _proposalManager.Remove(bucketKey);
                return;
            }
            if (jobConfirmation.IsFixPlanned)
            {
                return;
            }


            jobConfirmation.ResetConfirmation();
            _proposalManager.Add(jobConfirmation.Job.Key, _capabilityManager.GetAllCapabilityProvider(jobConfirmation.Job.RequiredCapability));
            

            Agent.DebugMessage($"Enqueue {jobConfirmation.Job.Name} with {((FBucket)jobConfirmation.Job).Operations.Count} operations", CustomLogger.PROPOSAL, LogLevel.Warn);
            
            var capabilityDefinition = _capabilityManager.GetResourcesByCapability(jobConfirmation.Job.RequiredCapability);

            foreach (var capabilityProvider in capabilityDefinition.GetAllCapabilityProvider())
            {
                foreach (var setup in capabilityProvider.ResourceSetups)
                {
                    var resource = setup.Resource;
                    if (setup.Resource.Count == 0) continue;

                    var resourceRef = resource.IResourceRef as IActorRef;
                    Agent.DebugMessage(msg: $"Ask for proposal at resource {resourceRef.Path.Name} with {jobConfirmation.Job.Key}", CustomLogger.PROPOSAL, LogLevel.Warn);
                    Agent.Send(instruction: Resource.Instruction.Default.RequestProposal
                        .Create(new FRequestProposalForCapabilityProvider(jobConfirmation.Job
                                                                  , capabilityProviderId : capabilityProvider.Id)
                              , target: resourceRef));
                }
            }

        }

        internal override void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(fProposal.JobKey);

            if (jobConfirmation == null) return;

            var bucket = jobConfirmation.Job as FBucket;
            var resourceAgent = fProposal.ResourceAgent as IActorRef;
            var required = _proposalManager.AddProposal(fProposal);
            if (required == null) return;
            var schedules = fProposal.PossibleSchedule as List<FQueueingScope>;
            var propSet = _proposalManager.GetProposalForSetupDefinitionSet(fProposal.JobKey);
            Agent.DebugMessage(msg: $"Proposal({propSet.ReceivedProposals}of{propSet.RequiredProposals}) " +
                                    $"for {bucket.Name} {bucket.Key} with Schedule: {schedules.First().Scope.Start} " +
                                    $"JobKey: {fProposal.JobKey} from: {resourceAgent.Path.Name}!", CustomLogger.PROPOSAL, LogLevel.Warn);

            // if all resources replied 
            if (propSet.AllProposalsReceived)
            {
                // item Postponed by All resources ? -> requeue after given amount of time.
                var proposalForCapabilityProvider = propSet.GetValidProposal();
                if (proposalForCapabilityProvider.Count() == 0)
                {
                    var postponedFor = propSet.PostponedUntil; // TODO: Naming Until != For

                    _proposalManager.RemoveAllProposalsFor(bucket.Key);

                    Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(bucket.Key, target: Agent.Context.Self), waitFor: postponedFor);
                    Agent.DebugMessage($"{bucket.Name} {bucket.Key} has been postponed for {postponedFor}", CustomLogger.PROPOSAL, LogLevel.Warn);
                    return;

                }


                List<PossibleProcessingPosition> possibleProcessingPositions = _proposalManager.CreatePossibleProcessingPositions(proposalForCapabilityProvider, bucket);

                var possiblePosition = possibleProcessingPositions.OrderBy(x => x._processingPosition).First();

                jobConfirmation.CapabilityProvider = possiblePosition._resourceCapabilityProvider;

                Agent.Send(Job.Instruction.UpdateJob.Create(jobConfirmation.CapabilityProvider, jobConfirmation.JobAgentRef));

                foreach (var setup in jobConfirmation.CapabilityProvider.ResourceSetups)
                {
                    if (setup.Resource.Count == 0) continue;

                    jobConfirmation.ScopeConfirmation = possiblePosition._queuingDictionary.Single(x => x.Key.Equals(setup.Resource.IResourceRef)).Value;
                    Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {bucket.Name} {bucket.Key} on resource {setup.Resource.Name}" +
                                            $" with scope confirmation for {jobConfirmation.ScopeConfirmation.GetScopeStart()} to {jobConfirmation.ScopeConfirmation.GetScopeEnd()}"
                                            , CustomLogger.PROPOSAL, LogLevel.Warn);
                    Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal
                        .Create(jobConfirmation.ToImmutable()
                            , target: setup.Resource.IResourceRef as IActorRef));
                }




                _proposalManager.Remove(bucket.Key);

            }
        }

        /// <summary>
        /// Source: ResourceAgent 
        /// </summary>
        /// <param name="bucketKey"></param>
        internal void SetBucketFix(Guid bucketKey)
        {
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey);
            var bucket = jobConfirmation.Job as FBucket;
            //refuse bucket if not exits anymore
            if (bucket != null)
            {
                var notSatisfiedOperations = _bucketManager.GetAllNotSatifsiedOperation(bucket);
                
                if (notSatisfiedOperations.Count > 0)
                {
                    bucket = _bucketManager.RemoveOperations(bucket, notSatisfiedOperations);
                }

                bucket = bucket.SetFixPlanned;
                _bucketManager.SetBucketSatisfied(bucket);
                _unassigendOperations.AddRange(notSatisfiedOperations); 
                RequeueOperations(notSatisfiedOperations);
            }
            else
            {
                Agent.DebugMessage(msg: $"{bucket.Name} does not exits anymore");
                jobConfirmation.ScopeConfirmation = null;
            }

            Agent.Send(Job.Instruction.LockJob.Create(jobConfirmation.ToImmutable(), jobConfirmation.JobAgentRef));
            //TODO Send only to one resource and let the resource handle all the other resources? or send to all? --> For now send to first (like requeue bucket)
            
            //Requeue all unsatisfied operations
        }

        internal override void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            var operations = _unassigendOperations.Where(x => x.Key == startCondition.OperationKey);

            if (operations.Count() > 0)
            {
                operations.First().SetStartConditions(startCondition);
                return;
            }

            var bucket = _bucketManager.GetBucketByOperationKey(startCondition.OperationKey);
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey: bucket.Key);
            bucket = jobConfirmation.Job as FBucket;

            _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);

            if (!jobConfirmation.IsConfirmed || !bucket.Operations.Any(x => x.StartConditions.Satisfied))
                return;

            foreach (var setup in jobConfirmation.CapabilityProvider.ResourceSetups)
            {
                if (setup.Resource.Count == 0) continue;
                var resourceRef = setup.Resource.IResourceRef as IActorRef;
                Agent.DebugMessage(msg: $"Update and forward start condition: {startCondition.OperationKey} in {bucket.Name}" +
                                        $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                        $"| PreCondition: {startCondition.PreCondition} " +
                                        $"to resource {resourceRef.Path.Name}");

                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: resourceRef));
            }
            
        }

        internal override void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _bucketManager.GetOperationByOperationKey(operationKey);
            if (operation == null)
                throw new Exception("operation was not found in bucketManager ");
            Agent.DebugMessage($"WithdrawRequiredArticles for operation {operation.Operation.Name} {operationKey} on {Agent.Context.Self.Path.Name}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles
            .Create(message: operation.Key
                , target: operation.ProductionAgent));
        }

        internal void RequeueOperations(List<FOperation> operations)
        {
            foreach (var operation in operations.OrderBy(x => x.ForwardStart).ToList())
            {
                Agent.DebugMessage(msg: $"Requeue operation {operation.Operation.Name} {operation.Key}");
                EnqueueOperation(operation);
               
            }

        }

        /// <summary>
        /// Job = Bucket
        /// </summary>
        /// <param name="jobResult"></param>
        internal override void FinishJob(IJobResult jobResult)
        {
            
            var operation =_bucketManager.RemoveOperation(jobResult.Key);

            // TODO Dynamic Lot Sizing
            _bucketManager.DecreaseBucketSize(operation.RequiredCapability.Id,
                operation.Operation.Duration);
            if (Agent.DebugThis)
            {
                var bucket = _bucketManager.GetBucketByOperationKey(operationKey: operation.Key);
                Agent.DebugMessage(msg: $"Operation finished: {operation.Operation.Name} {jobResult.Key} in bucket: {bucket.Name} {bucket.Key}");
            }
            
            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: operation.ProductionAgent));
            
        }

        internal void FinishBucket(IJobResult jobResult)
        {
            _bucketManager.Remove(jobResult.Key, Agent);
        }

    }
}
