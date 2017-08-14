using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(addtionalPropertyName, ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(addtionalPropertyName0, addtionalPropertyName1, ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(addtionalPropertyNames, ref field, value, propertyName);
            return true;
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            if (PropertyChanged == null)
                return;
            OnPropertyChanged(propertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            if (PropertyChanged == null)
                return;
            OnPropertyChanged(propertyName, addtionalPropertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            if (PropertyChanged == null)
                return;
            OnPropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        protected void ForceSet<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            if (PropertyChanged == null)
                return;
            IEnumerable<string> g()
            {
                yield return propertyName;
                if (addtionalPropertyNames == null)
                    yield break;
                foreach (var item in addtionalPropertyNames)
                {
                    yield return item;
                }
            }
            OnPropertyChanged(g());
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged == null)
                return;
            OnPropertyChanged(new SinglePropertyChangedEventArgsSource(propertyName));
        }

        protected void OnPropertyChanged(string propertyName0, string propertyName1)
        {
            if (PropertyChanged == null)
                return;
            IEnumerable<string> g()
            {
                yield return propertyName0;
                yield return propertyName1;
            }
            OnPropertyChanged(g());
        }

        protected void OnPropertyChanged(string propertyName0, string propertyName1, string propertyName2)
        {
            if (PropertyChanged == null)
                return;
            IEnumerable<string> g()
            {
                yield return propertyName0;
                yield return propertyName1;
                yield return propertyName2;
            }
            OnPropertyChanged(g());
        }

        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged == null)
                return;
            this.OnPropertyChanged(new MultiPropertyChangedEventArgsSource(propertyNames));
        }

        protected void OnPropertyChanged(IEnumerable<string> propertyNames)
        {
            if (PropertyChanged == null)
                return;
            this.OnPropertyChanged(new MultiPropertyChangedEventArgsSource(propertyNames));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgsSource args)
        {
            if (args == null)
                return;
            var temp = PropertyChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                foreach (var item in args)
                {
                    temp(this, item);
                }
            });
        }
    }

    public abstract class PropertyChangedEventArgsSource : IEnumerable<PropertyChangedEventArgs>
    {
        public abstract IEnumerator<PropertyChangedEventArgs> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected class EditablePropertyChangedEventArgs : PropertyChangedEventArgs
        {
            public EditablePropertyChangedEventArgs() : base(null) { }

            public override string PropertyName => this.pName;

            private string pName;

            public void SetpropertyName(string propertyName) => this.pName = propertyName;
        }
    }

    public sealed class SinglePropertyChangedEventArgsSource : PropertyChangedEventArgsSource
    {
        private readonly string name;

        public SinglePropertyChangedEventArgsSource(string propertyName)
        {
            this.name = propertyName;
        }

        public override IEnumerator<PropertyChangedEventArgs> GetEnumerator()
        {
            yield return new PropertyChangedEventArgs(this.name);
        }
    }

    public sealed class MultiPropertyChangedEventArgsSource : PropertyChangedEventArgsSource
    {
        public MultiPropertyChangedEventArgsSource(IEnumerable<string> propertyNames)
        {
            this.names = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
        }

        private readonly IEnumerable<string> names;
        public override IEnumerator<PropertyChangedEventArgs> GetEnumerator() => new MultiPropertyChangedEventArgsEnumerator(this.names);

        private sealed class MultiPropertyChangedEventArgsEnumerator : IEnumerator<PropertyChangedEventArgs>
        {
            public MultiPropertyChangedEventArgsEnumerator(IEnumerable<string> propertyNames)
            {
                this.enumrator = propertyNames.GetEnumerator();
            }

            private IEnumerator<string> enumrator;

            private EditablePropertyChangedEventArgs args = new EditablePropertyChangedEventArgs();
            public PropertyChangedEventArgs Current
            {
                get
                {
                    this.args.SetpropertyName(this.enumrator.Current);
                    return this.args;
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext() => this.enumrator.MoveNext();

            public void Reset() => this.enumrator.Reset();

            public void Dispose() => this.enumrator.Dispose();
        }
    }
}
