using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Opportunity.MvvmUniverse.Helpers;

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
            RaisePropertyChanged(propertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName);
        }

        protected void ForceSet<TProp>(string addtionalPropertyName0, string addtionalPropertyName1, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        protected void ForceSet<TProp>(IEnumerable<string> addtionalPropertyNames, ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
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
            RaisePropertyChanged(g());
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            IEnumerable<string> g()
            {
                yield return propertyName;
            }
            RaisePropertyChanged(g());
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1)
        {
            IEnumerable<string> g()
            {
                yield return propertyName0;
                yield return propertyName1;
            }
            RaisePropertyChanged(g());
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1, string propertyName2)
        {
            IEnumerable<string> g()
            {
                yield return propertyName0;
                yield return propertyName1;
                yield return propertyName2;
            }
            RaisePropertyChanged(g());
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            this.RaisePropertyChanged((IEnumerable<string>)propertyNames);
        }

        protected virtual void RaisePropertyChanged(IEnumerable<string> propertyNames)
        {
            if (propertyNames == null)
                return;
            var temp = PropertyChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                foreach (var item in propertyNames)
                {
                    temp(this, new PropertyChangedEventArgs(item));
                }
            });
        }
    }
}
