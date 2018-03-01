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
    }

    public class DataList : IncrementalLoadingList<DataItem>
    {
        public override bool HasMoreItems => this.Count < 1000;

        protected override IAsyncOperation<IEnumerable<DataItem>> LoadItemsAsync(int count)
        {
            Debug.WriteLine($"{count} items loading.");
            return AsyncInfo.Run(async token =>
            {
                await Task.Delay(count * 50);
                Debug.WriteLine($"{count} items loaded.");
                return Enumerable.Range(this.Count, count).Select(i => new DataItem { Name = i.ToString() });
            });
        }
    }
}
