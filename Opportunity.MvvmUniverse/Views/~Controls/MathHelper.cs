using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using static System.Math;

namespace Opportunity.MvvmUniverse.Views
{
    internal static class MathHelper
    {

        public static double Diff(Thickness t1, Thickness t2)
        {
            var l = Abs(t1.Left - t2.Left);
            var r = Abs(t1.Right - t2.Right);
            var t = Abs(t1.Top - t2.Top);
            var b = Abs(t1.Bottom - t2.Bottom);
            return Max(Max(l, r), Max(t, b));
        }

        public static double BoundToZero(this double v) => v < 0 ? 0 : v;
    }
}
