using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Settings
{
    [DebuggerTypeProxy(typeof(DebugProxy))]
    public class SettingCollection : ObservableObject
    {
        private class DebugProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly SettingCollection parent;

            public DebugProxy(SettingCollection c)
            {
                this.parent = c;
            }

            internal abstract class Member
            {
                public abstract string GetName();
            }

            [DebuggerDisplay("{value(),nq}", Name = "{Defination.Name,nq}", Type = "{type(),nq}")]
            internal sealed class NoStoreMember<T> : Member
            {
                public NoStoreMember(SettingProperty def)
                {
                    Defination = def;
                }

                public T Value => default;

                private string value()
                {
                    var def = Defination.GetDefault();
                    if (def == null || def.ToString() == "")
                        return "(Unset)";
                    if (def is string v)
                        return "\"" + v + "\" (Unset)";
                    return def + " (Unset)";
                }

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public SettingProperty Defination { get; internal set; }

                private string type()
                {
                    return Defination.PropertyType.ToString();
                }

                public override string GetName() => Defination.Name;
            }

            [DebuggerDisplay("{Value}", Name = "{Defination.Name,nq}", Type = "{type(),nq}")]
            internal sealed class DefMember<T> : Member
            {
                public DefMember(object value, SettingProperty def)
                {
                    Defination = def;
                    Value = (T)value;
                }

                public T Value { get; internal set; }

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public SettingProperty Defination { get; internal set; }

                private string type()
                {
                    var valueType = Value?.GetType() ?? Defination.PropertyType;
                    if (valueType == Defination.PropertyType)
                        return Defination.PropertyType.ToString();
                    else
                        return $"{Defination.PropertyType} {{{valueType}}})";
                }

                public override string GetName() => Defination.Name;
            }

            [DebuggerDisplay("{Value}", Name = "{Name,nq}", Type = "{type(),nq}")]
            internal sealed class NoDefMember : Member
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string Name { get; internal set; }
                public object Value { get; internal set; }

                private string type()
                {
                    if (Value == null)
                        return "System.Object";
                    return Value.GetType().ToString();
                }

                public NoDefMember(string name, object value)
                {
                    Name = name;
                    Value = value;
                }

                public override string GetName() => Name;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            internal Member[] Items => build();

            private IEnumerable<SettingProperty> getSettingProperties()
            {
                return from field in this.parent.GetType().GetRuntimeFields()
                       where field.IsStatic && typeof(SettingProperty).IsAssignableFrom(field.FieldType)
                       select (SettingProperty)field.GetValue(null);
            }

            private Member[] build()
            {
                var values = this.parent.Container.Values.ToList();
                var defs = getSettingProperties().ToList();
                var list = new List<Member>(values.Count);
                for (var i = 0; i < values.Count; i++)
                {
                    var value = values[i];
                    var defI = defs.FindIndex(d => d.Name == value.Key);
                    if (defI >= 0)
                    {
                        var def = defs[defI];
                        defs.RemoveAt(defI);
                        var v = this.parent.GetFromContainer(def);
                        var type = typeof(DefMember<>).MakeGenericType(def.PropertyType);
                        list.Add((Member)Activator.CreateInstance(type, v, def));
                    }
                    else
                    {
                        list.Add(new NoDefMember(value.Key, value.Value));
                    }
                }
                foreach (var item in defs)
                {
                    var type = typeof(NoStoreMember<>).MakeGenericType(item.PropertyType);
                    list.Add((Member)Activator.CreateInstance(type, item));
                }
                list.Sort((m1, m2) => string.Compare(m1.GetName(), m2.GetName()));
                return list.ToArray();
            }
        }

        static SettingCollection()
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
                    target.RoamingDataChanged();
                }
            }
        }

        protected virtual void RoamingDataChanged()
        {
            OnPropertyChanged((string)null);
        }

        private static readonly List<WeakReference<SettingCollection>> roamingCollcetions
            = new List<WeakReference<SettingCollection>>();

        public SettingCollection(ApplicationDataContainer container)
        {
            this.Container = container;
            if (container.Locality == ApplicationDataLocality.Roaming)
                roamingCollcetions.Add(new WeakReference<SettingCollection>(this));
        }

        private static ApplicationDataContainer create(ApplicationDataContainer parent, string containerName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));
            return parent.CreateContainer(containerName, ApplicationDataCreateDisposition.Always);
        }

        public SettingCollection(ApplicationDataContainer parent, string containerName)
            : this(create(parent, containerName)) { }

        private static ApplicationDataContainer create(SettingCollection parent, string containerName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (parent.Container == null)
                throw new ArgumentException("Container of given SettingCollection is null", nameof(parent));
            return create(parent.Container, containerName);
        }

        public SettingCollection(SettingCollection parent, string containerName)
            : this(create(parent, containerName)) { }

        protected ApplicationDataContainer Container { get; }

        protected T GetFromContainer<T>(SettingProperty<T> property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            try
            {
                if (this.Container.Values.TryGetValue(property.Name, out var v))
                {
                    return (T)deserializeValue(property, v);
                }
            }
            catch { }
            return property.DefaultValue;
        }

        protected object GetFromContainer(SettingProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            try
            {
                if (this.Container.Values.TryGetValue(property.Name, out var v))
                {
                    var r = deserializeValue(property, v);
                    if (property.TestValue(r))
                        return r;
                }
            }
            catch { }
            return property.GetDefault();
        }

        protected bool SetToContainer<T>(SettingProperty<T> property, T value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var old = GetFromContainer(property);
            if (this.Container.Values.ContainsKey(property.Name) && property.Equals(old, value))
            {
                return false;
            }
            setToContainerCore(property, old, value);
            return true;
        }

        protected bool SetToContainer(SettingProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (!property.TestValue(value))
                throw new ArgumentException("Type of value doesn't match property.PropertyType.");
            var old = GetFromContainer(property);
            if (this.Container.Values.ContainsKey(property.Name) && property.Equals(old, value))
            {
                return false;
            }
            setToContainerCore(property, old, value);
            return true;
        }

        protected void ForceSetToContainer<T>(SettingProperty<T> property, T value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            var old = GetFromContainer(property);
            setToContainerCore(property, old, value);
        }

        protected void ForceSetToContainer(SettingProperty property, object value)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (!property.TestValue(value))
                throw new ArgumentException("Type of value doesn't match property.PropertyType.");
            var old = GetFromContainer(property);
            setToContainerCore(property, old, value);
        }

        private void setToContainerCore<T>(SettingProperty<T> property, T old, T value)
        {
            this.Container.Values[property.Name] = serializeValue(property, value);
            OnPropertyChanged(property.Name);
            property.RaisePropertyChanged(this, old, value);
        }

        private void setToContainerCore(SettingProperty property, object old, object value)
        {
            this.Container.Values[property.Name] = serializeValue(property, value);
            OnPropertyChanged(property.Name);
            property.RaisePropertyChanged(this, old, value);
        }

        private static object serializeValue(SettingProperty property, object value)
        {
            if (value is Enum e)
                return e.ToUInt64();
            else
                return value;
        }

        private object deserializeValue(SettingProperty property, object value)
        {
            if (property.GetTypeDefault() is Enum)
                return Enum.ToObject(property.PropertyType, value);
            return value;
        }
    }
}
