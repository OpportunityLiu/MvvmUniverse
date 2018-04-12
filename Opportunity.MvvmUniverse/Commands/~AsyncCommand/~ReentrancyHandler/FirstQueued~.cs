namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// First reentrance request will be handled after current execution, others will be ignored.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class FirstQueuedReentrancyHandler<T> : SingleQueuedReentrancyHandler<T>
    {
        /// <summary>
        /// Call <see cref="SingleQueuedReentrancyHandler{T}.Enqueue(T)"/> only if <see cref="SingleQueuedReentrancyHandler{T}.HasValue"/> is false.
        /// </summary>
        /// <param name="value">The parameter of reentered execution.</param>
        /// <returns><see langword="false"/>.</returns>
        public override bool Enqueue(T value)
        {
            if (!HasValue)
                base.Enqueue(value);
            return false;
        }
    }

}