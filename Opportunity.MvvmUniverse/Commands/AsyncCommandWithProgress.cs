using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public static class AsyncCommandWithProgress
    {
        public static AsyncCommandWithProgress<TProgress> Create<TProgress>(AsyncActionCommandWithProgressExecutor<TProgress> execute, ProgressMapper<TProgress> progressMapper)
            => new AsyncActionCommandWithProgress<TProgress>(execute, progressMapper, null);
        public static AsyncCommandWithProgress<TProgress> Create<TProgress>(AsyncActionCommandWithProgressExecutor<TProgress> execute, ProgressMapper<TProgress> progressMapper, AsyncCommandPredicate canExecute)
            => new AsyncActionCommandWithProgress<TProgress>(execute, progressMapper, canExecute);

        public static AsyncCommandWithProgress<T, TProgress> Create<T, TProgress>(AsyncActionCommandWithProgressExecutor<T, TProgress> execute, ProgressMapper<TProgress> progressMapper)
            => new AsyncActionCommandWithProgress<T, TProgress>(execute, progressMapper, null);
        public static AsyncCommandWithProgress<T, TProgress> Create<T, TProgress>(AsyncActionCommandWithProgressExecutor<T, TProgress> execute, ProgressMapper<TProgress> progressMapper, AsyncCommandPredicate<T> canExecute)
            => new AsyncActionCommandWithProgress<T, TProgress>(execute, progressMapper, canExecute);
    }
}
