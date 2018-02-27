using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class ColorSerializer :
        ISerializer<Color>, ISerializer<SolidColorBrush>
    {
        public static ColorSerializer Instance { get; } = new ColorSerializer();

        public void Serialize(in Color value, DataWriter storage)
        {
            var data = new byte[4];
            data[0] = value.A;
            data[1] = value.R;
            data[2] = value.G;
            data[3] = value.B;
            storage.WriteBytes(data);
        }

        public void Deserialize(DataReader storage, ref Color value)
        {
            var data = new byte[4];
            storage.ReadBytes(data);
            value = Color.FromArgb(data[0], data[1], data[2], data[3]);
        }

        void ISerializer<SolidColorBrush>.Serialize(in SolidColorBrush value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteDouble(double.NaN);
                return;
            }
            storage.WriteDouble(value.Opacity);
            Serialize(value.Color, storage);
        }

        void ISerializer<SolidColorBrush>.Deserialize(DataReader storage, ref SolidColorBrush value)
        {
            var opa = storage.ReadDouble();
            if (double.IsNaN(opa))
            {
                value = null;
                return;
            }
            var color = default(Color);
            Deserialize(storage, ref color);
            if (value == null)
            {
                value = new SolidColorBrush();
            }
            value.Opacity = opa;
            value.Color = color;
        }
    }
}
