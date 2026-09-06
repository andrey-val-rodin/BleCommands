using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BleCommands.Maui
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
        /// Gets reference to <see cref="IAdapter"/>.
        /// </summary>
        public static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        /// <summary>
        /// Occurs when a new Bluetooth Low Energy device is discovered during scanning.
        /// </summary>
        /// <remarks>
        /// This event is raised for each unique device detected by the scanner. 
        /// The <see cref="DeviceEventArgs"/> contains the native device.
        /// </remarks>
        public event EventHandler<DeviceEventArgs>? DeviceDiscovered
        {
            add { Adapter.DeviceDiscovered += value; }
            remove { Adapter.DeviceDiscovered -= value; }
        }

        /// <summary>
        /// Scans for Bluetooth Low Energy devices.
        /// </summary>
        /// <param name="filter">Optional filter to narrow the range of devices to be scanned.</param>
        /// <param name="token">Cancellation token to stop the scanning operation.
        /// If not provided (default), the scan will run indefinitely.</param>
        /// <returns>A task that represents the asynchronous scanning operation.</returns>
        /// <remarks>
        /// <para>
        /// The scanning continues indefinitely until the cancellation token is triggered.
        /// Each time a new unique device is discovered, the <see cref="DeviceDiscovered"/> event is raised.
        /// </para>
        /// <para>
        /// <b>Important:</b> Without a cancellation token, the scan will never stop.
        /// </para>
        /// <para>
        /// <b>Configuration:</b>
        /// Before calling this method, you can configure the scan behavior by setting properties
        /// on <see cref="Adapter"/>:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="IAdapter.ScanMode"/> — Controls the scan mode (LowPower, Balanced, LowLatency).
        ///       Default is <see cref="ScanMode.LowLatency"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <see cref="IAdapter.ScanMatchMode"/> — (Android only) Controls how advertisements are matched.
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// <para>
        /// <b>Usage examples:</b>
        /// <code>
        ///   // Configure scan mode (optional)
        ///   BleScanner.Adapter.ScanMode = ScanMode.LowLatency;
        /// 
        ///   // Scan with timeout (recommended)
        ///   using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        ///   try { await scanner.ScanAsync(token: cts.Token); }
        ///   catch (OperationCanceledException) { /* timeout */ }
        /// 
        ///   // Scan with filter
        ///   var filter = new ScanFilterOptions { DeviceNames = new[] { "MyDevice" } };
        ///   await scanner.ScanAsync(filter: filter, token: cts.Token);
        /// 
        ///   // Scan indefinitely (use with caution)
        ///   await scanner.ScanAsync();
        /// </code>
        /// </para>
        /// </remarks>
        public async Task ScanAsync(
            ScanFilterOptions? filter = null,
            CancellationToken token = default)
        {
            try
            {
                await Adapter.StartScanningForDevicesAsync(
                    scanFilterOptions: filter,
                    cancellationToken: token).ConfigureAwait(false);

                token.ThrowIfCancellationRequested();
            }
            finally
            {
                await Adapter.StopScanningForDevicesAsync().ConfigureAwait(false);
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
            CancellationTokenSource tokenSource)
        {
            try
            {
                var tcs = new TaskCompletionSource<Device?>();

                void Handler(object sender, DeviceEventArgs args)
                {
                    if (args.Device?.Name == deviceName)
                    {
                        if (tcs.TrySetResult(new Device(args.Device)))
                            tokenSource.Cancel();
                    }
                }

                try
                {
                    Adapter.ScanMode = ScanMode.LowLatency;
                    Adapter.DeviceDiscovered += Handler;

                    await Adapter.StartScanningForDevicesAsync(
                        scanFilterOptions: new ScanFilterOptions { DeviceNames = new[] { deviceName } },
                        cancellationToken: tokenSource.Token
                    ).ConfigureAwait(false);

                    using (tokenSource.Token.Register(() => tcs.TrySetCanceled()))
                    {
                        return await tcs.Task.ConfigureAwait(false);
                    }
                }
                finally
                {
                    Adapter.DeviceDiscovered -= Handler;
                    await Adapter.StopScanningForDevicesAsync().ConfigureAwait(false);
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
