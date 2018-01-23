using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class WeakAsyncCommand : CommandBase, IAsyncCommand, IWeakCommand
    {
        public static WeakAsyncCommand Create(AsyncAction execute)
            => new WeakAsyncTaskCommand(execute, null);
        public static WeakAsyncCommand Create(WeakAsyncAction execute)
            => new WeakAsyncTaskCommand(execute, null);
        public static WeakAsyncCommand Create(AsyncAction execute, Func<bool> canExecute)
            => new WeakAsyncTaskCommand(execute, canExecute);
        public static WeakAsyncCommand Create(WeakAsyncAction execute, WeakFunc<bool> canExecute)
            => new WeakAsyncTaskCommand(execute, canExecute);

        public static WeakAsyncCommand Create(Func<IAsyncAction> execute)
            => new WeakAsyncActionCommand(execute, null);
        public static WeakAsyncCommand Create(WeakFunc<IAsyncAction> execute)
            => new WeakAsyncActionCommand(execute, null);
        public static WeakAsyncCommand Create(Func<IAsyncAction> execute, Func<bool> canExecute)
            => new WeakAsyncActionCommand(execute, canExecute);
        public static WeakAsyncCommand Create(WeakFunc<IAsyncAction> execute, WeakFunc<bool> canExecute)
            => new WeakAsyncActionCommand(execute, canExecute);

        public static WeakAsyncCommand<T> Create<T>(AsyncAction<T> execute)
            => new WeakAsyncTaskCommand<T>(execute, null);
        public static WeakAsyncCommand<T> Create<T>(WeakAsyncAction<T> execute)
            => new WeakAsyncTaskCommand<T>(execute, null);
        public static WeakAsyncCommand<T> Create<T>(AsyncAction<T> execute, Predicate<T> canExecute)
            => new WeakAsyncTaskCommand<T>(execute, canExecute);
        public static WeakAsyncCommand<T> Create<T>(WeakAsyncAction<T> execute, WeakPredicate<T> canExecute)
            => new WeakAsyncTaskCommand<T>(execute, canExecute);

        public static WeakAsyncCommand<T> Create<T>(Func<T, IAsyncAction> execute)
            => new WeakAsyncActionCommand<T>(execute, null);
        public static WeakAsyncCommand<T> Create<T>(WeakFunc<T, IAsyncAction> execute)
            => new WeakAsyncActionCommand<T>(execute, null);
        public static WeakAsyncCommand<T> Create<T>(Func<T, IAsyncAction> execute, Predicate<T> canExecute)
            => new WeakAsyncActionCommand<T>(execute, canExecute);
        public static WeakAsyncCommand<T> Create<T>(WeakFunc<T, IAsyncAction> execute, WeakPredicate<T> canExecute)
            => new WeakAsyncActionCommand<T>(execute, canExecute);

        internal WeakAsyncCommand(Func<bool> canExecute)
        {
            this.canExecute = new WeakFunc<bool>(canExecute);
        }

        internal WeakAsyncCommand(WeakFunc<bool> canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly WeakFunc<bool> canExecute;
        protected WeakFunc<bool> CanExecuteDelegate => this.canExecute;

        private bool isExecuting = false;
        public bool IsExecuting
        {
            get => this.isExecuting;
            protected set
            {
                if (Set(ref this.isExecuting, value))
                    OnCanExecuteChanged();
            }
        }

        public abstract bool IsAlive { get; }

        protected override bool CanExecuteOverride()
        {
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }
    }
}
