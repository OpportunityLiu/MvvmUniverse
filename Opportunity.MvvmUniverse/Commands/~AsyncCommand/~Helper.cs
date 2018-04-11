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
    }
}
