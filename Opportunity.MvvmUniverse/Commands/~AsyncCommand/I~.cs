namespace Opportunity.MvvmUniverse.Commands
{
    public interface IAsyncCommand : System.Windows.Input.ICommand
    {
        bool IsExecuting { get; }
    }
}