namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a Bluetooth Low Energy device.
    /// </summary>
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// Occurs when the device is disconnected.
        /// </summary>
        event EventHandler? Disconnected;

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the device is currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Establishes a connection to the Bluetooth device asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        Task ConnectAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Represents a generic Bluetooth Low Energy device with platform-specific types.
    /// </summary>
    /// <typeparam name="TNativeDevice">The platform-specific device type.</typeparam>
    /// <typeparam name="TService">
    /// A specific service implementation.
    /// </typeparam>
    public interface IDevice<TNativeDevice, TService> : IDevice
        where TService : IService
    {
        /// <summary>
        /// Gets the platform-specific native device instance.
        /// </summary>
        TNativeDevice? NativeDevice { get; }

        /// <summary>
        /// Retrieves all GATT services available on the device asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A read-only list of services exposed by the device.</returns>
        Task<IReadOnlyList<TService>> GetServicesAsync(CancellationToken token = default);

        /// <summary>
        /// Retrieves a specific GATT service by its UUID asynchronously.
        /// </summary>
        /// <param name="id">The UUID of the service to retrieve.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>The requested service, or null if not found.</returns>
        Task<TService?> GetServiceAsync(Guid id, CancellationToken token = default);
    }
}
