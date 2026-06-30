using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using Windows.Devices.Bluetooth.Advertisement;

namespace BleCommands.Windows
{
    /// <summary>
    /// Bluetooth Low Energy scanner.
    /// </summary>
    public class BleScanner : IBleScanner<Device>
    {
        /// <summary>
        /// Default timeout for device search.
        /// </summary>
        public const int DefaultTimeoutSeconds = 5;
        /// <summary>
        /// Maximum timeout for device search.
        /// </summary>
        public const int MaxTimeoutSeconds = 60;

        /// <summary>
        /// Searches for a Bluetooth device by name with default timeout (5 seconds).
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <returns>Found device or <c>null</c> if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceName"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<Device?> FindDeviceAsync(string deviceName)
        {
            return await FindDeviceAsync(deviceName,
                TimeSpan.FromSeconds(DefaultTimeoutSeconds)).ConfigureAwait(false);
        }

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
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<Device?> FindDeviceAsync(string deviceName, TimeSpan timeout)
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

        private async Task<Device?> FindDeviceInternalAsync(
            string deviceName,
            CancellationToken token)
        {
            try
            {
                var tcs = new TaskCompletionSource<Device?>();

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
        }

        private static void ValidateDeviceName(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                throw new ArgumentNullException(nameof(deviceName));
        }

        private static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout),
                    "Timeout is less than or equal to zero.");

            if (timeout > TimeSpan.FromSeconds(MaxTimeoutSeconds))
                throw new ArgumentOutOfRangeException(nameof(timeout),
                    $"Timeout too long. Maximum is {MaxTimeoutSeconds} seconds.");
        }
    }
}
