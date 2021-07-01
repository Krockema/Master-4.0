using System;

namespace Mate.Ganttplan.ConfirmationSimulator.Environment.Abstractions
{
    /// <summary>
    /// From CLI Commands. Does auto mapping to typed Options.
    /// </summary>
    public interface ICommand
    {
        string ArgLong { get; }
        string ArgShort { get; }
        bool HasProperty { get; }
        string Description { get; }
        /// <summary>
        /// Input is { Configuration, ConfigElementName, ConfigElementValue }
        /// </summary>
        Action<Configuration, string> Action { get; }
    }
}
