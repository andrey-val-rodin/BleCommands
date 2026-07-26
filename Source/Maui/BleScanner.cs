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
        protected static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

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
                    if (Adapter.IsScanning)
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
