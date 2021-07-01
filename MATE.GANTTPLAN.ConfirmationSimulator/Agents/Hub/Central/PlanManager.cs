namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    public class PlanManager
    {
        public int PlanVersion { get; private set; }

        public PlanManager()
        {
            PlanVersion = 0; // start at first cycle, ++ after each GanttPlan synchronization
        }

        public int IncrementPlaningNumber()
        {
            PlanVersion++;
            return PlanVersion;
        }


    }
}
