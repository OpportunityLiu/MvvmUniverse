using System;

namespace Opportunity.MvvmUniverse.Collections
{
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
