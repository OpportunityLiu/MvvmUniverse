namespace Opportunity.MvvmUniverse.Commands
{
    public interface ICommandWithProgress<TProgress> : IAsyncCommand
    {
        TProgress Progress { get; }
        double NormalizedProgress { get; }
    }
}