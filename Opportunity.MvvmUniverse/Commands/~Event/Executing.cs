using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void ExecutingEventHandler(ICommand sender, ExecutingEventArgs e);
    public delegate void ExecutingEventHandler<T>(ICommand<T> sender, ExecutingEventArgs<T> e);

    public class ExecutingEventArgs : EventArgs
    {
        public ExecutingEventArgs()
        {
        }

        public bool Cancelled { get; set; }
    }

    public class ExecutingEventArgs<T> : ExecutingEventArgs
    {
        public ExecutingEventArgs(T parameter)
        {
            this.Parameter = parameter;
        }

        public T Parameter { get; }
    }
}
