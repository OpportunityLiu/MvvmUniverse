using System;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Storage
{
    /// <summary>
    /// Mark properies with <see cref="ApplicationData"/> as backend storage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ApplicationSettingAttribute : Attribute
    {
        /// <summary>
        /// Create new instance of <see cref="ApplicationSettingAttribute"/>.
        /// </summary>
        /// <param name="locality">Locality of property.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="locality"/> is neither
        /// <see cref="ApplicationDataLocality.Local"/> nor <see cref="ApplicationDataLocality.Roaming"/>.
        /// </exception>
        public ApplicationSettingAttribute(ApplicationDataLocality locality)
        {
            switch (locality)
            {
            case ApplicationDataLocality.Local:
            case ApplicationDataLocality.Roaming:
                this.Locality = locality;
                break;
            default:
                throw new ArgumentException("Invalid value, should be Local or Roaming.", nameof(locality));
            }
        }

        /// <summary>
        /// Locality of property.
        /// </summary>
        public ApplicationDataLocality Locality { get; }

        /// <summary>
        /// Get serializer for property.
        /// </summary>
        /// <typeparam name="T">Type of property.</typeparam>
        /// <returns>Serializer for property.</returns>
        protected internal virtual ISerializer<T> GetSerializer<T>()
        {
            return Serializer<T>.Default;
        }
    }
}
