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
            OnPropertyChanged(propertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName, addtionalPropertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        protected void ForceSet<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName, addtionalPropertyNames);
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (!NeedRaisePropertyChanged)
                return;
            OnPropertyChanged(new SinglePropertyChangedEventArgsSource(propertyName));
        }

        protected void OnPropertyChanged(string propertyName0, string propertyName1)
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

        protected void OnPropertyChanged(string propertyName0, string propertyName1, string propertyName2)
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

        protected void OnPropertyChanged(string propertyName, IEnumerable<string> propertyNamesRest)
        {
            if (propertyNamesRest == null)
            {
                OnPropertyChanged(propertyName);
                return;
            }
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

        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null || propertyNames.Length == 0)
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

        protected void OnPropertyChanged(IEnumerable<string> propertyNames)
        {
            if (!NeedRaisePropertyChanged)
                return;
            this.OnPropertyChanged(new MultiPropertyChangedEventArgsSource(propertyNames));
        }

        /// <summary>
        /// Tell caller of <see cref="OnPropertyChanged(PropertyChangedEventArgsSource)"/> that whether this call can be skipped.
        /// Returns <c><see cref="PropertyChanged"/> != null</c> by default.
        /// </summary>
        protected virtual bool NeedRaisePropertyChanged => PropertyChanged != null;

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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
