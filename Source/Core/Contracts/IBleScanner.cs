namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a Bluetooth Low Energy scanner that searches for a nearby BLE device.
    /// </summary>
    /// <typeparam name="TDevice">
    /// Platform-specific device type (e.g., BluetoothLEDevice on Windows, IDevice on MAUI).
    /// </typeparam>
    /// <typeparam name="TService">
    /// Platform-specific service type (e.g., GattDeviceService on Windows, IService on MAUI).
    /// </typeparam>
    /// <typeparam name="TCharacteristic">
    /// Platform-specific characteristic type (e.g., GattCharacteristic on Windows, ICharacteristic on MAUI).
    /// </typeparam>
    public interface IBleScanner<TDevice, TService, TCharacteristic> : IDisposable
    {
        /// <summary>
        /// Searches for a Bluetooth device by name with the default timeout (5 seconds).
        /// </summary>
        /// <param name="deviceName">The name of the device to search for.</param>
        /// <returns>
        /// The found device, or <c>null</c> if the timeout expires.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceName"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        Task<IDevice<TDevice, TService, TCharacteristic>?> FindDeviceAsync(string deviceName);

        /// <summary>
        /// Searches for a Bluetooth device by name with a specified timeout.
        /// </summary>
        /// <param name="deviceName">The name of the device to search for.</param>
        /// <param name="timeout">Maximum wait time for device discovery.</param>
        /// <returns>
        /// The found device, or <c>null</c> if the timeout expires before the device is discovered.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceName"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified timeout is outside the range of 1 to 60 seconds.
        /// </exception>
        Task<IDevice<TDevice, TService, TCharacteristic>?> FindDeviceAsync(
            string deviceName, TimeSpan timeout);
    }
}
