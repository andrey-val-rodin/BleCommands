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

        /// <summary>
        /// Occurs when a new Bluetooth Low Energy device is discovered during scanning.
        /// </summary>
        /// <remarks>
        /// This event is raised for each unique device detected by the scanner. 
        /// The <see cref="DeviceDiscoveredEventArgs"/> contains the Bluetooth address of the device.
        /// </remarks>
        public event EventHandler<DeviceDiscoveredEventArgs>? DeviceDiscovered;

        /// <summary>
        /// Scans for Bluetooth Low Energy devices.
        /// </summary>
        /// <param name="mode">The scanning mode (Active or Passive). 
        /// Active mode provides more data but consumes more power.</param>
        /// <param name="filter">Optional filter to narrow the range of devices to be scanned.</param>
        /// <param name="token">Cancellation token to stop the scanning operation.
        /// If not provided (default), the scan will run indefinitely.</param>
        /// <returns>A task that represents the asynchronous scanning operation.</returns>
        /// <exception cref="DeviceException">
        /// Thrown when an error occurs during the BLE scanning process.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The scanning continues indefinitely until the cancellation token is triggered.
        /// Each time a new unique device is discovered, the <see cref="DeviceDiscovered"/> event is raised.
        /// </para>
        /// <para>
        /// <b>Important:</b> If you call this method without providing a cancellation token,
        /// the scan will never stop. Always either provide a token or call it from a context
        /// where cancellation is managed externally.
        /// </para>
        /// <para>
        /// <b>Usage examples:</b>
        /// <code>
        /// // Scan with timeout (recommended)
        /// using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        /// await scanner.ScanAsync(token: cts.Token);
        /// 
        /// // Scan with filter and timeout
        /// var filter = new BluetoothLEAdvertisementFilter { ... };
        /// await scanner.ScanAsync(
        ///     mode: BluetoothLEScanningMode.Active,
        ///     filter: filter,
        ///     token: cts.Token);
        /// 
        /// // Scan indefinitely (use with caution)
        /// await scanner.ScanAsync();
        /// </code>
        /// </para>
        /// </remarks>
        public async Task ScanAsync(
            BluetoothLEScanningMode mode = BluetoothLEScanningMode.Active,
            BluetoothLEAdvertisementFilter? filter = null,
            CancellationToken token = default)
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
                return await FindDeviceInternalAsync(deviceName, cts).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Timeout
                return null;
            }
        }

        private async Task<Device?> FindDeviceInternalAsync(
            string deviceName,
            CancellationTokenSource cts)
        {
            Device? device = null;
            void Handler(object? sender, DeviceDiscoveredEventArgs e)
            {
                device =  new Device(e.BluetoothAddress);
                cts.Cancel();
            }

            try
            {
                var filter = new BluetoothLEAdvertisementFilter
                {
                    Advertisement = new BluetoothLEAdvertisement
                    {
                        LocalName = deviceName
                    }
                };

                DeviceDiscovered += Handler;
                await ScanAsync(BluetoothLEScanningMode.Active, filter, cts.Token);
                
                return device;
            }
            finally
            {
                DeviceDiscovered -= Handler;
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
