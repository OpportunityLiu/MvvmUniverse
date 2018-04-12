namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Represents a command with async executor.
    /// </summary>
    public interface IAsyncCommand : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Indicates whether the command is executing. When <see langword="true"/>, <see cref="System.Windows.Input.ICommand.CanExecute(object)"/> will returns <see langword="false"/>.
        /// </summary>
        bool IsExecuting { get; }
    }

}