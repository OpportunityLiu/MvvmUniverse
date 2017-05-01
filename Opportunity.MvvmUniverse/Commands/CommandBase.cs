using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opportunity.MvvmUniverse.Helpers;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase : ObservableObject
    {
        public event EventHandler CanExecuteChanged;

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                Set(ref this.isEnabled, value);
                RaiseCanExecuteChanged();
            }
        }

        private object tag;
        public object Tag
        {
            get => this.tag;
            set => Set(ref this.tag, value);
        }

        public void RaiseCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }
    }
}
