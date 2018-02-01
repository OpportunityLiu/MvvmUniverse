namespace Opportunity.MvvmUniverse.Commands
{
    public interface IControllable
    {
        /// <summary>
        /// A tag associated with this object.
        /// </summary>
        object Tag { get; set; }

        bool IsEnabled { get; set; }
    }
}