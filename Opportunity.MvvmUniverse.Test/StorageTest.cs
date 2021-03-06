﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Opportunity.MvvmUniverse.Storage;
using Opportunity.MvvmUniverse.Storage.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class StorageTest
    {
        public class St1 : StorageObject
        {
            public St1(string containerPath) : base(containerPath)
            {
            }
        }

        public class St2 : St1
        {
            public St2(string containerPath) : base(containerPath)
            {
            }
        }

        [TestInitialize]
        public async Task Init()
        {
            await ApplicationData.Current.ClearAsync();
        }

        [TestMethod]
        public void BasicFunctions()
        {
            var i1 = new St1("aa");
            Assert.ThrowsException<InvalidOperationException>(() => new St1("aa"));
            Assert.ThrowsException<InvalidOperationException>(() => new St2("aa"));
            Assert.ThrowsException<ArgumentNullException>(() => new St1(""));
            Assert.ThrowsException<ArgumentException>(() => new St1("///"));
        }

        [TestMethod]
        public void WinRTTypes()
        {
            Tester.Test(new[] { true, false });
            Tester.Test(new byte[] { 32, 34, 255 });

            Tester.Test(new[] { -12.214f, float.MinValue, float.PositiveInfinity, float.NaN, float.NegativeInfinity });
            Tester.Test(new[] { 2143.214, double.MinValue, double.PositiveInfinity, double.NaN, double.NegativeInfinity });

            Tester.Test(new ushort[] { 2132 });
            Tester.Test(new short[] { -3125 });

            Tester.Test(new[] { 32153416U, uint.MaxValue, 0U });
            Tester.Test(new[] { -123463256, int.MaxValue, int.MinValue });

            Tester.Test(new[] { 314564745221763UL });
            Tester.Test(new[] { -34165357546336475L });

            Tester.Test(new[] { '\0', 'h', char.MaxValue });
            Tester.Test(new string[] { "", "null", "null_", "null_string", "null___", " ", "123\0\0123", "\0" });

            Tester.Test(new[] { Guid.NewGuid(), default, Guid.Empty, Guid.NewGuid() });

            Tester.Test(new[] { DateTimeOffset.Now, default });
            Tester.Test(new[] { TimeSpan.FromSeconds(12.5132435464524), default });

            Tester.Test(new[] { new Point(123, 123) });
            Tester.Test(new[] { new Rect(12, 23, 34, 56) });
            Tester.Test(new[] { new Size(12, 24) });
        }

        [TestMethod]
        public void NullableTypes()
        {
            Tester.Test(new[] { true, false, default(bool?) });
            Tester.Test(new byte?[] { 32, 34, 255, default });

            Tester.Test(new[] { -12.214f, float.MinValue, float.PositiveInfinity, float.NaN, float.NegativeInfinity, default(float?) });
            Tester.Test(new[] { 2143.214, double.MinValue, double.PositiveInfinity, double.NaN, double.NegativeInfinity, default(double?) });

            Tester.Test(new ushort?[] { 2132, default });
            Tester.Test(new short?[] { -3125, default });

            Tester.Test(new[] { 32153416U, uint.MaxValue, 0U, default(uint?) });
            Tester.Test(new[] { -123463256, int.MaxValue, int.MinValue, default(int?) });

            Tester.Test(new[] { 314564745221763UL, default(ulong?) });
            Tester.Test(new[] { -34165357546336475L, default(long?) });

            Tester.Test(new[] { '\0', 'h', char.MaxValue, default(char?) });

            Tester.Test(new[] { Guid.NewGuid(), default(Guid), Guid.Empty, Guid.NewGuid(), default(Guid?) });

            Tester.Test(new[] { DateTimeOffset.Now, default(DateTimeOffset), default(DateTimeOffset?) });
            Tester.Test(new[] { TimeSpan.FromSeconds(12.5132435464524), default(TimeSpan), default(TimeSpan?) });

            Tester.Test(new[] { new Point(123, 123), default(Point?) });
            Tester.Test(new[] { new Rect(12, 23, 34, 56), default(Rect?) });
            Tester.Test(new[] { new Size(12, 24), default(Size?) });
        }

        [TestMethod]
        public void BasicTypes()
        {
            Tester.Test(new[] { StringComparison.CurrentCulture, (StringComparison)123 });

            Tester.Test(new[]
            {
                default,
                new Uri("http://example.com"),
                new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative),
                new Uri("", UriKind.Relative),
                new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"),
            }, new[]
            {
                default,
                new Uri("http://example.com"),
                new Uri("/saf.saf?as=1232&sdf=23", UriKind.Relative),
                new Uri(new string(' ', 0), UriKind.Relative),
                new Uri("http://example.com:8824/safsa/saf/asf.sdf?dsf=24&saf=4"),
            });

            Tester.Test(new sbyte[] { 0, 127, -128, 1, -1 });

            Tester.Test(new[] { DateTime.Now, new DateTime(12435, DateTimeKind.Local), new DateTime(12435, DateTimeKind.Unspecified), new DateTime(12435, DateTimeKind.Utc) });

            Tester.Test(new[] { new IntPtr(12), IntPtr.Zero });
            Tester.Test(new[] { new UIntPtr(12), UIntPtr.Zero });
        }

        [TestMethod]
        public async Task UITypes()
        {
            await CoreApplication.MainView.Dispatcher.Yield();
            Tester.Test(new[]
            {
                default,
                Color.FromArgb(1, 2, 3, 4),
                Colors.AliceBlue,
                Colors.Transparent,
            });

            Tester.Test(new[]
            {
                default,
                new SolidColorBrush(Color.FromArgb(1, 2, 3, 4)),
                new SolidColorBrush(Colors.AliceBlue),
                new SolidColorBrush(Colors.Transparent),
                new SolidColorBrush(Color.FromArgb(1, 2, 3, 4)) { Opacity = 0 },
                new SolidColorBrush(Colors.AliceBlue) { Opacity = 0 },
                new SolidColorBrush(Colors.Transparent) { Opacity = 0 },
                new SolidColorBrush(Color.FromArgb(1, 2, 3, 4)) { Opacity = 0.424321 },
                new SolidColorBrush(Colors.AliceBlue) { Opacity = 0.42135 },
                new SolidColorBrush(Colors.Transparent) { Opacity = 0.423153 },
            }, comparer: (b1, b2) =>
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
            });
        }

        [TestMethod]
        public void Collection()
        {
            Tester.Test(new[] { new[] { 1, 2, 3 }, new[] { 4, 5 }, new int[0] }, null, (a, b) => (a == b || a.SequenceEqual(b)) ? 0 : 1);
            Tester.Test(new[] { new[] { "1", "22", "333" }, new[] { "4444", "55555" }, new string[0] }, null, (a, b) => (a == b || a.SequenceEqual(b)) ? 0 : 1);
            Tester.Test(new[] { new List<int> { 1, 2, 3 }, new List<int> { 4, 5 }, new List<int>() }, null, (a, b) => (a == b || a.SequenceEqual(b)) ? 0 : 1);
            Tester.Test(new[] { new List<string> { "1", "22", "333" }, new List<string> { "4444", "55555" }, new List<string>() }, null, (a, b) => (a == b || a.SequenceEqual(b)) ? 0 : 1);
            Tester.Test(new[] { new Dictionary<int, string> { [1] = "1", [2] = "22", [3] = "333" }, new Dictionary<int, string>() }, null, (a, b) => (a == b || a.SequenceEqual(b)) ? 0 : 1);
            Tester.Test(new[] { new Dictionary<int, char> { [1] = '1', [2] = '2', [3] = '3' }, new Dictionary<int, char>() }, null, (a, b) => (a == b || a.SequenceEqual(b)) ? 0 : 1);
        }

        //[TestMethod]
        //public void XMLSerializer()
        //{
        //    var p = StorageProperty.CreateLocal("xml", serializer: new XmlSerializer<Point>());
        //    var p2 = StorageProperty.CreateLocal("xml", serializer: new XmlSerializer<Point>());
        //    p.Delete();
        //    p.Value = new Point(1, 2);
        //    p2.Populate();
        //    Assert.AreEqual(new Point(1, 2), p2.Value);
        //}

        private static class Tester
        {
            private static void clearData()
            {
                clear(ApplicationData.Current.LocalSettings);
                clear(ApplicationData.Current.RoamingSettings);
                void clear(ApplicationDataContainer container)
                {
                    container.Values.Clear();
                    while (container.Containers.Count != 0)
                    {
                        container.DeleteContainer(container.Containers.Keys.First());
                    }
                }
            }

            public static void Test<T>(T[] values, T[] valuesCopy = null, Comparison<T> comparer = null)
            {
                clearData();
                if (valuesCopy != null)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        TestSingle(values[i], valuesCopy[i], comparer);
                    }
                }
                else
                {
                    foreach (var item in values)
                    {
                        TestSingle(item, comparer);
                    }
                }
                TestCollction(values, comparer);
                TestCollction(new List<T>(values), comparer);
                TestCollction(new LinkedList<T>(values), comparer);
                TestCollction(new Collections.ObservableList<T>(values), comparer);

                values = values.Concat(Enumerable.Repeat<T>(default, 5)).ToArray();
                TestCollction(values, comparer);
                TestCollction(new List<T>(values), comparer);
                TestCollction(new LinkedList<T>(values), comparer);
                TestCollction(new Collections.ObservableList<T>(values), comparer);

                values = new T[5];
                TestCollction(values, comparer);
                TestCollction(new List<T>(values), comparer);
                TestCollction(new LinkedList<T>(values), comparer);
                TestCollction(new Collections.ObservableList<T>(values), comparer);

                values = new T[0];
                TestCollction(values, comparer);
                TestCollction(new List<T>(values), comparer);
                TestCollction(new LinkedList<T>(values), comparer);
                TestCollction(new Collections.ObservableList<T>(values), comparer);

                TestCollction(default(T[]), comparer);
                TestCollction(default(List<T>), comparer);
                TestCollction(default(LinkedList<T>), comparer);
                TestCollction(default(Collections.ObservableList<T>), comparer);
            }

            private static void TestCollction<T, TEle>(T v, Comparison<TEle> comparer = null)
                where T : ICollection<TEle>, ICollection
            {
                var c = TypeCollection<T>.Instance;
                c.ValueL = v;
                c.Populate();
                if (comparer == null)
                {
                    CollectionAssert.AreEqual(v, c.ValueL);
                }
                else
                {
                    var pro = c.ValueL;
                    if (ReferenceEquals(pro, v))
                        return;
                    var p = pro.ToList();
                    var q = v.ToList();
                    Assert.AreEqual(q.Count, p.Count);
                    for (var i = 0; i < p.Count; i++)
                    {
                        Assert.IsTrue(comparer(q[i], p[i]) == 0);
                    }
                }
            }

            private static void TestDefault<T>(Comparison<T> comparer = null)
                where T : new()
                => TestSingle(new T(), new T(), comparer);

            private static void TestSingle<T>(T v, Comparison<T> comparer = null)
            {
                var c = TypeCollection<T>.Instance;
                c.ValueL = v;
                c.Populate();
                if (comparer == null)
                {
                    Assert.AreEqual(v, c.ValueL);
                }
                else
                {
                    Assert.IsTrue(comparer(v, c.ValueL) == 0);
                }
            }

            private static void TestSingle<T>(T v1, T v2, Comparison<T> comparer = null)
            {
                var c = TypeCollection<T>.Instance;
                c.ValueL = v1;
                c.ValueR = v2;
                c.Populate();
                if (comparer == null)
                {
                    Assert.AreEqual(v1, c.ValueR);
                    Assert.AreEqual(v2, c.ValueL);
                }
                else
                {
                    Assert.IsTrue(comparer(v1, c.ValueR) == 0);
                    Assert.IsTrue(comparer(v2, c.ValueL) == 0);
                }
            }
        }

        private sealed class TypeCollection<T> : StorageObject
        {
            public static TypeCollection<T> Instance { get; } = new TypeCollection<T>();
            private TypeCollection() : base(typeof(T).GetHashCode().ToString()) { }

            [ApplicationSetting(ApplicationDataLocality.Local)]
            public T ValueL
            {
                get => GetStorage<T>();
                set => SetStorage(value);
            }
            [ApplicationSetting(ApplicationDataLocality.Roaming)]
            public T ValueR
            {
                get => GetStorage<T>();
                set => SetStorage(value);
            }

            public void Populate()
            {
                foreach (var item in this.StorageData.Data)
                {
                    this.StorageData.Populate(item.Key, (StorageObjectData.StoragePropertyData<T>)item.Value);
                }
            }

            public void Flush()
            {
                foreach (var item in this.StorageData.Data)
                {
                    this.StorageData.Flush(item.Key, (StorageObjectData.StoragePropertyData<T>)item.Value);
                }
            }
        }
    }
}
