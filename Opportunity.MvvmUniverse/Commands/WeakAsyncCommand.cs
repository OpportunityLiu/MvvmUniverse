using Opportunity.MvvmUniverse.Delegates;
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
        private bool isExecuting = false;

        public bool IsAlive => this.execute.IsAlive && (this.canExecute?.IsAlive == true);

        public bool IsExecuting
        {
            get => this.isExecuting;
            private set
            {
                if (Set(ref this.isExecuting, value))
                    OnCanExecuteChanged();
            }
        }

        protected override bool CanExecuteOverride()
        {
            if (!IsAlive)
                return false;
            if (this.IsExecuting)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }

        protected override async void ExecuteImpl()
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke();
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
