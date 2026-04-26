using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using Microsoft.VisualStudio.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    /// <summary>
    /// Bluetooth Low Energy scanner.
    /// </summary>
    /// <typeparam name="TDevice">Platform-specific device type.</typeparam>
    /// <typeparam name="TService">Platform-specific service type.</typeparam>
    /// <typeparam name="TCharacteristic">Platform-specific characteristic type.</typeparam>
    public class BleScanner : IBleScanner<BluetoothLEDevice, GattDeviceService, GattCharacteristic>
    {
        private const int DefaultTimeoutSeconds = 5;
        private const int MaxTimeoutSeconds = 60;

        private readonly AsyncSemaphore _scanLock = new(1);

        /// <summary>
        /// Searches for a Bluetooth device by name with default timeout (5 seconds).
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">Thrown if deviceName is null or empty.</exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceAsync(
            string deviceName)
        {
            return await FindDeviceAsync(deviceName, TimeSpan.FromSeconds(DefaultTimeoutSeconds)).ConfigureAwait(false);
        }

        /// <summary>
        /// Searches for a Bluetooth device by name with the specified timeout.
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <param name="timeout">Timeout.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">Thrown if deviceName is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified timeout is less than or equal to zero,
        /// or greater than 60 seconds.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceAsync(
            string deviceName, TimeSpan timeout)
        {
            ValidateDeviceName(deviceName);
            ValidateTimeout(timeout);

            using var cts = new CancellationTokenSource(timeout);
            try
            {
                return await FindDeviceInternalAsync(deviceName, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Timeout
                return null;
            }
        }

        private async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceInternalAsync(
            string deviceName,
            CancellationToken token)
        {
            var releaser = await _scanLock.EnterAsync(token).ConfigureAwait(false);
            try
            {
                var tcs = new TaskCompletionSource<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?>();

                var deviceWatcher = new BluetoothLEAdvertisementWatcher
                {
                    ScanningMode = BluetoothLEScanningMode.Active,
                    AdvertisementFilter = new BluetoothLEAdvertisementFilter
                    {
                        Advertisement = new BluetoothLEAdvertisement
                        {
                            LocalName = deviceName
                        }
                    }
                };

                void Handler(object sender, BluetoothLEAdvertisementReceivedEventArgs args)
                {
                    if (args.Advertisement.LocalName == deviceName)
                        tcs.TrySetResult(new Device(args.BluetoothAddress));
                }

                try
                {
                    deviceWatcher.Received += Handler;
                    deviceWatcher.Start();

                    using (token.Register(() => tcs.TrySetCanceled()))
                    {
                        return await tcs.Task.ConfigureAwait(false);
                    }
                }
                finally
                {
                    deviceWatcher.Received -= Handler;
                    deviceWatcher.Stop();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DeviceException("BLE scanning error.", ex);
            }
            finally
            {
                releaser.Dispose();
            }
        }

        private static void ValidateDeviceName(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                throw new ArgumentNullException(nameof(deviceName));
        }

        private static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is less than or equal to zero.");

            if (timeout > TimeSpan.FromSeconds(MaxTimeoutSeconds))
                throw new ArgumentOutOfRangeException(nameof(timeout), $"Timeout too long. Maximum is {MaxTimeoutSeconds} seconds.");
        }

        public void Dispose()
        {
            _scanLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
