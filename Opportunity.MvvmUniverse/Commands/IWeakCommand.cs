namespace Opportunity.MvvmUniverse.Commands
{
    public interface IWeakCommand
    {
        bool IsAlive { get; }
    }
}