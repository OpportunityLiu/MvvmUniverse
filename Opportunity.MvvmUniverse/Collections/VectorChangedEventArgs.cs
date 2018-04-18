using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        public VectorChangedEventArgs(CollectionChange collectionChange) => CollectionChange = collectionChange;

        public VectorChangedEventArgs(CollectionChange collectionChange, uint index)
        {
            CollectionChange = collectionChange;
            Index = index;
        }

        public CollectionChange CollectionChange { get; }

        public uint Index { get; }

        public static readonly VectorChangedEventArgs Reset = new VectorChangedEventArgs(CollectionChange.Reset);
    }
}
