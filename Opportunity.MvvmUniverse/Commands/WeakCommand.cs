using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class WeakCommand : CommandBase, IWeakCommand
    {
        public static WeakCommand Create(Action execute) => new WeakCommand(execute, null);
        public static WeakCommand Create(WeakAction execute) => new WeakCommand(execute, null);
        public static WeakCommand Create(Action execute, Func<bool> canExecute) => new WeakCommand(execute, canExecute);
        public static WeakCommand Create(WeakAction execute, WeakFunc<bool> canExecute) => new WeakCommand(execute, canExecute);
        public static WeakCommand<T> Create<T>(Action<T> execute) => new WeakCommand<T>(execute, null);
        public static WeakCommand<T> Create<T>(WeakAction<T> execute) => new WeakCommand<T>(execute, null);
        public static WeakCommand<T> Create<T>(Action<T> execute, Predicate<T> canExecute) => new WeakCommand<T>(execute, canExecute);
        public static WeakCommand<T> Create<T>(WeakAction<T> execute, WeakPredicate<T> canExecute) => new WeakCommand<T>(execute, canExecute);

        internal WeakCommand(WeakAction execute, WeakFunc<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        internal WeakCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = new WeakAction(execute ?? throw new ArgumentNullException(nameof(execute)));
            if (canExecute != null)
                this.canExecute = new WeakFunc<bool>(canExecute);
        }

        private readonly WeakAction execute;
        private readonly WeakFunc<bool> canExecute;

        public bool IsAlive => this.execute.IsAlive && (this.canExecute?.IsAlive == true);

        protected override bool CanExecuteOverride()
        {
            if (!IsAlive)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }

        protected override void StartExecution()
        {
            try
            {
                this.execute.Invoke();
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
