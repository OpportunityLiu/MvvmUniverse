using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    public class RoamingStorageObject : StorageObject
    {
        #region RoamingDataChanged
        static RoamingStorageObject()
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
                    target.OnRoamingDataChanged();
                }
            }
        }

        private static readonly List<WeakReference<RoamingStorageObject>> roamingCollcetions
            = new List<WeakReference<RoamingStorageObject>>();

        protected virtual void OnRoamingDataChanged()
        {
            OnPropertyChanged((string)null);
        }
        #endregion RoamingDataChanged

        private static ApplicationDataContainer check(ApplicationDataContainer container, string propertyName)
        {
            if (container == null)
                throw new ArgumentNullException(propertyName);
            if (container.Locality != ApplicationDataLocality.Roaming)
                throw new ArgumentException("Only supports roaming ApplicationDataContainer", propertyName);
            return container;
        }

        public RoamingStorageObject(ApplicationDataContainer container) : base(check(container, nameof(container))) { }

        public RoamingStorageObject(ApplicationDataContainer parent, string containerName) : base(check(parent, nameof(parent)), containerName) { }

        public RoamingStorageObject(RoamingStorageObject parent, string containerName) : base(parent, containerName) { }
    }
}
