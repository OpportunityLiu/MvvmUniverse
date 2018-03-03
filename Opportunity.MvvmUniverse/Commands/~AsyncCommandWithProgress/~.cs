using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Factory methods for <see cref="AsyncCommandWithProgress{TProgress}"/> and <see cref="AsyncCommandWithProgress{T, TProgress}"/>.
    /// </summary>
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

        public static AsyncCommandWithProgress<HttpProgress> Create(AsyncActionWithProgressExecutor<HttpProgress> execute)
            => Create(execute, ProgressMappers.HttpProgressMapper);
        public static AsyncCommandWithProgress<HttpProgress> Create(AsyncActionWithProgressExecutor<HttpProgress> execute, AsyncPredicate canExecute)
            => Create(execute, ProgressMappers.HttpProgressMapper, canExecute);

        public static AsyncCommandWithProgress<T, HttpProgress> Create<T>(AsyncActionWithProgressExecutor<T, HttpProgress> execute)
            => Create(execute, ProgressMappers.HttpProgressMapper);
        public static AsyncCommandWithProgress<T, HttpProgress> Create<T>(AsyncActionWithProgressExecutor<T, HttpProgress> execute, AsyncPredicate<T> canExecute)
            => Create(execute, ProgressMappers.HttpProgressMapper, canExecute);
    }
}
