using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    public static class RoamingStoragePropertyManager
    {
        static RoamingStoragePropertyManager()
        {
            ApplicationData.Current.DataChanged += applicationDataChanged;
        }

        private static void applicationDataChanged(ApplicationData sender, object args)
        {
            RoamingProperties.RemoveAll(c => !c.TryGetTarget(out var ignore));
            foreach (var item in RoamingProperties)
            {
                if (item.TryGetTarget(out var target))
                {
                    target.Populate();
                }
            }
        }

        internal static readonly List<WeakReference<IStorageProperty>> RoamingProperties
            = new List<WeakReference<IStorageProperty>>();
    }
}
