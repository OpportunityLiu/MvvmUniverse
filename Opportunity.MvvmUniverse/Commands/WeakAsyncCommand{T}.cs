﻿using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class WeakAsyncCommand<T> : CommandBase<T>, IAsyncCommand, IWeakCommand
    {
        internal WeakAsyncCommand(Predicate<T> canExecute)
        {
            this.canExecute = new WeakPredicate<T>(canExecute);
        }

        internal WeakAsyncCommand(WeakPredicate<T> canExecute)
        {
            this.canExecute = canExecute;
        }

        private readonly WeakPredicate<T> canExecute;
        protected WeakPredicate<T> CanExecuteDelegate => this.canExecute;

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

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }
    }
}
