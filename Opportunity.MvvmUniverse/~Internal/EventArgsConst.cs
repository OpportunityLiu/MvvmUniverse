using Opportunity.MvvmUniverse.Collections;
using System;
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
        public static readonly PropertyChangedEventArgs PropertyReset = new PropertyChangedEventArgs(null);
        public static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");

        public static readonly IVectorChangedEventArgs VectorReset = new VectorChangedEventArgs(CollectionChange.Reset);
    }
}
