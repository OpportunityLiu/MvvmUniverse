using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Collections
{
    public abstract class ObservableCollectionBase : ObservableObject, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void RaiseCollectionChanged(Func<NotifyCollectionChangedEventArgs> argsGenerator)
        {
            if (argsGenerator == null)
                throw new ArgumentNullException(nameof(argsGenerator));
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, argsGenerator());
            });
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            RaiseCollectionChanged(() => args);
        }

        protected void RaiseCollectionReset()
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void RaiseCollectionMove(object item, int newIndex, int oldIndex)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected void RaiseCollectionAdd(object item, int index)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected void RaiseCollectionRemove(object item, int index)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected void RaiseCollectionReplace(object newItem, object oldItem, int index)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }
    }
}
