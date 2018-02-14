using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;

namespace Opportunity.MvvmUniverse
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
    }
}