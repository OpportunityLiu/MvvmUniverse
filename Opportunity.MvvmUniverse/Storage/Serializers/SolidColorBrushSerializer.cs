using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class SolidColorBrushSerializer : ISerializer<SolidColorBrush>
    {
        private const ulong NULL_HINT = 0x7A26DF24_44CEA6B8UL;
        private const ulong NULL_REPLACE = 0x7A26DF25_44CEA6B8UL;

        public SolidColorBrush Deserialize(object value)
        {
            if (!(value is ulong data))
                return default;
            if (data == NULL_HINT)
                return null;
            var b = (byte)((data & 0x000000FF));
            var g = (byte)((data & 0x0000FF00) >> 8);
            var r = (byte)((data & 0x00FF0000) >> 16);
            var a = (byte)((data & 0xFF000000) >> 24);
            var op = (uint)(data >> 32);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b)) { Opacity = op / (double)uint.MaxValue };
        }

        public object Serialize(SolidColorBrush value)
        {
            if (value == null)
                return NULL_HINT;
            var color = value.Color;
            var op = value.Opacity;
            if (op < 0) op = 0;
            else if (op > 1) op = 1;
            else if (double.IsNaN(op)) op = 1;
            var opacaty = (ulong)(op * uint.MaxValue);
            var r = (opacaty << 32) + ((ulong)color.A << 24) + ((ulong)color.R << 16) + ((ulong)color.G << 8) + color.B;
            if (r == NULL_HINT)
            {
                return NULL_REPLACE;
            }
            return r;
        }
    }
}
