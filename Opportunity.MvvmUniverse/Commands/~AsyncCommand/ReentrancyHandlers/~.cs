namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// Factory methods for <see cref="IReentrancyHandler{T}"/>.
    /// </summary>
    public static class ReentrancyHandler
    {
        internal static class Cache<T>
        {
            public static readonly DisallowedReentrancyHandler<T> Disallowed = new DisallowedReentrancyHandler<T>();
            public static readonly IgnoredReentrancyHandler<T> Ignored = new IgnoredReentrancyHandler<T>();
        }

        /// <summary>
        /// Return cached <see cref="DisallowedReentrancyHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>Cached <see cref="DisallowedReentrancyHandler{T}"/>.</returns>
        public static DisallowedReentrancyHandler<T> Disallowed<T>() => Cache<T>.Disallowed;
        /// <summary>
        /// Return cached <see cref="IgnoredReentrancyHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>Cached <see cref="IgnoredReentrancyHandler{T}"/>.</returns>
        public static IgnoredReentrancyHandler<T> Ignored<T>() => Cache<T>.Ignored;

        /// <summary>
        /// Create new instance of <see cref="QueuedReentrancyHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>New instance of <see cref="QueuedReentrancyHandler{T}"/>.</returns>
        public static QueuedReentrancyHandler<T> Queued<T>() => new QueuedReentrancyHandler<T>();

        /// <summary>
        /// Create new instance of <see cref="LastQueuedReentrancyHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>New instance of <see cref="LastQueuedReentrancyHandler{T}"/>.</returns>
        public static LastQueuedReentrancyHandler<T> LastQueued<T>() => new LastQueuedReentrancyHandler<T>();

        /// <summary>
        /// Create new instance of <see cref="FirstQueuedReentrancyHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>New instance of <see cref="FirstQueuedReentrancyHandler{T}"/>.</returns>
        public static FirstQueuedReentrancyHandler<T> FirstQueued<T>() => new FirstQueuedReentrancyHandler<T>();

        /// <summary>
        /// Create new instance of <see cref="RestartReentrancyHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>New instance of <see cref="RestartReentrancyHandler{T}"/>.</returns>
        public static RestartReentrancyHandler<T> Restart<T>() => new RestartReentrancyHandler<T>();

        /// <summary>
        /// Return cached <see cref="DisallowedReentrancyHandler{T}"/>.
        /// </summary>
        /// <returns>Cached <see cref="DisallowedReentrancyHandler{T}"/>.</returns>
        public static DisallowedReentrancyHandler<object> Disallowed()
            => Cache<object>.Disallowed;
        /// <summary>
        /// Return cached <see cref="IgnoredReentrancyHandler{T}"/>.
        /// </summary>
        /// <returns>Cached <see cref="IgnoredReentrancyHandler{T}"/>.</returns>
        public static IgnoredReentrancyHandler<object> Ignored()
            => Cache<object>.Ignored;

        /// <summary>
        /// Create new instance of <see cref="QueuedReentrancyHandler{T}"/>.
        /// </summary>
        /// <returns>New instance of <see cref="QueuedReentrancyHandler{T}"/>.</returns>
        public static QueuedReentrancyHandler<object> Queued()
            => new QueuedReentrancyHandler<object>();

        /// <summary>
        /// Create new instance of <see cref="LastQueuedReentrancyHandler{T}"/>.
        /// </summary>
        /// <returns>New instance of <see cref="LastQueuedReentrancyHandler{T}"/>.</returns>
        public static LastQueuedReentrancyHandler<object> LastQueued()
            => new LastQueuedReentrancyHandler<object>();

        /// <summary>
        /// Create new instance of <see cref="FirstQueuedReentrancyHandler{T}"/>.
        /// </summary>
        /// <returns>New instance of <see cref="FirstQueuedReentrancyHandler{T}"/>.</returns>
        public static FirstQueuedReentrancyHandler<object> FirstQueued()
            => new FirstQueuedReentrancyHandler<object>();

        /// <summary>
        /// Create new instance of <see cref="RestartReentrancyHandler{T}"/>.
        /// </summary>
        /// <returns>New instance of <see cref="RestartReentrancyHandler{T}"/>.</returns>
        public static RestartReentrancyHandler<object> Restart()
            => new RestartReentrancyHandler<object>();
    }

}