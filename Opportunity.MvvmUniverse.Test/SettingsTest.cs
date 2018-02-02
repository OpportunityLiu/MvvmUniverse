using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Opportunity.MvvmUniverse.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void Settings()
        {
            var c = new TestCollection
            {
                MyProperty = 12
            };
            var c2 = new TestCollection();
            c.MyProperty2 = StringComparison.CurrentCulture;
            c.MyProperty4 = "";
            Assert.AreEqual(c.MyProperty, c2.MyProperty);
            Assert.AreEqual(c.MyProperty2, c2.MyProperty2);
            c2.MyProperty = 1213;
            c.MyProperty2 = StringComparison.OrdinalIgnoreCase;
            Assert.AreEqual(c.MyProperty, c2.MyProperty);
            Assert.AreEqual(c.MyProperty2, c2.MyProperty2);
        }

        class TestCollection : RoamingStorageObject
        {
            public TestCollection() : base(ApplicationData.Current.RoamingSettings, "haha")
            {

            }



            public int MyProperty
            {
                get => GetFromContainer(MyPropertyProperty);
                set => SetToContainer(MyPropertyProperty, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly StorageProperty<int> MyPropertyProperty =
                 StorageProperty.Create("MyProperty", 0, MtpropCB);


            public static void MtpropCB(StorageObject sender, StoragePropertyChangedEventArgs<int> e)
            {

            }



            public StringComparison MyProperty2
            {
                get => GetFromContainer(MyProperty2Property);
                set => SetToContainer(MyProperty2Property, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly StorageProperty<StringComparison> MyProperty2Property =
                StorageProperty.Create<StringComparison>("MyProperty2", 0, MtpropCB2);


            public static void MtpropCB2(StorageObject sender, StoragePropertyChangedEventArgs<StringComparison> e)
            {

            }

            public string MyProperty3
            {
                get => GetFromContainer(MyProperty3Property);
                set => SetToContainer(MyProperty3Property, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly StorageProperty<string> MyProperty3Property =
                StorageProperty.Create<string>("MyProperty3", null);

            public string MyProperty4
            {
                get => GetFromContainer(MyProperty4Property);
                set => SetToContainer(MyProperty4Property, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly StorageProperty<string> MyProperty4Property =
                StorageProperty.Create<string>("MyProperty4", "");

            public DateTimeOffset DateTimeP
            {
                get => GetFromContainer(DateTimePP);
                set => SetToContainer(DateTimePP, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly StorageProperty<DateTimeOffset> DateTimePP =
                StorageProperty.Create<DateTimeOffset>(nameof(DateTimeP));
        }

        [TestMethod]
        public void WinRTTypes()
        {
            Test(new[] { true, false });
            Test(new byte[] { 32, 34, 255 });

            Test(new[] { -12.214f, float.MinValue, float.PositiveInfinity, float.NaN, float.NegativeInfinity });
            Test(new[] { 2143.214, double.MinValue, double.PositiveInfinity, double.NaN, double.NegativeInfinity });

            Test(new ushort[] { 2132 });
            Test(new short[] { -3125 });

            Test(new[] { 32153416U, uint.MaxValue, 0U });
            Test(new[] { -123463256, int.MaxValue, int.MinValue });

            Test(new[] { 314564745221763UL });
            Test(new[] { -34165357546336475L });

            Test(new[] { '\0', 'h', char.MaxValue });
            Test(new string[] { "", "null", "null_", "null_string", "null___" });

            Test(new[] { Guid.NewGuid() });

            Test(new[] { DateTimeOffset.Now, default });
            Test(new[] { TimeSpan.FromSeconds(12.5132435464524), default });

            Test(new[] { new Point(123, 123) });
            Test(new[] { new Rect(12, 23, 34, 56) });
            Test(new[] { new Size(12, 24) });
        }

        [TestMethod]
        public void SerializerTypes()
        {
            TypeCollection.Test(StringComparison.CurrentCulture);

            TypeCollection.Test(default(Uri));
            TypeCollection.Test(new Uri("http://example.com"));
            TypeCollection.Test(new Uri("http://example.com"), new Uri("http://example.com"));
            TypeCollection.Test(new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative));
            TypeCollection.Test(new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative), new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative));
            TypeCollection.Test(new Uri("", UriKind.Relative));
            TypeCollection.Test(new Uri("", UriKind.Relative), new Uri(new string(' ', 0), UriKind.Relative));
            TypeCollection.Test(new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"));
            TypeCollection.Test(new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"), new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"));

            TypeCollection.Test(0);
            TypeCollection.Test(127);
            TypeCollection.Test(-128);
            TypeCollection.Test(1);
            TypeCollection.Test(-1);

            TypeCollection.Test(DateTime.Now);
            TypeCollection.Test(new DateTime(12435, DateTimeKind.Local));
            TypeCollection.Test(new DateTime(12435, DateTimeKind.Unspecified));
            TypeCollection.Test(new DateTime(12435, DateTimeKind.Utc));

            TypeCollection.Test(new IntPtr(12));
            TypeCollection.Test(new UIntPtr(21));
        }

        [UITestMethod]
        public void SerializerUITypes()
        {
            TypeCollection.Test(default(Color));
            TypeCollection.Test(Color.FromArgb(1, 2, 3, 4));
            TypeCollection.Test(Colors.AliceBlue);
            TypeCollection.Test(Colors.Transparent);

            int brushComp(SolidColorBrush b1, SolidColorBrush b2)
            {
                if (b1 == null && b2 == null)
                    return 0;
                if (b1 == null)
                    return -1;
                if (b2 == null)
                    return 1;
                if (b1.Color != b2.Color)
                    return Comparer<Color>.Default.Compare(b1.Color, b2.Color);
                return Comparer<double>.Default.Compare(b1.Opacity, b2.Opacity);
            }

            TypeCollection.Test<SolidColorBrush>(brushComp);
            TypeCollection.Test(new SolidColorBrush(Color.FromArgb(1, 2, 3, 4)), brushComp);
            TypeCollection.Test(new SolidColorBrush(Colors.AliceBlue), brushComp);
            TypeCollection.Test(new SolidColorBrush(Colors.Transparent), brushComp);
            TypeCollection.Test(new SolidColorBrush(Color.FromArgb(1, 2, 3, 4)) { Opacity = 0 }, brushComp);
            TypeCollection.Test(new SolidColorBrush(Colors.AliceBlue) { Opacity = 0 }, brushComp);
            TypeCollection.Test(new SolidColorBrush(Colors.Transparent) { Opacity = 0 }, brushComp);
            TypeCollection.Test(new SolidColorBrush(Color.FromArgb(1, 2, 3, 4)) { Opacity = 0.424321 }, brushComp);
            TypeCollection.Test(new SolidColorBrush(Colors.AliceBlue) { Opacity = 0.42135 }, brushComp);
            TypeCollection.Test(new SolidColorBrush(Colors.Transparent) { Opacity = 0.423153 }, brushComp);
        }

        void Test<T>(T[] values, T[] valuesCopy = null, Comparison<T> comparer = null)
        {
            if (valuesCopy != null)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    TypeCollection.Test(values[i], valuesCopy[i], comparer);
                }
            }
            else
            {
                foreach (var item in values)
                {
                    TypeCollection.Test(item, comparer);
                }
            }
            TypeCollection.TestCollction(values, comparer);
            TypeCollection.TestCollction(new List<T>(values), comparer);
            TypeCollection.TestCollction(new Collections.ObservableList<T>(values), comparer);

            values = values.Concat(Enumerable.Repeat<T>(default, 5)).ToArray();
            TypeCollection.TestCollction(values, comparer);
            TypeCollection.TestCollction(new List<T>(values), comparer);
            TypeCollection.TestCollction(new Collections.ObservableList<T>(values), comparer);

            values = new T[5];
            TypeCollection.TestCollction(values, comparer);
            TypeCollection.TestCollction(new List<T>(values), comparer);
            TypeCollection.TestCollction(new Collections.ObservableList<T>(values), comparer);

            values = new T[0];
            TypeCollection.TestCollction(values, comparer);
            TypeCollection.TestCollction(new List<T>(values), comparer);
            TypeCollection.TestCollction(new Collections.ObservableList<T>(values), comparer);

            TypeCollection.TestCollction(default(T[]), comparer);
            TypeCollection.TestCollction(default(List<T>), comparer);
            TypeCollection.TestCollction(default(Collections.ObservableList<T>), comparer);
        }

        static class TypeCollection
        {
            public static void TestCollction<T, TEle>(T v, Comparison<TEle> comparer = null)
                where T : ICollection<TEle>, ICollection
            {
                var c = new TypeCollection<T> { Property = v };
                if (comparer == null)
                {
                    CollectionAssert.AreEqual(v, c.Property);
                }
                else
                {
                    var p = c.Property.ToList();
                    var q = v.ToList();
                    Assert.AreEqual(q.Count, p.Count);
                    for (var i = 0; i < p.Count; i++)
                    {
                        Assert.IsTrue(comparer(q[i], p[i]) == 0);
                    }
                }
            }

            public static void Test<T>(Comparison<T> comparer = null)
                where T : new()
            {
                var v1 = new T();
                var v2 = new T();
                var c = new TypeCollection<T> { Property = v1 };
                if (comparer == null)
                {
                    Assert.AreEqual(v1, c.Property);
                    Assert.AreEqual(v2, c.Property);
                }
                else
                {
                    Assert.IsTrue(comparer(v1, c.Property) == 0);
                    Assert.IsTrue(comparer(v2, c.Property) == 0);
                }
            }

            public static void Test<T>(T v, Comparison<T> comparer = null)
            {
                var c = new TypeCollection<T> { Property = v };
                if (comparer == null)
                {
                    Assert.AreEqual(v, c.Property);
                }
                else
                {
                    Assert.IsTrue(comparer(v, c.Property) == 0);
                }
            }

            public static void Test<T>(T v1, T v2, Comparison<T> comparer = null)
            {
                var c = new TypeCollection<T> { Property = v1 };
                if (comparer == null)
                {
                    Assert.AreEqual(v1, c.Property);
                    Assert.AreEqual(v2, c.Property);
                }
                else
                {
                    Assert.IsTrue(comparer(v1, c.Property) == 0);
                    Assert.IsTrue(comparer(v2, c.Property) == 0);
                }
            }
        }

        class TypeCollection<T> : LocalStorageObject
        {
            private static Random random = new Random();
            public TypeCollection()
                : base(ApplicationData.Current.LocalSettings, $"TypeCollection-{typeof(T)}-{random.Next()}") { }

            public T Property { get => GetFromContainer(Setting); set => SetToContainer(Setting, value); }
            static readonly StorageProperty<T> Setting = StorageProperty.Create<T>(nameof(Property));
        }
    }
}
