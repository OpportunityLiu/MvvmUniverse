using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}
