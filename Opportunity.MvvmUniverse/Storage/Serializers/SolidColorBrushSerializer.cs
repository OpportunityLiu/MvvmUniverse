using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class SolidColorBrushSerializer : ISerializer<SolidColorBrush>
    {
        private static readonly int size = Unsafe.SizeOf<Color>() + sizeof(double);
        private static readonly int colorOffset = Unsafe.SizeOf<double>();

        public bool IsFixedSize => true;

        public int CaculateSize(in SolidColorBrush value) => size;

        public void Serialize(in SolidColorBrush value, Span<byte> storage)
        {
            if (value == null)
            {
                storage.NonPortableCast<byte, double>()[0] = double.NaN;
                return;
            }
            storage.NonPortableCast<byte, double>()[0] = value.Opacity;
            storage.Slice(colorOffset).NonPortableCast<byte, Color>()[0] = value.Color;
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref SolidColorBrush value)
        {
            if (storage.IsEmpty)
            {
                value = null;
                return;
            }
            var opa = storage.NonPortableCast<byte, double>()[0];
            if (double.IsNaN(opa))
            {
                value = null;
                return;
            }
            var color = storage.Slice(colorOffset).NonPortableCast<byte, Color>()[0];
            if (value == null)
                value = new SolidColorBrush(color);
            else
                value.Color = color;
            value.Opacity = opa;
        }
    }
}
