using Opportunity.MvvmUniverse.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Windows.ApplicationModel;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Base class of VMs.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Indicate whether the VM is running in design mode or not.
        /// </summary>
        public bool DesignModeEnabled => DesignMode.DesignModeEnabled;
        /// <summary>
        /// Indicate whether the VM is running in design mode or not.
        /// </summary>
        public static bool DesignModeEnabledStatic => DesignMode.DesignModeEnabled;

        /// <summary>
        /// Create new instance of <see cref="ViewModelBase"/>.
        /// </summary>
        protected ViewModelBase() { }

        /// <summary>
        /// Commands of the vm,
        /// all <see cref="IControllable"/>'s <see cref="IControllable.Tag"/> in this dictionary will be set to the instance.
        /// </summary>
        protected CommandDictionary Commands => LazyInitializer.EnsureInitialized(ref this.commands, () => new CommandDictionary(this));
        private CommandDictionary commands;

        /// <summary>
        /// Get command from <see cref="Commands"/>.
        /// </summary>
        /// <typeparam name="T">Type of command.</typeparam>
        /// <param name="key">Key of command.</param>
        /// <returns>Command with <paramref name="key"/> from <see cref="Commands"/>.</returns>
        protected T GetCommand<T>([CallerMemberName] string key = null)
            where T : System.Windows.Input.ICommand => (T)Commands[key];
    }
}