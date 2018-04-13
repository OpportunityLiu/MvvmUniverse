namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// Cancel current execution when reentrance happens.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class RestartReentrancyHandler<T> : SingleQueuedReentrancyHandler<T>
    {
        /// <summary>
        /// Returns <see langword="true"/>.
        /// </summary>
        /// <param name="value">The parameter of reentered execution.</param>
        /// <returns><see langword="true"/>.</returns>
        public override bool Enqueue(T value)
        {
            base.Enqueue(value);
            return true;
        }
    }

}