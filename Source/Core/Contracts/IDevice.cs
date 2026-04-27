namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a generic Bluetooth Low Energy device with platform-specific types.
    /// </summary>
    /// <typeparam name="TDevice">The platform-specific device type.</typeparam>
    /// <typeparam name="TService">The platform-specific service type.</typeparam>
    /// <typeparam name="TCharacteristic">The platform-specific characteristic type.</typeparam>
    public interface IDevice<TDevice, TService, TCharacteristic> : IDisposable
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
        /// Gets the platform-specific native device instance.
        /// </summary>
        TDevice? NativeDevice { get; }

        /// <summary>
        /// Establishes a connection to the Bluetooth device asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        Task ConnectAsync(CancellationToken token = default);

        /// <summary>
        /// Retrieves all GATT services available on the device asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A read-only list of services exposed by the device.</returns>
        Task<IReadOnlyList<IService<TService, TCharacteristic>>> GetServicesAsync(CancellationToken token = default);

        /// <summary>
        /// Retrieves a specific GATT service by its UUID asynchronously.
        /// </summary>
        /// <param name="id">The UUID of the service to retrieve.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>The requested service, or null if not found.</returns>
        Task<IService<TService, TCharacteristic>?> GetServiceAsync(Guid id, CancellationToken token = default);
    }
}
