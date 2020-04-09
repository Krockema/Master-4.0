﻿using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
using static FRequestProposalForCapabilityProviders;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue
{
    public class TimeConstraintQueue : SortedList<long, FJobConfirmation>, IJobQueue
    {
        public int Limit { get; set; }

        public TimeConstraintQueue(int limit)
        {
            Limit = limit;
        }

        public FJobConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null)
        {
            var bucket = GetFirstSatisfied(currentTime);
            this.Remove(bucket.Key);
            return bucket.Value;
        }

        public void Enqueue(FJobConfirmation jobConfirmation)
        {
            this.Add(jobConfirmation.Schedule, jobConfirmation);
        }

        public KeyValuePair<long, FJobConfirmation> GetFirstSatisfied(long currentTime)
        {
            var bucket = this.First(x => ((FBucket)x.Value.Job).HasSatisfiedJob);
            return bucket;
        }

        public bool CapacitiesLeft()
        {
            return Limit > GetJobsAs<FBucket>().Sum(selector: x => x.Scope);
        }

        public bool HasQueueAbleJobs()
        {
            var hasSatisfiedJob = false;
            foreach (var job in this.Values)
            {
                if (((FBucket)job.Job).HasSatisfiedJob)
                {
                    hasSatisfiedJob = true;
                }
            }
            return hasSatisfiedJob;
        }

        public IEnumerable<T> GetJobsAs<T>()
        {
            return this.Values.Select(x => x.Job).Cast<T>();
        }

        public T GetJobAs<T>(Guid key)
        {
            var jobConfirmation = this.SingleOrDefault(x => x.Value.Job.Key == key).Value;
            return (T)Convert.ChangeType(jobConfirmation.Job, typeof(T));
        }

        public FJobConfirmation GetConfirmation(Guid key)
        {
            return this.Values.SingleOrDefault(x => x.Job.Key == key);
        }

        public bool RemoveJob(FJobConfirmation job)
        {
            return this.Remove(job.Schedule);
        }

        public HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = this.Where(x => x.Key >= jobConfirmation.Schedule
                                           && x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime))
                .Select(x => x.Value).ToHashSet();
            return toRequeue;
        }

        public List<QueueingPosition> GetQueueAbleTime(FRequestProposalForCapabilityProvider jobProposal
                                , long currentTime, CapabilityProviderManager cpm)
        {
            
            var positions = new List<QueueingPosition>();
            var job = jobProposal.Job;
            var jobPriority = jobProposal.Job.Priority(currentTime);
            var allWithLowerPriority = this.Where(x => x.Value.Job.Priority(currentTime) <= jobPriority);
            var enumerator = allWithLowerPriority.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                // Queue contains no job --> Add queable item.
                positions.Add(new QueueingPosition(isQueueAble: true,
                                                    estimatedStart: currentTime + GetRequiredSetupTime(cpm, jobProposal.CapabilityProviderId)));
            }
            else
            {
                var current = enumerator.Current;
                var totalWorkLoad = current.Key 
                                            + ((FBucket) current.Value.Job).MaxBucketSize
                                            + GetRequiredSetupTime(cpm, current.Value.CapabilityProvider.Id, jobProposal);
                while (enumerator.MoveNext())
                {
                    var endPre = current.Key + ((FBucket) current.Value.Job).MaxBucketSize;
                    var startPost = enumerator.Current.Key;
                    var requiredSetupTime = GetRequiredSetupTime(cpm, current.Value.CapabilityProvider.ResourceCapabilityId, jobProposal);
                    if (endPre <= startPost - ((FBucket)current.Value.Job).MaxBucketSize - requiredSetupTime)
                    {
                        // slotFound = validSlots.TryAdd(endPre, startPost - endPre);
                        positions.Add(new QueueingPosition(isQueueAble: true,
                                                            estimatedStart: endPre));
                    }

                    totalWorkLoad = endPre + ((FBucket) current.Value.Job).MaxBucketSize + requiredSetupTime;
                    current = enumerator.Current;
                }

                positions.Add(new QueueingPosition(isQueueAble: (totalWorkLoad < Limit || ((FBucket)job).HasSatisfiedJob),
                                                    estimatedStart: currentTime + totalWorkLoad));
                
            }
            enumerator.Dispose();
            return positions;
        }

        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int capabilityProviderId)
        {
            if (cpm.AlreadyEquipped(capabilityProviderId)) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(capabilityProviderId);
        }

        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int currentId, FRequestProposalForCapabilityProvider requestProposalForCapabilityProvider)
        {
            if (currentId == requestProposalForCapabilityProvider.Job.RequiredCapability.Id) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(requestProposalForCapabilityProvider.CapabilityProviderId);
        }

        public FJobConfirmation FirstOrNull()
        {
            return this.Count > 0 ? this.Values.First() : null;
        }

        public long Workload => this.Values.Sum(x => x.Job.Duration);
    }
}
