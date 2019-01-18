using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Opportunity.MvvmUniverse.Collections
{
    public delegate void LoadingListFailedEventHandler(object sender, LoadingListFailedEventArgs args);

    public class LoadingListFailedEventArgs
    {
        internal LoadingListFailedEventArgs(Exception error) => this.Error = error;

        public Exception Error { get; }

        public bool Handled { get; set; }
    }

    public delegate void LoadingListLoadedEventHandler(object sender, LoadingListLoadedEventArgs args);

    public class LoadingListLoadedEventArgs
    {
        internal LoadingListLoadedEventArgs() { }

        internal static readonly LoadingListLoadedEventArgs Instance = new LoadingListLoadedEventArgs();
    }

    /// <summary>
    /// Base class for collections with loading features.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public abstract class LoadingListBase<T> : ObservableList<T>
    {

        /// <summary>
        /// Create instance of <see cref="LoadingListBase{T}"/>.
        /// </summary>
        public LoadingListBase()
        {
        }

        /// <summary>
        /// Create instance of <see cref="LoadingListBase{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied to the <see cref="LoadingListBase{T}"/>.</param>
        public LoadingListBase(IEnumerable<T> items) : base(items)
        {
        }

        /// <summary>
        /// Create instance of <see cref="LoadingListBase{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied or wrapped to the <see cref="LoadingListBase{T}"/>.</param>
        /// <param name="makeCopy">Indicates the <paramref name="items"/> should be copied or directly set to <see cref="ObservableList{T}.Items"/>.</param>
        protected LoadingListBase(IList<T> items, bool makeCopy) : base(items, makeCopy)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int isLoading;
        /// <summary>
        /// Indicates that the loading procedure is running.
        /// </summary>
        public bool IsLoading => this.isLoading != 0;

        private readonly DepedencyEvent<LoadingListFailedEventHandler, LoadingListBase<T>, LoadingListFailedEventArgs> failed
            = new DepedencyEvent<LoadingListFailedEventHandler, LoadingListBase<T>, LoadingListFailedEventArgs>((h, s, e) => h(s, e));
        /// <summary>
        /// Raises when the loading procedure failed.
        /// </summary>
        public event LoadingListFailedEventHandler LoadingFailed
        {
            add => this.failed.Add(value);
            remove => this.failed.Remove(value);
        }

        /// <summary>
        /// Raises the <see cref="LoadingFailed"/> event.
        /// </summary>
        /// <param name="args">Event args.</param>
        protected virtual async void OnLoadingFailed(LoadingListFailedEventArgs args)
        {
            await this.failed.RaiseAsync(this, args);
            if (!args.Handled)
                DispatcherHelper.ThrowUnhandledError(args.Error);
        }

        private readonly DepedencyEvent<LoadingListLoadedEventHandler, LoadingListBase<T>, LoadingListLoadedEventArgs> loaded
            = new DepedencyEvent<LoadingListLoadedEventHandler, LoadingListBase<T>, LoadingListLoadedEventArgs>((h, s, e) => h(s, e));

        /// <summary>
        /// Raises when the loading procedure finished successfully.
        /// </summary>
        public event LoadingListLoadedEventHandler Loaded
        {
            add => this.loaded.Add(value);
            remove => this.loaded.Remove(value);
        }

        /// <summary>
        /// Raises the <see cref="Loaded"/> event.
        /// </summary>
        /// <param name="args">Event args.</param>
        protected virtual void OnLoaded(LoadingListLoadedEventArgs args)
        {
            var ignore = this.loaded.RaiseAsync(this, args);
        }

        /// <summary>
        /// Begin action provided by <paramref name="loadingAction"/>, handles status of <see cref="IsLoading"/>, raises <see cref="LoadingFailed"/> or <see cref="Loaded"/>.
        /// </summary>
        /// <param name="loadingAction">The factory function to generate the loading action.</param>
        /// <exception cref="ArgumentNullException"><paramref name="loadingAction"/> is <see langword="null"/>.</exception>
        protected IAsyncOperation<TResult> BeginLoading<TResult>(Func<IAsyncOperation<TResult>> loadingAction)
        {
            if (loadingAction is null)
                throw new ArgumentNullException(nameof(loadingAction));

            if (Interlocked.CompareExchange(ref this.isLoading, 1, 0) == 0)
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.IsLoading);
                return Run(async token =>
                {
                    var result = default(TResult);
                    var ex = default(Exception);
                    try
                    {
                        result = await loadingAction().AsTask(token);
                    }
                    catch (Exception exc)
                    {
                        ex = exc;
                    }
                    finally
                    {
                        Volatile.Write(ref this.isLoading, 0);
                        OnPropertyChanged(ConstPropertyChangedEventArgs.IsLoading);
                    }
                    if (ex is null)
                        OnLoaded(LoadingListLoadedEventArgs.Instance);
                    else
                        OnLoadingFailed(new LoadingListFailedEventArgs(ex));
                    return result;
                });
            }
            else
            {
                return Run(async token =>
                {
                    while (!token.IsCancellationRequested && this.isLoading != 0)
                    {
                        await Task.Delay(500, token);
                    }
                    token.ThrowIfCancellationRequested();
                    return await BeginLoading(loadingAction).AsTask(token);
                });
            }
        }

        /// <summary>
        /// Begin action provided by <paramref name="loadingAction"/>, handles status of <see cref="IsLoading"/>, raises <see cref="LoadingFailed"/> or <see cref="Loaded"/>.
        /// </summary>
        /// <param name="loadingAction">The factory function to generate the loading action.</param>
        /// <exception cref="ArgumentNullException"><paramref name="loadingAction"/> is <see langword="null"/>.</exception>
        protected IAsyncAction BeginLoading(Func<IAsyncAction> loadingAction)
        {
            if (loadingAction is null)
                throw new ArgumentNullException(nameof(loadingAction));

            if (Interlocked.CompareExchange(ref this.isLoading, 1, 0) == 0)
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.IsLoading);
                return Run(async token =>
                {
                    var ex = default(Exception);
                    try
                    {
                        await loadingAction().AsTask(token);
                    }
                    catch (Exception exc)
                    {
                        ex = exc;
                    }
                    finally
                    {
                        Volatile.Write(ref this.isLoading, 0);
                        OnPropertyChanged(ConstPropertyChangedEventArgs.IsLoading);
                    }
                    if (ex is null)
                        OnLoaded(LoadingListLoadedEventArgs.Instance);
                    else
                        OnLoadingFailed(new LoadingListFailedEventArgs(ex));
                });
            }
            else
            {
                return Run(async token =>
                {
                    while (!token.IsCancellationRequested && this.isLoading != 0)
                    {
                        await Task.Delay(500, token);
                    }
                    token.ThrowIfCancellationRequested();
                    await BeginLoading(loadingAction).AsTask(token);
                });
            }
        }
    }
}
