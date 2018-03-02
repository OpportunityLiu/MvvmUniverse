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

    public class DataList : IncrementalLoadingList<DataItem>
    {
        public DataList() : base(getData(0, 100)) { }

        private static IEnumerable<DataItem> getData(int start, int count)
        {
            return Enumerable.Range(start, count).Select(i => new DataItem { Name = i.ToString() });
        }

        public override bool HasMoreItems => Count < 200;

        protected override IAsyncOperation<IEnumerable<DataItem>> LoadItemsAsync(int count)
        {
            Debug.WriteLine($"{count} items loading.");
            return AsyncInfo.Run(async token =>
            {
                await Task.Delay(count * 50);
                Debug.WriteLine($"{count} items loaded.");
                return getData(this.Count, count);
            });
        }

        public void Swap(int i)
        {
            this[i] = new DataItem { Name = this[i].Name + "*" };
        }
    }
}
