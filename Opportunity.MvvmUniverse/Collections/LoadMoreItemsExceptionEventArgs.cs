using System;

namespace Opportunity.MvvmUniverse.Collections
{
    public delegate void LoadMoreItemsExceptionEventHadler<T>(IncrementalLoadingList<T> sender, LoadMoreItemsExceptionEventArgs e);

    public class LoadMoreItemsExceptionEventArgs : EventArgs
    {
        internal LoadMoreItemsExceptionEventArgs(Exception ex)
        {
            this.Exception = ex;
        }

        public Exception Exception
        {
            get;
        }

        public string Message => this.Exception?.Message;

        public bool Handled
        {
            get;
            set;
        }
    }
}
