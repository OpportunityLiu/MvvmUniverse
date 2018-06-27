namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// View with view model.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// View model of this view.
        /// </summary>
        ViewModelBase ViewModel { get; set; }
    }
}