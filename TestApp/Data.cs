using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TestApp
{
    public class DataItem
    {
        public string Name { get; set; }

        public int Index => GetHashCode();
    }

    public class DataList : FixedIncrementalLoadingList<DataItem>
    {
        public DataList() : base(500) { }

        private static IEnumerable<DataItem> getData(int start, int count)
        {
            return Enumerable.Range(start, count).Select(i => new DataItem { Name = i.ToString() });
        }

        public void Swap(int i)
        {
            this[i] = new DataItem { Name = this[i].Name + "*" };
        }

        protected override DataItem CreatePlaceholder(int index)
        {
            return new DataItem { Name = "PH " + index };
        }

        protected override IAsyncOperation<LoadItemsResult<DataItem>> LoadItemAsync(int index)
        {
            return AsyncInfo.Run(async token =>
            {
                await Task.Delay(200);
                var s = index / 5 * 5;
                Debug.WriteLine($"Loaded {s} to {s + 5}.");
                return new LoadItemsResult<DataItem>(s, getData(s, 5));
            });
        }
    }
}
