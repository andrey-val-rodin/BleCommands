namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a Bluetooth Low Energy device.
    /// </summary>
    public interface IDevice : IDisposable, IChildDisposer
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
        /// Initiates process of connection to the device.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <remarks>
        /// This method is intended to be called once per instance lifecycle.
        /// The connection will be established shortly.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown if the device has been disposed.</exception>
        Task ConnectAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Represents a generic Bluetooth Low Energy device with platform-specific types.
    /// </summary>
    /// <typeparam name="TNativeDevice">The platform-specific device type.</typeparam>
    /// <typeparam name="TService">A specific service implementation.</typeparam>
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
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="IDevice.ConnectAsync(CancellationToken)"/> has not been called.
        /// </exception>
        Task<IReadOnlyList<TService>> GetServicesAsync(CancellationToken token = default);

        /// <summary>
        /// Retrieves a specific GATT service by its UUID asynchronously.
        /// </summary>
        /// <param name="id">The UUID of the service to retrieve.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>The requested service, or <c>null</c> if not found.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="IDevice.ConnectAsync(CancellationToken)"/> has not been called.
        /// </exception>
        Task<TService?> GetServiceAsync(Guid id, CancellationToken token = default);
    }
}
