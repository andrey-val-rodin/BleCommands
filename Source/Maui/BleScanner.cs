using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using Microsoft.VisualStudio.Threading;
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
        private const int DefaultTimeoutSeconds = 5;
        private const int MaxTimeoutSeconds = 60;

        private readonly AsyncSemaphore _scanLock = new(1);

        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        /// <summary>
        /// Searches for a Bluetooth device by name with default timeout (5 seconds).
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceName"/> is <c>null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Bluetooth scanning is already in progress.</exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<Device?> FindDeviceAsync(string deviceName)
        {
            return await FindDeviceAsync(deviceName, TimeSpan.FromSeconds(DefaultTimeoutSeconds)).ConfigureAwait(false);
        }

        /// <summary>
        /// Searches for a Bluetooth device by name with the specified timeout.
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <param name="timeout">Timeout.</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceName"/> is <c>null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified timeout is less than or equal to zero,
        /// or greater than 60 seconds.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when Bluetooth scanning is already in progress.</exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
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
            var releaser = await _scanLock.EnterAsync(tokenSource.Token).ConfigureAwait(false);
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
