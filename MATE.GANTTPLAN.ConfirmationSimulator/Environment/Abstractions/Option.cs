using System;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions
{
    public abstract class Option<T> : IOption<T>, ICommand
    {
        protected T _value;
        public T Value => _value;
        public string ArgLong => this.GetType().Name;
        public string ArgShort => this.GetType().Name;
        public bool HasProperty => true; 
        public string Description { get; }
        public Action<Configuration, string> Action { get; set; }
    }
}
