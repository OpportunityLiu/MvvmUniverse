using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    public class LocalStorageObject : StorageObject
    {
        private static ApplicationDataContainer check(ApplicationDataContainer container, string propertyName)
        {
            if (container == null)
                throw new ArgumentNullException(propertyName);
            if (container.Locality == ApplicationDataLocality.Roaming)
                throw new ArgumentException("Only supports local ApplicationDataContainer", propertyName);
            return container;
        }

        public LocalStorageObject(ApplicationDataContainer container) : base(check(container, nameof(container))) { }

        public LocalStorageObject(ApplicationDataContainer parent, string containerName) : base(check(parent, nameof(parent)), containerName) { }

        public LocalStorageObject(LocalStorageObject parent, string containerName) : base(parent, containerName) { }
    }
}
