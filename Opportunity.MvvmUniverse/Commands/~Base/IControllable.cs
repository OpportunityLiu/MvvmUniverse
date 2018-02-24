namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Object that can be tagged and controlled.
    /// </summary>
    public interface IControllable
    {
        /// <summary>
        /// A tag associated with this object.
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Whether the instance can interactive.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}