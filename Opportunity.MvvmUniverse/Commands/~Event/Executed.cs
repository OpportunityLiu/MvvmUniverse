using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void ExecutedEventHandler(ICommand sender, ExecutedEventArgs e);
    public delegate void ExecutedEventHandler<T>(ICommand<T> sender, ExecutedEventArgs<T> e);

    public class ExecutedEventArgs : EventArgs
    {
        public static ExecutedEventArgs Succeed { get; } = new ExecutedEventArgs(null);

        public ExecutedEventArgs() { }

        public ExecutedEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    public class ExecutedEventArgs<T> : ExecutedEventArgs
    {
        public ExecutedEventArgs(T parameter) : base()
        {
            this.Parameter = parameter;
        }

        public ExecutedEventArgs(T parameter, Exception exception)
            : base(exception)
        {
            this.Parameter = parameter;
        }

        public T Parameter { get; }
    }
}
