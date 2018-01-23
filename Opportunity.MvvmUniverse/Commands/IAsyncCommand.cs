namespace Opportunity.MvvmUniverse.Commands
{
    public interface IAsyncCommand
    {
        bool IsExecuting { get; }
    }
}