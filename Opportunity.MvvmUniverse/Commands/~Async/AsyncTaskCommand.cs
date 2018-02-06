﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate Task AsyncTaskExecutor(AsyncCommand command);

    internal sealed class AsyncTaskCommand : AsyncCommand
    {
        internal AsyncTaskCommand(AsyncTaskExecutor execute, AsyncPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncTaskExecutor execute;

        protected override Task StartExecutionAsync()
        {
            return this.execute.Invoke(this);
        }
    }
}
