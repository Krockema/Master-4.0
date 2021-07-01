namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions
{ 
    public interface IOption<out T> 
    {
        T Value { get; }
    }
}