using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        private VectorChangedEventArgs(CollectionChange collectionChange) => CollectionChange = collectionChange;

        public VectorChangedEventArgs(CollectionChange collectionChange, uint index)
        {
            CollectionChange = collectionChange;
            Index = index;
        }

        public CollectionChange CollectionChange { get; }

        public uint Index { get; }

        public static IVectorChangedEventArgs Reset { get; } = new VectorChangedEventArgs(CollectionChange.Reset);
    }
}
