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
    /// <summary>
    /// Base class for observable object.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// If <paramref name="value"/> and <paramref name="field"/> are different,
        /// set <paramref name="field"/> with <paramref name="value"/> and notify.
        /// Use default <see cref="EqualityComparer{T}"/> to compare.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        /// <returns>Whether <paramref name="value"/> has set to <paramref name="field"/> or not.</returns>
        protected bool Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
        }

        /// <summary>
        /// If <paramref name="value"/> and <paramref name="field"/> are different,
        /// set <paramref name="field"/> with <paramref name="value"/> and notify.
        /// Use default <see cref="EqualityComparer{T}"/> to compare.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="addtionalPropertyName">name of addtional property need to notify</param>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        /// <returns>Whether <paramref name="value"/> has set to <paramref name="field"/> or not.</returns>
        protected bool Set<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(addtionalPropertyName, ref field, value, propertyName);
            return true;
        }

        /// <summary>
        /// If <paramref name="value"/> and <paramref name="field"/> are different,
        /// set <paramref name="field"/> with <paramref name="value"/> and notify.
        /// Use default <see cref="EqualityComparer{T}"/> to compare.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="addtionalPropertyName0">name of first addtional property need to notify</param>
        /// <param name="addtionalPropertyName1">name of second addtional property need to notify</param>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        /// <returns>Whether <paramref name="value"/> has set to <paramref name="field"/> or not.</returns>
        protected bool Set<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(addtionalPropertyName0, addtionalPropertyName1, ref field, value, propertyName);
            return true;
        }

        /// <summary>
        /// If <paramref name="value"/> and <paramref name="field"/> are different,
        /// set <paramref name="field"/> with <paramref name="value"/> and notify.
        /// Use default <see cref="EqualityComparer{T}"/> to compare.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="addtionalPropertyNames">names of addtional properties need to notify</param>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        /// <returns>Whether <paramref name="value"/> has set to <paramref name="field"/> or not.</returns>
        protected bool Set<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(field, value))
                return false;
            ForceSet(addtionalPropertyNames, ref field, value, propertyName);
            return true;
        }

        /// <summary>
        /// Set <paramref name="field"/> with <paramref name="value"/> and notify,
        /// regardless <paramref name="value"/> and <paramref name="field"/> are equal or not.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        protected void ForceSet<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Set <paramref name="field"/> with <paramref name="value"/> and notify,
        /// regardless <paramref name="value"/> and <paramref name="field"/> are equal or not.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="addtionalPropertyName">name of addtional property need to notify</param>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        protected void ForceSet<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName, addtionalPropertyName);
        }

        /// <summary>
        /// Set <paramref name="field"/> with <paramref name="value"/> and notify,
        /// regardless <paramref name="value"/> and <paramref name="field"/> are equal or not.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="addtionalPropertyName0">name of first addtional property need to notify</param>
        /// <param name="addtionalPropertyName1">name of second addtional property need to notify</param>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        protected void ForceSet<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        /// <summary>
        /// Set <paramref name="field"/> with <paramref name="value"/> and notify,
        /// regardless <paramref name="value"/> and <paramref name="field"/> are equal or not.
        /// </summary>
        /// <typeparam name="TProp">type of property</typeparam>
        /// <param name="addtionalPropertyNames">names of addtional properties need to notify</param>
        /// <param name="field">backing field of property</param>
        /// <param name="value">new value</param>
        /// <param name="propertyName">name of property, used for <see cref="PropertyChanged"/> event</param>
        protected void ForceSet<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName, addtionalPropertyNames);
        }

        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">name of changed property</param>
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (!NeedRaisePropertyChanged)
                return;
            OnPropertyChanged(new SinglePropertyChangedEventArgsSource(propertyName));
        }
        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName0">first name of changed property</param>
        /// <param name="propertyName1">second name of changed property</param>
        public void OnPropertyChanged(string propertyName0, string propertyName1)
        {
            if (!NeedRaisePropertyChanged)
                return;
            OnPropertyChanged(g(propertyName0, propertyName1));
            IEnumerable<string> g(string p0, string p1)
            {
                yield return p0;
                yield return p1;
            }
        }
        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName0">first name of changed property</param>
        /// <param name="propertyName1">second name of changed property</param>
        /// <param name="propertyName2">third name of changed property</param>
        public void OnPropertyChanged(string propertyName0, string propertyName1, string propertyName2)
        {
            if (!NeedRaisePropertyChanged)
                return;
            OnPropertyChanged(g(propertyName0, propertyName1, propertyName2));
            IEnumerable<string> g(string p0, string p1, string p2)
            {
                yield return p0;
                yield return p1;
                yield return p2;
            }
        }
        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">first name of changed property</param>
        /// <param name="propertyNamesRest">rest names of changed property</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyNamesRest"/> is <see langword="null"/></exception>
        public void OnPropertyChanged(string propertyName, IEnumerable<string> propertyNamesRest)
        {
            if (propertyNamesRest == null)
                throw new ArgumentNullException(nameof(propertyNamesRest));
            if (!NeedRaisePropertyChanged)
                return;
            this.OnPropertyChanged(new MultiPropertyChangedEventArgsSource(g(propertyName, propertyNamesRest)));
            IEnumerable<string> g(string p, IEnumerable<string> pRest)
            {
                yield return p;
                foreach (var item in pRest)
                {
                    yield return item;
                }
            }
        }
        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyNames">names of changed property</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyNames"/> is <see langword="null"/></exception>
        public void OnPropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null)
                throw new ArgumentNullException(nameof(propertyNames));
            if (propertyNames.Length == 0)
                return;
            else if (propertyNames.Length == 1)
                OnPropertyChanged(propertyNames[0]);
            else if (propertyNames.Length == 2)
                OnPropertyChanged(propertyNames[0], propertyNames[1]);
            else if (propertyNames.Length == 3)
                OnPropertyChanged(propertyNames[0], propertyNames[1], propertyNames[2]);
            else
                OnPropertyChanged((IEnumerable<string>)propertyNames);
        }
        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyNames">names of changed property</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyNames"/> is <see langword="null"/></exception>
        public void OnPropertyChanged(IEnumerable<string> propertyNames)
        {
            if (propertyNames == null)
                throw new ArgumentNullException(nameof(propertyNames));
            if (!NeedRaisePropertyChanged)
                return;
            this.OnPropertyChanged(new MultiPropertyChangedEventArgsSource(propertyNames));
        }

        /// <summary>
        /// Tell caller of <see cref="OnPropertyChanged(IEnumerable{PropertyChangedEventArgs})"/> that whether this call can be skipped.
        /// <para></para>
        /// Returns <c><see cref="PropertyChanged"/> != <see langword="null"/></c> by default.
        /// </summary>
        protected virtual bool NeedRaisePropertyChanged => PropertyChanged != null;

        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="args">event args</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        /// <remarks>Will use <see cref="DispatcherHelper"/> to raise event on UI thread
        /// if <see cref="DispatcherHelper.UseForNotification"/> is <see langword="true"/>.</remarks>
        protected virtual void OnPropertyChanged(IEnumerable<PropertyChangedEventArgs> args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
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

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
