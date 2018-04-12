namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Ignore reentrance, instance of this class can be reused between <see cref="IAsyncCommand"/> because no inner status keeps in it.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public sealed class IgnoredReentrancyHandler<T> : IReentrancyHandler<T>
    {
        /// <summary>
        /// Always be <see langword="true"/>.
        /// </summary>
        public bool AllowReenter => true;

        /// <summary>
        /// Do nothing, returns <see langword="false"/>.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <returns><see langword="false"/>.</returns>
        public bool Enqueue(T value) => false;
        /// <summary>
        /// Do nothing, returns <see langword="false"/>.
        /// </summary>
        /// <param name="value">Will be set to default value.</param>
        /// <returns><see langword="false"/>.</returns>
        public bool TryDequeue(out T value)
        {
            value = default;
            return false;
        }

        void IReentrancyHandler<T>.Attach(IAsyncCommand command) { }
        void IReentrancyHandler<T>.Detach() { }
    }

}