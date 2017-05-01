using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Opportunity.MvvmUniverse.Collections;

namespace Opportunity.MvvmUniverse.Views
{
    public sealed class Navigator
    {
        private static readonly Dictionary<int, Navigator> dic = new Dictionary<int, Navigator>();

        public static Navigator CreateOrGetForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (dic.TryGetValue(id, out var r))
                return r;
            return dic[id] = new Navigator();
        }

        public static Navigator GetForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (dic.TryGetValue(id, out var r))
                return r;
            return null;
        }

        public static bool DestoryForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (!dic.TryGetValue(id, out var r))
                return false;
            dic.Remove(id);
            r.destory();
            return true;
        }

        public IList<INavigationHandler> Handlers { get; } = new ObservableCollection<INavigationHandler>();

        private SystemNavigationManager manager = SystemNavigationManager.GetForCurrentView();

        private Navigator()
        {
            this.manager.BackRequested += this.manager_BackRequested;
        }

        public AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get => manager.AppViewBackButtonVisibility;
            private set => manager.AppViewBackButtonVisibility = value;
        }

        private void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {

        }

        private void destory()
        {
            this.manager.BackRequested -= manager_BackRequested;
            this.manager = null;
        }
    }
}
