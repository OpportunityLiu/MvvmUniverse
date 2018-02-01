using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void Settings()
        {
            var c = new TestSettingCollection
            {
                MyProperty = 12
            };
            var c2 = new TestSettingCollection();
            c.MyProperty2 = StringComparison.CurrentCulture;
            c.MyProperty4 = "";
            Assert.AreEqual(c.MyProperty, c2.MyProperty);
            Assert.AreEqual(c.MyProperty2, c2.MyProperty2);
            c2.MyProperty = 1213;
            c.MyProperty2 = StringComparison.OrdinalIgnoreCase;
            Assert.AreEqual(c.MyProperty, c2.MyProperty);
            Assert.AreEqual(c.MyProperty2, c2.MyProperty2);
        }

        class TestSettingCollection : SettingCollection
        {
            public TestSettingCollection() : base(ApplicationData.Current.RoamingSettings, "haha")
            {

            }



            public int MyProperty
            {
                get => GetFromContainer(MyPropertyProperty);
                set => SetToContainer(MyPropertyProperty, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly SettingProperty<int> MyPropertyProperty =
                 SettingProperty.Create("MyProperty", 0, MtpropCB);


            public static void MtpropCB(SettingCollection sender, SettingPropertyChangedEventArgs<int> e)
            {

            }



            public StringComparison MyProperty2
            {
                get => GetFromContainer(MyProperty2Property);
                set => SetToContainer(MyProperty2Property, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly SettingProperty<StringComparison> MyProperty2Property =
                SettingProperty.Create<StringComparison>("MyProperty2", 0, MtpropCB2);


            public static void MtpropCB2(SettingCollection sender, SettingPropertyChangedEventArgs<StringComparison> e)
            {

            }

            public string MyProperty3
            {
                get => GetFromContainer(MyProperty3Property);
                set => SetToContainer(MyProperty3Property, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly SettingProperty<string> MyProperty3Property =
                SettingProperty.Create<string>("MyProperty3", null);

            public string MyProperty4
            {
                get => GetFromContainer(MyProperty4Property);
                set => SetToContainer(MyProperty4Property, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly SettingProperty<string> MyProperty4Property =
                SettingProperty.Create<string>("MyProperty4", "");

            public DateTimeOffset DateTimeP
            {
                get => GetFromContainer(DateTimePP);
                set => SetToContainer(DateTimePP, value);
            }

            // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
            public static readonly SettingProperty<DateTimeOffset> DateTimePP =
                SettingProperty.Create<DateTimeOffset>(nameof(DateTimeP));
        }

        [TestMethod]
        public void WinRTTypes()
        {
            TypeCollection<bool>.Test(true);
            TypeCollection<byte>.Test(32);

            TypeCollection<float>.Test(-12.214f);
            TypeCollection<double>.Test(2143.214);

            TypeCollection<ushort>.Test(2132);
            TypeCollection<short>.Test(-3125);

            TypeCollection<uint>.Test(32153416);
            TypeCollection<int>.Test(-123463256);

            TypeCollection<ulong>.Test(314564745221763);
            TypeCollection<long>.Test(-34165357546336475);

            TypeCollection<char>.Test('h');
            TypeCollection<string>.Test("123", new string("123".ToCharArray()));

            TypeCollection<Guid>.Test(Guid.NewGuid());

            TypeCollection<DateTimeOffset>.Test(DateTimeOffset.Now);
            TypeCollection<TimeSpan>.Test(TimeSpan.FromSeconds(12.5132435464524));

            TypeCollection<Point>.Test(new Point(123, 123));
            TypeCollection<Rect>.Test(new Rect(12, 23, 34, 56));
            TypeCollection<Size>.Test(new Size(12, 24));
        }

        [TestMethod]
        public void SerializerTypes()
        {
            TypeCollection<StringComparison>.Test(StringComparison.CurrentCulture);

            TypeCollection<Uri>.Test(null);
            TypeCollection<Uri>.Test(new Uri("http://example.com"));
            TypeCollection<Uri>.Test(new Uri("http://example.com"), new Uri("http://example.com"));
            TypeCollection<Uri>.Test(new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative));
            TypeCollection<Uri>.Test(new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative), new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative));
            TypeCollection<Uri>.Test(new Uri("", UriKind.Relative));
            TypeCollection<Uri>.Test(new Uri("", UriKind.Relative), new Uri(new string(' ', 0), UriKind.Relative));
            TypeCollection<Uri>.Test(new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"));
            TypeCollection<Uri>.Test(new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"), new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"));

            TypeCollection<sbyte>.Test(0);
            TypeCollection<sbyte>.Test(127);
            TypeCollection<sbyte>.Test(-128);
            TypeCollection<sbyte>.Test(1);
            TypeCollection<sbyte>.Test(-1);

            TypeCollection<DateTime>.Test(DateTime.Now);
            TypeCollection<DateTime>.Test(new DateTime(12435, DateTimeKind.Local));
            TypeCollection<DateTime>.Test(new DateTime(12435, DateTimeKind.Unspecified));
            TypeCollection<DateTime>.Test(new DateTime(12435, DateTimeKind.Utc));

            TypeCollection<A>.Test(new A { b = "sd", c = "sf" });
        }

        class A : IEquatable<A>
        {
            public string b;
            public string c;

            public bool Equals(A other) => this.b == other.b && this.c == other.c;
        }

        class TypeCollection<T> : SettingCollection
        {
            public static void Test()
            {
                var t1 = Activator.CreateInstance<T>();
                var t2 = Activator.CreateInstance<T>();
                var c = new TypeCollection<T> { Property = t1 };
                Assert.AreEqual(t1, c.Property);
                Assert.AreEqual(t2, c.Property);
            }

            public static void Test(T v)
            {
                var c = new TypeCollection<T> { Property = v };
                Assert.AreEqual(v, c.Property);
            }

            public static void Test(T v1, T v2)
            {
                var c = new TypeCollection<T> { Property = v1 };
                Assert.AreEqual(v1, c.Property);
                Assert.AreEqual(v2, c.Property);
            }

            private static Random random = new Random();

            public TypeCollection()
                : base(ApplicationData.Current.LocalSettings, $"TypeCollection-{typeof(T)}-{random.Next()}")
            {

            }

            public T Property { get => GetFromContainer(Setting); set => SetToContainer(Setting, value); }
            static readonly SettingProperty<T> Setting = SettingProperty.Create<T>(nameof(Property));
        }
    }
}
