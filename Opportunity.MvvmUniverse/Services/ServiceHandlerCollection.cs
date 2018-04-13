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
    internal sealed class ServiceHandlerCollection<TService, THandler> : ObservableList<THandler>
        where TService : class, IService<THandler>
        where THandler : IServiceHandler<TService>
    {
        public static readonly Dictionary<THandler, TService> HandlerDic
            = new Dictionary<THandler, TService>();

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

        private static LockHelper GetLock() => new LockHelper(ViewDependentSingleton<TService>.Count > 1);

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

        public void Destory()
        {
            if (this.Service != null)
            {
                Clear();
                this.Service = null;
            }
        }
    }
}
