﻿using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class Command<T> : CommandBase, ICommand
    {
        public Command(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            this.execute = new WeakAction<T>(execute);
            if (canExecute != null)
                this.canExecute = new WeakPredicate<T>(canExecute);
        }

        public Command(Action<T> execute) : this(execute, null)
        {
        }

        private readonly WeakAction<T> execute;
        private readonly WeakPredicate<T> canExecute;
        
        bool ICommand.CanExecute(object parameter) => CanExecute(Cast(parameter));

        public bool CanExecute(T parameter)
        {
            if (!this.IsEnabled)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }

        void ICommand.Execute(object parameter) => Execute(Cast(parameter));

        public bool Execute(T parameter)
        {
            if (CanExecute(parameter))
            {
                this.execute.Invoke(parameter);
                return true;
            }
            return false;
        }

        private T Cast(object obj)
        {
            if (obj is T p)
                return p;
            else
                return default(T);
        }
    }
}
