﻿using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class AsyncActionCommand<T> : AsyncCommand<T>
    {
        public AsyncActionCommand(Func<T, IAsyncAction> execute, Predicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly Func<T, IAsyncAction> execute;

        protected override async void StartExecution(T parameter)
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke(parameter);
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
