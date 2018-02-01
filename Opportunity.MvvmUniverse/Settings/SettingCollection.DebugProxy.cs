using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Opportunity.MvvmUniverse.Settings
{
    public partial class SettingCollection
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
                public NoStoreMember(ISettingProperty def)
                {
                    Defination = def;
                }

                public T Value => default;

                public object Storage => null;

                private string value()
                {
                    var def = Defination.DefaultValue;
                    if (def == null || def.ToString() == "")
                        return "(Unset)";
                    if (def is string v)
                        return "\"" + v + "\" (Unset)";
                    return def + " (Unset)";
                }

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public ISettingProperty Defination { get; internal set; }

                private string type()
                {
                    return Defination.PropertyType.ToString();
                }

                public override string GetName() => Defination.Name;
            }

            [DebuggerDisplay("{Value}", Name = "{Defination.Name,nq}", Type = "{type(),nq}")]
            internal sealed class DefMember<T> : Member
            {
                public DefMember(object storage, object value, ISettingProperty def)
                {
                    Defination = def;
                    Value = (T)value;
                    Storage = storage;
                }


                public T Value { get; }
                public object Storage { get; }

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public ISettingProperty Defination { get; internal set; }

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

            private IEnumerable<ISettingProperty> getSettingProperties()
            {
                return from field in this.parent.GetType().GetRuntimeFields()
                       where field.IsStatic && typeof(SettingProperty).IsAssignableFrom(field.FieldType)
                       select (ISettingProperty)field.GetValue(null);
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
                        var v = this.parent.GetFromContainer((SettingProperty)def);
                        var type = typeof(DefMember<>).MakeGenericType(def.PropertyType);
                        list.Add((Member)Activator.CreateInstance(type, value.Value, v, def));
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
    }
}
