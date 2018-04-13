using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// Last reentrance request will be handled after current execution, others will be ignored.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class LastQueuedReentrancyHandler<T> : SingleQueuedReentrancyHandler<T>
    {
    }

}