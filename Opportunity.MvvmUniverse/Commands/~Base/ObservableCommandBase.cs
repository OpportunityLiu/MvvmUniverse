﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class of all commands.
    /// </summary>
    public abstract class ObservableCommandBase : ObservableObject, IControllable
    {
        internal ObservableCommandBase() { }

        private bool isEnabled = true;
        /// <summary>
        /// Overall switch of command.
        /// </summary>
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (Set(ref this.isEnabled, value))
                    OnCanExecuteChanged();
            }
        }

        private object tag;
        /// <summary>
        /// A tag associated with this object.
        /// </summary>
        public object Tag
        {
            get => this.tag;
            set => ForceSet(ref this.tag, value);
        }

        internal static void ThrowUnhandledError(Exception error)
        {
            if (error is null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                if (error is AggregateException ae)
                    throw new AggregateException(ae.InnerExceptions);
                else
                    throw new AggregateException(error);
            });
        }

        /// <summary>
        /// Raise when return value of <see cref="System.Windows.Input.ICommand.CanExecute(object)"/> changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Raise <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void OnCanExecuteChanged()
        {
            var temp = this.CanExecuteChanged;
            if (temp is null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }

    }
}
