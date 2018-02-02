using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class ColorSerializer : ISerializer<Color>
    {
        public Color Deserialize(object value)
        {
            if (!(value is uint data))
                return default;
            var b = (byte)((data & 0x000000FF));
            var g = (byte)((data & 0x0000FF00) >> 8);
            var r = (byte)((data & 0x00FF0000) >> 16);
            var a = (byte)((data & 0xFF000000) >> 24);
            return Color.FromArgb(a, r, g, b);
        }

        public object Serialize(Color value)
        {
            return ((uint)value.A << 24) + ((uint)value.R << 16) + ((uint)value.G << 8) + value.B;
        }
    }
}
