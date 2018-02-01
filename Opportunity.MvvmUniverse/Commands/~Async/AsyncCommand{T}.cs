using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate bool AsyncPredicate<T>(AsyncCommand<T> command, T parameter);

    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand
    {
        protected AsyncCommand(AsyncPredicate<T> canExecute)
        {
            this.CanExecuteDelegate = canExecute;
        }

        protected AsyncPredicate<T> CanExecuteDelegate { get; }

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

        /// <summary>
        /// Check with <see cref="IsExecuting"/> and <see cref="CanExecuteDelegate"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.IsExecuting)
                return false;
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this, parameter);
        }

        /// <summary>
        /// Raise <see cref="Executing"/> event.
        /// If not be cancelled, <see cref="IsExecuting"/> will be set to <c>true</c>.
        /// </summary>
        /// <param name="parameter">Parameter of execution</param>
        /// <returns>True if executing not cancelled</returns>
        protected override bool OnStarting(T parameter)
        {
            var r = base.OnStarting(parameter);
            if (r)
                IsExecuting = true;
            return r;
        }

        /// <summary>
        /// Raise <see cref="Executed"/> event and set <see cref="IsExecuting"/> to <c>false</c>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnFinished(ExecutedEventArgs<T> e)
        {
            IsExecuting = false;
            base.OnFinished(e);
        }
    }
}
