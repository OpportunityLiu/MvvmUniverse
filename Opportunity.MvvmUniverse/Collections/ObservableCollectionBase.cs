using System;
using System.Collections;
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

        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, args);
            });
        }

        protected void RaiseCollectionReset()
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void RaiseCollectionMove(object item, int newIndex, int oldIndex)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected void RaiseCollectionMove(IList items, int newIndex, int oldIndex)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex));
        }

        protected void RaiseCollectionAdd(object item, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected void RaiseCollectionAdd(IList items, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
        }

        protected void RaiseCollectionRemove(object item, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected void RaiseCollectionRemove(IList items, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index));
        }

        protected void RaiseCollectionReplace(object newItem, object oldItem, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected void RaiseCollectionReplace(IList newItems, IList oldItems, int index)
        {
            if (CollectionChanged == null)
                return;
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, index));
        }
    }
}
