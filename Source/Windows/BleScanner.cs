using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using BleCommands.Windows.Events;
using System.Collections.Concurrent;
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

        public event EventHandler<DeviceDiscoveredEventArgs>? DeviceDiscovered;

        public async Task ScanAsync(
            CancellationToken token,
            BluetoothLEScanningMode mode = BluetoothLEScanningMode.Active,
            BluetoothLEAdvertisementFilter? filter = null)
        {
            try
            {
                var tcs = new TaskCompletionSource();

                var deviceWatcher = new BluetoothLEAdvertisementWatcher
                {
                    ScanningMode = mode
                };
                if (filter != null)
                    deviceWatcher.AdvertisementFilter = filter;

                ConcurrentDictionary<ulong, byte> deviceRegistry = new();

                void Handler(object sender, BluetoothLEAdvertisementReceivedEventArgs args)
                {
                    if (deviceRegistry.TryAdd(args.BluetoothAddress, 0))
                    {
                        DeviceDiscovered?.Invoke(
                            this,
                            new DeviceDiscoveredEventArgs(args.BluetoothAddress));
                    }
                }

                try
                {
                    deviceWatcher.Received += Handler;
                    deviceWatcher.Start();

                    using (token.Register(() => tcs.TrySetCanceled()))
                    {
                        await tcs.Task.ConfigureAwait(false);
                        return;
                    }
                }
                finally
                {
                    deviceWatcher.Received -= Handler;
                    deviceWatcher.Stop();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DeviceException("BLE scanning error.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Device?> FindDeviceAsync(string deviceName)
        {
            return await FindDeviceAsync(deviceName,
                TimeSpan.FromSeconds(DefaultTimeoutSeconds)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
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
