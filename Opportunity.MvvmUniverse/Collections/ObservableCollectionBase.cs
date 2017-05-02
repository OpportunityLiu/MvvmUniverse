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

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
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
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        protected void RaiseCollectionMove(object item, int newIndex, int oldIndex)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
            });
        }

        protected void RaiseCollectionAdd(object item, int index)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            });
        }

        protected void RaiseCollectionRemove(object item, int index)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            });
        }

        protected void RaiseCollectionReplace(object newItem, object oldItem, int index)
        {
            var temp = CollectionChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
            });
        }
    }
}
