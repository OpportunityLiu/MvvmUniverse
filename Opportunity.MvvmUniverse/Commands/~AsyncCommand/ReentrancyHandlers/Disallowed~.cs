using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// Disallow reentrance, instance of this class can be reused between <see cref="IAsyncCommand"/> because no inner status keeps in it.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    [DebuggerDisplay("Disallowed")]
    public sealed class DisallowedReentrancyHandler<T> : IReentrancyHandler<T>
    {
        /// <summary>
        /// Always be <see langword="false"/>.
        /// </summary>
        public bool AllowReenter => false;

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

        /// <summary>
        /// Compare <see cref="object.GetType()"/>.
        /// </summary>
        /// <param name="obj">object to compare with.</param>
        /// <returns><see langword="true"/> if this and <paramref name="obj"/> has same type.</returns>
        public override bool Equals(object obj) => this.GetType().Equals(obj?.GetType());

        /// <summary>
        /// Return hash code of <see cref="object.GetType()"/>.
        /// </summary>
        /// <returns>Hash code of <see cref="object.GetType()"/>.</returns>
        public override int GetHashCode() => this.GetType().GetHashCode();

        void IReentrancyHandler<T>.Attach(IAsyncCommand command) { }
        void IReentrancyHandler<T>.Detach() { }
    }

}