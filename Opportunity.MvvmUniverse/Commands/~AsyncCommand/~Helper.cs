using Opportunity.MvvmUniverse.Commands.ReentrancyHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    internal static class AsyncCommandHelper
    {
        public static bool CanExecuteOverride<T>(bool isExecuting, IReentrancyHandler<T> mode)
        {
            if (mode.AllowReenter)
                return true;

            return !isExecuting;
        }

        public static void SetReentrancyHandler<TCommand, T>(this TCommand command, ref IReentrancyHandler<T> field, IReentrancyHandler<T> value)
            where TCommand : ObservableObject, IAsyncCommand
        {
            value = value ?? ReentrancyHandler.Disallowed<T>();
            if (field.Equals(value))
                return;
            field.Detach();
            try
            {
                value.Attach(command);
                field = value;
                command.OnPropertyChanged(nameof(AsyncCommand.ReentrancyHandler));
            }
            catch
            {
                field.Attach(command);
                throw;
            }
        }
    }
}
