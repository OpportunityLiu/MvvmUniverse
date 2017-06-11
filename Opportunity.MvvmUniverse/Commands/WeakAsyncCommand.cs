﻿using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class WeakAsyncCommand : CommandBase
    {
        public WeakAsyncCommand(WeakAsyncAction execute, WeakFunc<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public WeakAsyncCommand(WeakAsyncAction execute) : this(execute, null)
        {
        }

        private readonly WeakAsyncAction execute;
        private readonly WeakFunc<bool> canExecute;
        private bool executing = false;

        public bool IsAlive => this.execute.IsAlive && (this.canExecute?.IsAlive == true);

        public bool Executing
        {
            get => this.executing;
            private set
            {
                if (Set(ref this.executing, value))
                    RaiseCanExecuteChanged();
            }
        }

        protected override bool CanExecuteOverride()
        {
            if (!IsAlive)
                return false;
            if (this.Executing)
                return false;
            if(this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }

        protected override async void ExecuteImpl()
        {
            this.Executing = true;
            try
            {
                await this.execute.Invoke();
            }
            finally
            {
                this.Executing = false;
            }
        }
    }
}
