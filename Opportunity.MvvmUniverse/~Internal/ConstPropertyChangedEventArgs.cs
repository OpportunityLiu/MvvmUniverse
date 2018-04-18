using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands.ReentrancyHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Opportunity.MvvmUniverse
{
    internal sealed class ConstPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ConstPropertyChangedEventArgs(string propertyName)
            : base(propertyName) { }

        public static readonly ConstPropertyChangedEventArgs PropertyReset = new ConstPropertyChangedEventArgs(null);
        public static readonly ConstPropertyChangedEventArgs Count = new ConstPropertyChangedEventArgs(nameof(Count));
        public static readonly ConstPropertyChangedEventArgs Current = new ConstPropertyChangedEventArgs(nameof(Current));
        public static readonly ConstPropertyChangedEventArgs Progress = new ConstPropertyChangedEventArgs(nameof(Progress));
        public static readonly ConstPropertyChangedEventArgs IsExecuting = new ConstPropertyChangedEventArgs(nameof(IsExecuting));
        public static readonly ConstPropertyChangedEventArgs AttachedCommand = new ConstPropertyChangedEventArgs(nameof(AttachedCommand));
        public static readonly ConstPropertyChangedEventArgs QueuedValue = new ConstPropertyChangedEventArgs(nameof(QueuedValue));
        public static readonly ConstPropertyChangedEventArgs HasValue = new ConstPropertyChangedEventArgs(nameof(HasValue));
        public static readonly ConstPropertyChangedEventArgs IsEmpty = new ConstPropertyChangedEventArgs(nameof(IsEmpty));
        public static readonly ConstPropertyChangedEventArgs PeekValue = new ConstPropertyChangedEventArgs(nameof(PeekValue));
    }
}
