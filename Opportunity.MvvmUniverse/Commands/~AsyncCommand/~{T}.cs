﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="AsyncCommand{T}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="parameter">Current parameter of execution.</param>
    public delegate bool AsyncPredicate<T>(AsyncCommand<T> command, T parameter);

    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand
    {
        protected AsyncCommand(AsyncPredicate<T> canExecute)
        {
            this.CanExecuteDelegate = canExecute;
        }

        protected AsyncPredicate<T> CanExecuteDelegate { get; }

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
        /// Call <see cref="CommandBase{T}.OnFinished(Task, T)"/> and set <see cref="IsExecuting"/> to <see langword="false"/>.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="CommandBase{T}.Execute(T)"/></param>
        /// <param name="execution">result of <see cref="CommandBase{T}.StartExecutionAsync(T)"/></param>
        protected override void OnFinished(Task execution, T parameter)
        {
            IsExecuting = false;
            base.OnFinished(execution, parameter);
        }
    }
}
