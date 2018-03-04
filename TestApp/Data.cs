using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Threading;
using Opportunity.MvvmUniverse;

namespace TestApp
{
    public class DataItem : ObservableObject
    {
        private string name;
        public string Name { get => this.name; set => Set(ref this.name, value); }

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
                await Task.Delay(1000);
                var s = index / 5 * 5;
                Debug.WriteLine($"Loaded {s} to {s + 5}.");
                for (int i = s; i < s + 5; i++)
                {
                    this[i].Name = this[i].Name.Split()[1];
                }
                return LoadItemsResult.Create(s, this.Skip(s).Take(5));
            });
        }
    }
}
