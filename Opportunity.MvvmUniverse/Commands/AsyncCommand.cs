using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public class AsyncCommand : CommandBase
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
        private bool executing = false;

        public bool Executing
        {
            get => this.executing;
            protected set
            {
                if (Set(ref this.executing, value))
                    RaiseCanExecuteChanged();
            }
        }

        protected override bool CanExecuteOverride()
        {
            if (this.Executing)
                return false;
            if (this.canExecute == null)
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
