using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void Executor(Command command);
    public delegate bool Predicate(Command command);

    public class Command : CommandBase
    {
        #region Factory methods
        public static Command Create(Executor execute) => new Command(execute, null);
        public static Command Create(Executor execute, Predicate canExecute) => new Command(execute, canExecute);
        public static Command<T> Create<T>(Executor<T> execute) => new Command<T>(execute, null);
        public static Command<T> Create<T>(Executor<T> execute, Predicate<T> canExecute) => new Command<T>(execute, canExecute);
        #endregion Factory methods

        protected internal Command(Executor execute, Predicate canExecute)
        {
            this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
            this.CanExecuteDelegate = canExecute;
        }

        protected Executor ExecuteDelegate { get; }
        protected Predicate CanExecuteDelegate { get; }

        /// <summary>
        /// Check with <see cref="CanExecuteDelegate"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride()
        {
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this);
        }

        /// <summary>
        /// Returns <see cref="Task.CompletedTask"/> or <see cref="Task.FromException(Exception)"/>.
        /// </summary>
        /// <returns>A completed <see cref="Task"/></returns>
        protected override Task StartExecutionAsync()
        {
            try
            {
                this.ExecuteDelegate.Invoke(this);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
