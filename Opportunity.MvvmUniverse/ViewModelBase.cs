using System;
using System.Collections;
using Windows.ApplicationModel;

namespace Opportunity.MvvmUniverse
{
    public abstract class ViewModelBase : ObservableObject
    {
        public bool DesignModeEnabled => DesignMode.DesignModeEnabled;
        public static bool DesignModeEnabledStatic => DesignMode.DesignModeEnabled;
    }
}