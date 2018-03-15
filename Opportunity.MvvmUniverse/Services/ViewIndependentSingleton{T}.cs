using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Services
{
    internal static class ViewIndependentSingleton<T>
        where T : class
    {
        private static int count;
        public static int Count => count;

        [ThreadStatic]
        private static T v;
        public static T Value
        {
            get => v;
            set
            {
                var old = Interlocked.Exchange(ref v, value);
                if (old is null)
                    Interlocked.Increment(ref count);
                if (value is null)
                    Interlocked.Decrement(ref count);
            }
        }
    }
}
