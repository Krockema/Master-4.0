using Akka.Actor;
using AkkaSim.Definitions;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using static Mate.Production.Core.Agents.SupervisorAgent.Supervisor.Instruction;

namespace Mate.Ganttplan.ConfirmationSimulator.Agents.SupervisorAgent.Behaviour
{
    public class Central : Types.Behaviour
    {
        private GanttPlanDBContext _ganttContext { get; set; }
        private DbConnection _dataBaseConnection { get; set; }
        private IMessageHub _messageHub { get; set; }
        private long _simulationEnds { get; set; }
        private decimal _transitionFactor { get; set; }
        public Central(string dbNameGantt
            , IMessageHub messageHub
            , Configuration configuration)
        {
            _ganttContext = Dbms.GetGanttDataBase(dbName: dbNameGantt).DbContext;
            _dataBaseConnection = _ganttContext.Database.GetDbConnection();
            _messageHub = messageHub;
            _simulationEnds = configuration.GetOption<SimulationEnd>().Value;
            _transitionFactor = configuration.GetOption<TransitionFactor>().Value;

        }
        public override bool Action(object message)
        {
            switch (message)
            {
                case BasicInstruction.ChildRef instruction: OnChildAdd(childRef: instruction.GetObjectFromMessage); break;
                // ToDo Benammung : Sollte die Letzte nachricht zwischen Produktionsagent und Contract Agent abfangen und Inital bei der ersten Forward Terminierung setzen
                case EndSimulation instruction: End(); break;
                default: throw new Exception(message: "Invalid Message Object.");
            }

            return true;
        }
        public override bool AfterInit()
        {
            Agent.Send(instruction: EndSimulation.Create(message: true, target: Agent.Context.Self), waitFor: _simulationEnds);
            Agent.Send(instruction: Supervisor.Instruction.SystemCheck.Create(message: "CheckForOrders", target: Agent.Context.Self), waitFor: 1);
            Agent.DebugMessage(msg: "Agent-System ready for Work");
            return true;
        }
        
        /// <summary>
        /// After a child has been ordered from Guardian a ChildRef will be returned by the responsible child
        /// it has been allready added to this.VirtualChilds at this Point
        /// </summary>
        /// <param name="childRef"></param>
        public override void OnChildAdd(IActorRef childRef)
        {
            Agent.VirtualChildren.Add(item: childRef);
        }

        private void End()
        {
            Agent.DebugMessage(msg: "End Sim");
            Agent.ActorPaths.SimulationContext.Ref.Tell(message: SimulationMessage.SimulationState.Finished);
        }
      
    }
}
