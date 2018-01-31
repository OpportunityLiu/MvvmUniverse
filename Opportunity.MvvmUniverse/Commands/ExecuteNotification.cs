using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void CommandExecutingEventHandler(ICommand sender, CommandExecutingEventArgs e);
    public delegate void CommandExecutingEventHandler<T>(ICommand<T> sender, CommandExecutingEventArgs<T> e);

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

    public delegate void CommandExecutedEventHandler(ICommand sender, CommandExecutedEventArgs e);
    public delegate void CommandExecutedEventHandler<T>(ICommand<T> sender, CommandExecutedEventArgs<T> e);

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

    public delegate void CommandExecutingProgressEventHandler<TProgress>(IProgressedCommand<TProgress> sender, CommandExecutingProgressEventArgs<TProgress> e);

    public class CommandExecutingProgressEventArgs<TProgress> : EventArgs
    {
        public CommandExecutingProgressEventArgs(TProgress progress)
        {
            this.Progress = progress;
        }

        public TProgress Progress { get; }
    }
}
