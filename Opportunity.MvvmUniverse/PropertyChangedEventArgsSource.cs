using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Opportunity.MvvmUniverse
{
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
