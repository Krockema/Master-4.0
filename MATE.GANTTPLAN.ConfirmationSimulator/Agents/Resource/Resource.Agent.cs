using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Ganttplan.ConfirmationSimulator.Environment;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.ResourceAgent
{
    public partial class Resource : Agent
    {
        internal M_Resource _resource;
        // public Constructor
        public static Props Props(ActorPaths actorPaths,Configuration configuration, M_Resource resource, long time, bool debug, IActorRef principal, IActorRef measurementActorRef)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, configuration, resource, time, debug, principal, measurementActorRef));
        }

        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, configuration, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, Configuration configuration, M_Resource resource, long time, bool debug, IActorRef principal, IActorRef measurementActorRef) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {
            _resource = resource;
        }

        public Resource(ActorPaths actorPaths, Configuration configuration, long time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {

        }

        protected override void PostAdvance()
        {
            this.Behaviour.PostAdvance();
        }
    }
}