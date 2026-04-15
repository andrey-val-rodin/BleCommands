using Core.Exceptions;

namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Bluetooth Low Energy scanner.
    /// </summary>
    /// <typeparam name="TDevice">Platform-specific device type</typeparam>
    /// <typeparam name="TService">Platform-specific service type</typeparam>
    /// <typeparam name="TCharacteristic">Platform-specific characteristic type</typeparam>
    public interface IBleScanner<TDevice, TService, TCharacteristic>
    {
        /// <summary>
        /// Searches for a Bluetooth device by name with default timeout (5 seconds).
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">Thrown if deviceName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Bluetooth scanning is already in progress.</exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        Task<IDevice<TDevice, TService, TCharacteristic>?> FindDeviceAsync(string deviceName);

        /// <summary>
        /// Searches for a Bluetooth device by name with cancellation support.
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <remarks>
        /// This method does NOT have a built-in timeout. 
        /// It runs indefinitely until either a device is found or the cancellation token is cancelled.
        /// For timeout-based scanning, use <see cref="FindDeviceAsync(string, TimeSpan)"/>.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is cancelled via <paramref name="token"/>.
        /// </exception>
        Task<IDevice<TDevice, TService, TCharacteristic>?> FindDeviceAsync(
            string deviceName, TimeSpan timeout);

        /// <summary>
        /// Searches for a Bluetooth device by name with cancellation support.
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">Thrown if deviceName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Bluetooth scanning is already in progress.</exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via the cancellation token.</exception>
        Task<IDevice<TDevice, TService, TCharacteristic>?> FindDeviceAsync(
            string deviceName, CancellationToken token);
    }
}