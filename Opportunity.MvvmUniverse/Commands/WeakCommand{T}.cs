﻿using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class WeakCommand<T> : CommandBase<T>
    {
        internal WeakCommand(WeakAction<T> execute, WeakPredicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        internal WeakCommand(Action<T> execute, Predicate<T> canExecute)
        {
            this.execute = new WeakAction<T>(execute ?? throw new ArgumentNullException(nameof(execute)));
            if (canExecute != null)
                this.canExecute = new WeakPredicate<T>(canExecute);
        }

        private readonly WeakAction<T> execute;
        private readonly WeakPredicate<T> canExecute;

        public bool IsAlive => this.execute.IsAlive && (this.canExecute?.IsAlive == true);

        protected override bool CanExecuteOverride(T parameter)
        {
            if (!IsAlive)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }

        protected override void StartExecution(T parameter)
        {
            try
            {
                this.execute.Invoke(parameter);
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
        }
    }
}
