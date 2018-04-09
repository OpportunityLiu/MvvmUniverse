using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Test
{
    [TestClass]
    public class VMTest
    {
        [TestMethod]
        public void VMDerived()
        {
            var v = new TestVM();
        }

        public class TestVM : ViewModelBase
        {
            public TestVM()
            {
                this.Commands["Test1"] = AsyncCommand.Create(async c => await Task.Delay(1));
                this.Commands["Test2"] = AsyncCommand.Create<int>(async (c, i) => await Task.Delay(1));
                this.Commands["Test3"] = Command.Create(c => { });
                this.Commands["Test4"] = Command.Create<int>((c, i) => { });
                foreach (var item in Commands.Values)
                {
                    Assert.AreSame(this, ((IControllable)item).Tag);
                }
            }
        }
    }
}
