using Akka.Actor;
using Akka.Event;
using AkkaSim.Logging;
using NLog;
using LogLevel = NLog.LogLevel;

namespace Mate.Production.Core.Reporting
{
    // A dead letter handling actor specifically for messages of type "DeadLetter"
    public class DeadLetterMonitor :  ReceiveActor
    {
        private Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        public DeadLetterMonitor()
        {
            Receive<DeadLetter>(handler: dl => HandleDeadletter(dl: dl));
        }

        private void HandleDeadletter(DeadLetter dl)
        {
            if (dl.Message.GetType().Name == "AdvanceTo")
            {
                return;
            }
            //TODO Throw unhandled messages ?
            _logger.Log(LogLevel.Trace, message: $"DeadLetter captured: {dl.Message}, sender: {dl.Sender}, recipient: {dl.Recipient}");
        }
    }
}

