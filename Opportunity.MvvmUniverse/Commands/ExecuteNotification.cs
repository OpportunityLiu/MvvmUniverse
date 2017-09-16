using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void CommandExecutingEventHandler(CommandBase sender, CommandExecutingEventArgs e);
    public delegate void CommandExecutingEventHandler<T>(CommandBase<T> sender, CommandExecutingEventArgs<T> e);

    public class CommandExecutingEventArgs : EventArgs
    {
        public CommandExecutingEventArgs()
        {
        }

        public bool Cancelled { get; set; }
    }

    public class CommandExecutingEventArgs<T> : CommandExecutingEventArgs
    {
        public CommandExecutingEventArgs(T parameter)
        {
            this.Parameter = parameter;
        }

        public T Parameter { get; private set; }
    }

    public delegate void CommandExecutedEventHandler(CommandBase sender, CommandExecutedEventArgs e);
    public delegate void CommandExecutedEventHandler<T>(CommandBase<T> sender, CommandExecutedEventArgs<T> e);

    public class CommandExecutedEventArgs : EventArgs
    {
        internal static CommandExecutedEventArgs Succeed { get; } = new CommandExecutedEventArgs(null);

        public CommandExecutedEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    public class CommandExecutedEventArgs<T> : CommandExecutedEventArgs
    {
        public CommandExecutedEventArgs(T parameter, Exception exception)
            : base(exception)
        {
            this.Parameter = parameter;
        }

        public T Parameter { get; private set; }
    }
}
