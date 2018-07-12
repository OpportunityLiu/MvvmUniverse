using Windows.UI.Xaml.Data;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Event args for <see cref="CollectionView{T}.CurrentChanging"/>
    /// </summary>
    public sealed class CurrentChangingEventArgs<T> : CurrentChangingEventArgs
    {
        internal CurrentChangingEventArgs(bool isCancelable, int oldPosition, T oldItem, int newPosition, T newItem)
            : base(isCancelable)
        {
            this.OldPosition = oldPosition;
            this.OldItem = oldItem;
            this.NewPosition = newPosition;
            this.NewItem = newItem;
        }

        /// <summary>
        /// Old value of <see cref="CollectionView{T}.CurrentPosition"/>.
        /// </summary>
        public int OldPosition { get; }
        /// <summary>
        /// Old value of <see cref="CollectionView{T}.CurrentItem"/>.
        /// </summary>
        public T OldItem { get; }
        /// <summary>
        /// New value of <see cref="CollectionView{T}.CurrentPosition"/>.
        /// </summary>
        public int NewPosition { get; }
        /// <summary>
        /// New value of <see cref="CollectionView{T}.CurrentItem"/>.
        /// </summary>
        public T NewItem { get; }
    }

    /// <summary>
    /// Event args for <see cref="CollectionView{T}.CurrentChanged"/>
    /// </summary>
    public sealed class CurrentChangedEventArgs<T>
    {
        internal CurrentChangedEventArgs(int oldPosition, T oldItem, int newPosition, T newItem)
        {
            this.OldPosition = oldPosition;
            this.OldItem = oldItem;
            this.NewPosition = newPosition;
            this.NewItem = newItem;
        }

        /// <summary>
        /// Old value of <see cref="CollectionView{T}.CurrentPosition"/>.
        /// </summary>
        public int OldPosition { get; }
        /// <summary>
        /// Old value of <see cref="CollectionView{T}.CurrentItem"/>.
        /// </summary>
        public T OldItem { get; }
        /// <summary>
        /// New value of <see cref="CollectionView{T}.CurrentPosition"/>.
        /// </summary>
        public int NewPosition { get; }
        /// <summary>
        /// New value of <see cref="CollectionView{T}.CurrentItem"/>.
        /// </summary>
        public T NewItem { get; }
    }
}
