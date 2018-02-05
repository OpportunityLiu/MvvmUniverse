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
            roamingCollcetions.RemoveAll(c => !c.TryGetTarget(out var ignore));
            foreach (var item in roamingCollcetions)
            {
                if (item.TryGetTarget(out var target))
                {
                    target.Populate();
                }
            }
        }

        private static readonly List<WeakReference<IStorageProperty>> roamingCollcetions
            = new List<WeakReference<IStorageProperty>>();
    }
}
