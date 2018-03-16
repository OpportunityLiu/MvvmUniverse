using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Opportunity.MvvmUniverse
{
    /// <summary>
    /// A group of <see cref="PropertyChangedEventArgs"/>.
    /// </summary>
    public abstract class PropertyChangedEventArgsSource : IEnumerable<PropertyChangedEventArgs>
    {
        /// <inheritdoc/>
        public abstract IEnumerator<PropertyChangedEventArgs> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// An <see cref="PropertyChangedEventArgs"/> whose <see cref="PropertyName"/> can be edited.
        /// </summary>
        protected class EditablePropertyChangedEventArgs : PropertyChangedEventArgs
        {
            /// <summary>
            /// Create new instance of <see cref="EditablePropertyChangedEventArgs"/>.
            /// </summary>
            public EditablePropertyChangedEventArgs() : base(null) { }

            /// <inheritdoc/>
            public override string PropertyName => this.pName;

            private string pName;

            /// <summary>
            /// Modify <see cref="PropertyName"/>.
            /// </summary>
            /// <param name="propertyName">New property name.</param>
            public void SetpropertyName(string propertyName) => this.pName = propertyName;
        }
    }

    /// <summary>
    /// <see cref="PropertyChangedEventArgsSource"/> with single item in it.
    /// </summary>
    public sealed class SinglePropertyChangedEventArgsSource : PropertyChangedEventArgsSource
    {
        private readonly PropertyChangedEventArgs args;

        /// <summary>
        /// Create new instance of <see cref="SinglePropertyChangedEventArgsSource"/>.
        /// </summary>
        /// <param name="propertyName">Property name of the only item.</param>
        public SinglePropertyChangedEventArgsSource(string propertyName)
            : this(new PropertyChangedEventArgs(propertyName)) { }

        /// <summary>
        /// Create new instance of <see cref="SinglePropertyChangedEventArgsSource"/>.
        /// </summary>
        /// <param name="args">The only item.</param>
        public SinglePropertyChangedEventArgsSource(PropertyChangedEventArgs args)
        {
            this.args = args ?? throw new ArgumentNullException(nameof(args));
        }

        /// <inheritdoc/>
        public override IEnumerator<PropertyChangedEventArgs> GetEnumerator()
        {
            yield return this.args;
        }
    }

    /// <summary>
    /// Implement of <see cref="PropertyChangedEventArgsSource"/>.
    /// </summary>
    public sealed class MultiPropertyChangedEventArgsSource : PropertyChangedEventArgsSource
    {
        /// <summary>
        /// Create new instance of <see cref="MultiPropertyChangedEventArgsSource"/>.
        /// </summary>
        /// <param name="propertyNames">Property names of the items.</param>
        public MultiPropertyChangedEventArgsSource(IEnumerable<string> propertyNames)
        {
            this.names = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
        }

        private readonly IEnumerable<string> names;
        /// <inheritdoc/>
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
