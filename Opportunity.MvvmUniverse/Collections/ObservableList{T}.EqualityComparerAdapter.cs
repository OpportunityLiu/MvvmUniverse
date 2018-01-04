using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    partial class ObservableList<T>
    {
        private class EqualityComparerAdapter : IEqualityComparer<T>
        {
            public static IEqualityComparer<T> Create(IComparer<T> comparer)
            {
                if (comparer == null)
                    return null;
                if (comparer is IEqualityComparer<T> ec)
                    return ec;
                return new EqualityComparerAdapter(comparer);
            }

            private EqualityComparerAdapter(IComparer<T> comparer)
            {
                this.comparer = comparer;
            }

            private readonly IComparer<T> comparer;

            public bool Equals(T x, T y) => this.comparer.Compare(x, y) == 0;
            public int GetHashCode(T obj) => obj?.GetHashCode() ?? 0;
        }
    }
}
