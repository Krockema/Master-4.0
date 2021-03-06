﻿using System.Linq;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Mate.Production.Core.Agents;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Types;

namespace Mate.Test.Online.Preparations
{
    public class AgentMoc : Agent, IAgent
    {
        IActorRef IAgent.Guardian => _actorPaths.Guardians.Single(predicate: x => x.Key == _guardianType).Value;
        private readonly ActorPaths _actorPaths;
        private readonly GuardianType _guardianType;
        

        public static AgentMoc CreateAgent(ActorPaths actorPaths, Configuration configuration, IActorRef principal, IBehaviour behaviour, GuardianType guardianType)
        {
            return new AgentMoc(actorPaths: actorPaths,
                          configuration: configuration,
                                   time: 0,
                                  debug: false,
                              principal: principal,
                              behaviour: behaviour, 
                           guardianType: guardianType);
        }
        public static ActorPaths CreateActorPaths(TestKit testKit, IActorRef simContext)
        {
            var simSystem = testKit.CreateTestProbe();
            var inbox = Inbox.Create(system: testKit.Sys);
            var agentPaths = new ActorPaths(simulationContext: simContext, systemMailBox: inbox.Receiver);
            agentPaths.SetSupervisorAgent(systemAgent: simSystem);
            agentPaths.Guardians.Add(GuardianType.Contract, testKit.CreateTestProbe());
            agentPaths.Guardians.Add(GuardianType.Dispo, testKit.CreateTestProbe());
            agentPaths.Guardians.Add(GuardianType.Production, testKit.CreateTestProbe());
            return agentPaths;
        }

        public AgentMoc(ActorPaths actorPaths, Configuration configuration, long time, bool debug, IActorRef principal, IBehaviour behaviour, GuardianType guardianType)
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {
            _actorPaths = actorPaths;
            _guardianType = guardianType;
            this.InitializeAgent(behaviour: behaviour);
        }
    }
}