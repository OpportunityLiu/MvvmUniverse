using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class for commands implements <see cref="IAsyncCommand"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand, ICommand<T>
    {
        /// <summary>
        /// Check with <see cref="IsExecuting"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride(T parameter) => !this.isExecuting;

        private bool isExecuting = false;
        /// <summary>
        /// Indicates whether the command is executing. 
        /// </summary>
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
        /// Call <see cref="CommandBase{T}.OnStarting(T)"/>.
        /// If not be canceled, <see cref="IsExecuting"/> will be set to <see langword="true"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution</param>
        /// <returns>True if executing not canceled</returns>
        protected override bool OnStarting(T parameter)
        {
            var r = base.OnStarting(parameter);
            if (r)
                IsExecuting = true;
            return r;
        }

        /// <summary>
        /// Call <see cref="CommandBase{T}.OnFinished(IAsyncAction, T)"/> and set <see cref="IsExecuting"/> to <see langword="false"/>.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="CommandBase{T}.Execute(T)"/>.</param>
        /// <param name="execution">Result of <see cref="CommandBase{T}.StartExecutionAsync(T)"/>.</param>
        protected override void OnFinished(IAsyncAction execution, T parameter)
        {
            IsExecuting = false;
            base.OnFinished(execution, parameter);
        }
    }
}
