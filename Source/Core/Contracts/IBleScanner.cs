namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a Bluetooth Low Energy scanner that searches for a nearby BLE device.
    /// </summary>
    /// <typeparam name="TDevice">
    /// A specific device implementation.
    /// </typeparam>
    public interface IBleScanner<TDevice> : IDisposable
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
        Task<TDevice?> FindDeviceAsync(
            string deviceName, TimeSpan timeout);
    }
}
