using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Delegates
{
    public delegate Task AsyncAction();
    public delegate Task AsyncAction<T>(T parameter);
}
