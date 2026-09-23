using BleCommands.Core.Exceptions;

namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a Bluetooth Low Energy scanner that searches for nearby BLE devices.
    /// </summary>
    /// <typeparam name="TDevice">
    /// A specific device implementation.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This interface defines a platform-independent contract for <b>finding</b> a BLE device
    /// by its name. The actual scanning mechanism is platform-specific and is intentionally
    /// not part of this contract: implementations for MAUI and Windows expose their own
    /// <c>ScanAsync</c> overloads with platform-specific parameters (scan modes, advertisement
    /// filters, etc.) and a <c>DeviceDiscovered</c> event.
    /// </para>
    /// <para>
    /// Consumers that only need to locate a device by name should program against this
    /// interface. Consumers that need fine-grained control over the scanning process
    /// (custom filters, scan modes, continuous discovery) should use the concrete
    /// platform-specific implementation directly.
    /// </para>
    /// </remarks>
    public interface IBleScanner<TDevice>
        where TDevice : IDevice
    {
        /// <summary>
        /// Searches for a Bluetooth device by name with default timeout (5 seconds).
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <returns>Found device or <c>null</c> if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceName"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on BLE scanning errors.</exception>
        Task<TDevice?> FindDeviceAsync(string deviceName);

        /// <summary>
        /// Searches for a Bluetooth device by name with the specified timeout.
        /// </summary>
        /// <param name="deviceName">The name of the device to search for.</param>
        /// <param name="timeout">Maximum wait time for device discovery.</param>
        /// <returns>Found device or <c>null</c> if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceName"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified timeout is less than or equal to zero,
        /// or greater than 60 seconds.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on BLE scanning errors.</exception>
        Task<TDevice?> FindDeviceAsync(string deviceName, TimeSpan timeout);

        /// <summary>
        /// Searches for a Bluetooth device by name with the specified timeout.
        /// </summary>
        /// <param name="deviceName">The name of the device to search for.</param>
        /// <param name="timeout">Maximum wait time for device discovery.</param>
        /// <param name="token">Token that cancels the search before the timeout.</param>
        /// <returns>Found device or <c>null</c> if the timeout expired.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceName"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified timeout is less than or equal to zero,
        /// or greater than 60 seconds.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the search is canceled by <paramref name="token"/>.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on BLE scanning errors.</exception>
        Task<TDevice?> FindDeviceAsync(
            string deviceName,
            TimeSpan timeout,
            CancellationToken token);
    }
}
