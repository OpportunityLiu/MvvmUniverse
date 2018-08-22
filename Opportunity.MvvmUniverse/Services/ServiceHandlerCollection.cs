using Opportunity.Helpers.ObjectModel;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Services
{
    /// <summary>
    /// Collection of <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <typeparam name="THandler">Handler type.</typeparam>
    public sealed class ServiceHandlerCollection<TService, THandler> : ObservableList<THandler>, IDisposable
        where TService : class, IService<TService, THandler>
        where THandler : IServiceHandler<TService>
    {
        private static readonly Dictionary<THandler, TService> HandlerDic
            = new Dictionary<THandler, TService>();

        /// <summary>
        /// Get the service associated with the <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">Handler to find associated service.</param>
        /// <returns>The service associated with the <paramref name="handler"/>, or <see langword="null"/>, if the <paramref name="handler"/> isn't associated with any services.</returns>
        public static TService GetService(THandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            if (HandlerDic.TryGetValue(handler, out var service))
                return service;
            return default;
        }

        private struct LockHelper : IDisposable
        {
            private bool locked;

            public LockHelper(bool needLock)
            {
                this = new LockHelper();
                if (needLock)
                    Monitor.Enter(HandlerDic, ref this.locked);
            }

            public void Dispose()
            {
                if (this.locked)
                    Monitor.Exit(HandlerDic);
                this.locked = false;
            }
        }

        private static LockHelper GetLock() => new LockHelper(ThreadLocalSingleton.Count<TService>() > 1);

        public TService Service { get; private set; }

        internal ServiceHandlerCollection(TService service)
        {
            this.Service = service;
        }

        private void CheckAvailable()
        {
            if (this.Service is null)
                throw new InvalidOperationException("The service of this collection has been destoryed.");
        }

        protected override void ClearItems()
        {
            CheckAvailable();
            try
            {
                using (GetLock())
                {
                    while (this.Count != 0)
                    {
                        var item = this[this.Count - 1];
                        item.OnRemove(this.Service);
                        HandlerDic.Remove(item);
                        base.RemoveItem(this.Count - 1);
                    }
                }
            }
            finally
            {
                Service.UpdateProperties();
            }
        }

        protected override void InsertItem(int index, THandler item)
        {
            CheckAvailable();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            item.OnAdd(this.Service);
            using (GetLock())
            {
                HandlerDic.Add(item, this.Service);
            }
            base.InsertItem(index, item);
            Service.UpdateProperties();
        }

        protected override void SetItem(int index, THandler item)
        {
            CheckAvailable();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            var old = this[index];
            item.OnAdd(this.Service);
            try
            {
                old.OnRemove(this.Service);
            }
            catch
            {
                item.OnRemove(this.Service);
                throw;
            }
            using (GetLock())
            {
                HandlerDic.Remove(old);
                HandlerDic.Add(item, this.Service);
            }
            base.SetItem(index, item);
            Service.UpdateProperties();
        }

        protected override void RemoveItem(int index)
        {
            CheckAvailable();
            var old = this[index];
            old.OnRemove(this.Service);
            using (GetLock())
            {
                HandlerDic.Remove(old);
            }
            base.RemoveItem(index);
            Service.UpdateProperties();
        }

        /// <summary>
        /// Clear handlers and set <see cref="Service"/> to <see langword="null"/>.
        /// </summary>
        public void Dispose()
        {
            if (this.Service != null)
            {
                Clear();
                this.Service = null;
            }
        }
    }
}
