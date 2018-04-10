﻿using Opportunity.MvvmUniverse.Collections;
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
    internal static class EventArgsConst
    {
        public static readonly ConstPropertyChangedEventArgs PropertyReset = new ConstPropertyChangedEventArgs(null);
        public static readonly ConstPropertyChangedEventArgs CountPropertyChanged = new ConstPropertyChangedEventArgs(nameof(IList.Count));
        public static readonly ConstPropertyChangedEventArgs ProgressPropertyChanged = new ConstPropertyChangedEventArgs(nameof(Commands.IAsyncCommandWithProgress<object>.Progress));

        public static readonly VectorChangedEventArgs VectorReset = new VectorChangedEventArgs(CollectionChange.Reset);
    }

    internal sealed class ConstPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ConstPropertyChangedEventArgs(string propertyName)
            : base(propertyName) { }
    }
}