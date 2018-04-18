using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse
{
    /// <summary>
    /// Base class for observable object.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Token for suspending notification.
        /// </summary>
        [DebuggerDisplay(@"SuspendNotificationCount = {SuspendNotificationCount}")]
        public sealed class NotificationSuspender : IDisposable
        {
            internal int SuspendNotificationCount;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly ObservableObject parent;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool resetOnResuming;

            internal ObservableObject Parent => this.parent;
            internal bool ResetOnResuming => this.resetOnResuming;


            internal NotificationSuspender(ObservableObject parent)
            {
                this.parent = parent;
            }

            internal void Enter(bool resetOnResuming)
            {
                this.resetOnResuming = resetOnResuming;
                Interlocked.Increment(ref this.SuspendNotificationCount);
            }

            void IDisposable.Dispose() => Exit();

            /// <summary>
            /// Resume property changed notification.
            /// </summary>
            public void Exit()
            {
                var resetOnResuming = this.resetOnResuming;
                var n = Interlocked.Decrement(ref this.SuspendNotificationCount);
                if (n < 0)
                    throw new InvalidOperationException("Exit() has been called repeatedly.");
                if (resetOnResuming && n == 0)
                    this.parent.OnObjectReset();
            }
        }

        /// <summary>
        /// Indicates the notification is suspending or not.
        /// </summary>
        protected bool NotificationSuspending
            => this.notificationSuspender is null ? false : this.notificationSuspender.SuspendNotificationCount > 0;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NotificationSuspender notificationSuspender;
        /// <summary>
        /// Suspend notification temporary.
        /// </summary>
        /// <param name="resetOnResuming">Call <see cref="OnObjectReset()"/> on resuming or not.</param>
        /// <returns><see cref="NotificationSuspender"/> for resuming notification.</returns>
        /// <remarks>
        /// Usage:
        /// <c>using(SuspendNotification(<see langword="true"/>)) { ... }</c>
        /// </remarks>
        public NotificationSuspender SuspendNotification(bool resetOnResuming)
        {
            var r = LazyInitializer.EnsureInitialized(ref this.notificationSuspender, () => new NotificationSuspender(this));
            r.Enter(resetOnResuming);
            return r;
        }

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
        /// Call <see cref="OnPropertyReset()"/>.
        /// </summary>
        public virtual void OnObjectReset() => OnPropertyReset();

        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event with empty property name string.
        /// </summary>
        public void OnPropertyReset()
        {
            if (!NeedRaisePropertyChanged)
                return;
            OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
        }

        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">name of changed property</param>
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (!NeedRaisePropertyChanged)
                return;
            if (string.IsNullOrEmpty(propertyName))
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
                return;
            }
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
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
            if (string.IsNullOrEmpty(propertyName0) || string.IsNullOrEmpty(propertyName1))
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
                return;
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName0));
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName1));
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
            if (string.IsNullOrEmpty(propertyName0) || string.IsNullOrEmpty(propertyName1) || string.IsNullOrEmpty(propertyName2))
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
                return;
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName0));
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName1));
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName2));
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
            if (string.IsNullOrEmpty(propertyName))
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
                return;
            }
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            foreach (var item in propertyNamesRest)
            {
                if (string.IsNullOrEmpty(item))
                {
                    OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
                    return;
                }
                OnPropertyChanged(new PropertyChangedEventArgs(item));
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
            if (!NeedRaisePropertyChanged)
                return;
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
            foreach (var item in propertyNames)
            {
                if (string.IsNullOrEmpty(item))
                {
                    OnPropertyChanged(ConstPropertyChangedEventArgs.PropertyReset);
                    return;
                }
                this.OnPropertyChanged(new PropertyChangedEventArgs(item));
            }
        }

        /// <summary>
        /// Tell caller of <see cref="OnPropertyChanged(PropertyChangedEventArgs)"/> that whether this call can be skipped.
        /// <para></para>
        /// Returns <see langword="false"/> if <see cref="SuspendNotification(bool)"/> has been called
        /// or <see cref="PropertyChanged"/> is not registed.
        /// </summary>
        protected virtual bool NeedRaisePropertyChanged => this.propertyChanged.InvocationListLength != 0 && !NotificationSuspending;

        /// <summary>
        /// Raise <see cref="PropertyChanged"/> event
        /// if <see cref="NeedRaisePropertyChanged"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="args">event args</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (!NeedRaisePropertyChanged)
                return;
            this.propertyChanged.Raise(this, args);
        }

        private readonly DepedencyEvent<PropertyChangedEventHandler, ObservableObject, PropertyChangedEventArgs> propertyChanged
            = new DepedencyEvent<PropertyChangedEventHandler, ObservableObject, PropertyChangedEventArgs>((h, s, e) => h(s, e));
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => this.propertyChanged.Add(value);
            remove => this.propertyChanged.Remove(value);
        }
    }
}
