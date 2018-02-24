using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public static class AsyncCommandWithProgress
    {
        public static AsyncCommandWithProgress<TProgress> Create<TProgress>(AsyncActionWithProgressExecutor<TProgress> execute, ProgressMapper<TProgress> progressMapper)
            => new AsyncActionCommandWithProgress<TProgress>(execute, progressMapper, null);
        public static AsyncCommandWithProgress<TProgress> Create<TProgress>(AsyncActionWithProgressExecutor<TProgress> execute, ProgressMapper<TProgress> progressMapper, AsyncPredicate canExecute)
            => new AsyncActionCommandWithProgress<TProgress>(execute, progressMapper, canExecute);

        public static AsyncCommandWithProgress<T, TProgress> Create<T, TProgress>(AsyncActionWithProgressExecutor<T, TProgress> execute, ProgressMapper<T, TProgress> progressMapper)
            => new AsyncActionCommandWithProgress<T, TProgress>(execute, progressMapper, null);
        public static AsyncCommandWithProgress<T, TProgress> Create<T, TProgress>(AsyncActionWithProgressExecutor<T, TProgress> execute, ProgressMapper<T, TProgress> progressMapper, AsyncPredicate<T> canExecute)
            => new AsyncActionCommandWithProgress<T, TProgress>(execute, progressMapper, canExecute);
    }
}
