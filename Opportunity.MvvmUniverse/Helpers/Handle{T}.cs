using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace Opportunity.MvvmUniverse.Helpers
{
    public class Handle<T> : ObservableObject
    {
        private DataLoader<T> loader;

        public Handle(DataLoader<T> loader)
        {
            this.loader = this.loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public void Reset()
        {
            this.Data = default(T);
            this.Status = HandleStatus.Reset;
        }

        public void StartLoading()
        {
            this.Status = HandleStatus.Loading;
            DispatcherHelper.BeginInvoke(() => this.loader(new DataLoaderOperation<T>(this)));
        }

        private object data;
        public T Data
        {
            get
            {
                switch (this.status)
                {
                case HandleStatus.Created:
                case HandleStatus.Reset:
                    StartLoading();
                    return default(T);
                case HandleStatus.Loading:
                default:
                    return default(T);
                case HandleStatus.Loaded:
                    return (T)this.data;
                case HandleStatus.Failed:
                    throw new InvalidOperationException("Load failed", (Exception)this.data);
                }
            }
            private set => ForceSet(ref this.data, value);
        }

        private HandleStatus status;
        public HandleStatus Status
        {
            get => status;
            private set => ForceSet(ref this.status, value);
        }

        public event TypedEventHandler<Handle<T>, EventArgs> LoadFinished;
        public event TypedEventHandler<Handle<T>, DataLoadFailedEventArgs> LoadFailed;

        internal void Finished(T result)
        {
            this.Status = HandleStatus.Loaded;
            var temp = LoadFinished;
            if (temp != null)
                DispatcherHelper.BeginInvoke(() => temp.Invoke(this, EventArgs.Empty));
        }

        internal void Failed(Exception error)
        {
            this.Status = HandleStatus.Failed;
            this.data = error;
            var temp = LoadFailed;
            if (temp != null)
                DispatcherHelper.BeginInvoke(() => temp.Invoke(this, new DataLoadFailedEventArgs { Error = error }));
        }
    }

    public enum HandleStatus
    {
        Created = 0,
        Loading,
        Loaded,
        Failed,
        Reset
    }

    public sealed class DataLoadFailedEventArgs : EventArgs
    {
        public Exception Error { get; internal set; }
    }

    public delegate void DataLoader<T>(DataLoaderOperation<T> operation);

    public class DataLoaderOperation<T>
    {
        internal DataLoaderOperation(Handle<T> handle)
        {
            this.Handle = handle;
        }

        public Handle<T> Handle { get; }

        public void ReportFailed(Exception error)
        {
            this.Handle.Failed(error);
        }

        public void ReportFailed()
        {
            ReportFailed(null);
        }

        public void ReportFinished(T result)
        {
            this.Handle.Finished(result);
        }
    }
}
