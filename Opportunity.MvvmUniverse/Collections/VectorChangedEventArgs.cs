using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        public VectorChangedEventArgs() { }

        public VectorChangedEventArgs(CollectionChange collectionChange) => this.CollectionChange = collectionChange;

        public VectorChangedEventArgs(CollectionChange collectionChange, uint index)
        {
            this.CollectionChange = collectionChange;
            this.Index = index;
        }

        public CollectionChange CollectionChange { get; }

        public uint Index { get; }

        public static IVectorChangedEventArgs Reset { get; } = new VectorChangedEventArgs(CollectionChange.Reset);
    }
}
