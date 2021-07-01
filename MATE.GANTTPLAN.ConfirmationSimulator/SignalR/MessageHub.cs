using Mate.Ganttplan.ConfirmationSimulator.SignalR.Messages;
using Microsoft.AspNetCore.SignalR;

namespace Mate.Ganttplan.ConfirmationSimulator.SignalR
{
    public class MessageHub : Hub, IMessageHub
    {
        protected IHubContext<MessageHub> _hubContext;
        public MessageHub(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SendToAllClients(string msg, MessageType msgType = MessageType.info)
        {
            this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: ReturnMsgBox(msg: msg, type: msgType));
        }

        public string ReturnMsgBox(string msg, MessageType type)
        {
            return "<div class=\"alert alert-" + type + "\">" + msg + "</div>";
        }

        public void StartSimulation(string simId, string simNumber)
        {
            this._hubContext.Clients.All.SendAsync(method: "workerListener", arg1:System.Text.Json.JsonSerializer.Serialize(new[] { "SimulationStart", simId, simNumber } ));
        }

        public void EndSimulation(string text,string simId,string simNumber)
        {
            this._hubContext.Clients.All.SendAsync(method: "workerListener", arg1: System.Text.Json.JsonSerializer.Serialize(value: new[] { "SimulationComplete", simId, simNumber } ));
        }

        public void ProcessingUpdate(int simId, int counter, string simType, int max)
        {
            // this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: "ProcessingUpdate", arg2: simId, arg3: Math.Round(value: (double)counter / max * 100, digits: 0).ToString(), arg4: simType.ToString() );
        }

        public void SendToClient(string listener, string msg, MessageType msgType = MessageType.info)
        {
            this._hubContext.Clients.All.SendAsync(method: listener, arg1: msg);
        }

        public void GuardianState(object msg)
        {
            this._hubContext.Clients.All.SendAsync(method: "clientListener", arg1: System.Text.Json.JsonSerializer.Serialize(msg));
        }
    }
}