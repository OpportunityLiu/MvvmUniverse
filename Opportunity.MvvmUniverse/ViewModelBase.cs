using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;

namespace Opportunity.MvvmUniverse
{
    public abstract class ViewModelBase : ObservableObject
    {
        public bool DesignModeEnabled => DesignMode.DesignModeEnabled;
        public static bool DesignModeEnabledStatic => DesignMode.DesignModeEnabled;
    }
}