namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// An interface implemented by a parent object (e.g., Device or Service),
    /// that manages the lifetime of its child objects.
    /// Parent registers all its child objects via RegisterChild
    /// and cascades all registered children when it disposes.
    /// </summary>
    public interface IChildDisposer
    {
        /// <summary>
        /// Register a child object for cascade disposing.
        /// </summary>
        void RegisterChild(IDisposable child);
    }
}
