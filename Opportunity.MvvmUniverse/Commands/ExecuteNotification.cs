using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void CommandExecutingEventHandler(ICommand sender, CommandExecutingEventArgs e);

    public sealed class CommandExecutingEventArgs
    {
        public CommandExecutingEventArgs(object parameter)
        {
            this.Parameter = parameter;
        }

        public object Parameter { get; private set; }

        public bool Cancelled { get; set; }
    }

    public delegate void CommandExecutedEventHandler(ICommand sender, CommandExecutedEventArgs e);

    public sealed class CommandExecutedEventArgs
    {
        public CommandExecutedEventArgs(object parameter, Exception exception)
        {
            this.Parameter = parameter;
            this.Exception = exception;
        }

        public object Parameter { get; private set; }

        public Exception Exception { get; private set; }
    }
}
