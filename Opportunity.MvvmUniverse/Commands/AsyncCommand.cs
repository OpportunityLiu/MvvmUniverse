using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class AsyncCommand : CommandBase
    {
        public AsyncCommand(AsyncAction execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public AsyncCommand(AsyncAction execute) : this(execute, null)
        {
        }

        private readonly AsyncAction execute;
        private readonly Func<bool> canExecute;
        private bool isExecuting = false;

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
