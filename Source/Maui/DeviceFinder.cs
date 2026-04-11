using Core.Contracts;
using Core.Exceptions;
using Microsoft.VisualStudio.Threading;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using IDevice = Core.Contracts.IDevice;
using DeviceEventArgs = Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs;

namespace Maui
{
    public class DeviceFinder : IDeviceFinder
    {
        private readonly AsyncSemaphore _scanLock = new(1);

        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        /// <summary>
        /// Searches for a Bluetooth device by name prefix.
        /// </summary>
        /// <param name="deviceName">Name prefix to search for.</param>
        /// <param name="timeout">Maximum time to scan (from 1 second to 1 minute).</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when Bluetooth scanning is already in progress.
        /// Use <see cref="Adapter.StopScanningForDevicesAsync"/> to stop existing scan.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors</exception>
        public async Task<IDevice?> FindDeviceAsync(string deviceName, TimeSpan timeout, CancellationToken token = default)
        {
            VerifyTimeout(timeout);

            var releaser = await _scanLock.EnterAsync(token).ConfigureAwait(false);
            try
            {
                var boundedDevice = GetBoundedDevice(deviceName);
                if (boundedDevice != null)
                    return boundedDevice;

                if (Adapter.IsScanning)
                {
                    throw new InvalidOperationException(
                        "Bluetooth scanning is already in progress. " +
                        "Stop existing scan before starting a new one, or wait for it to complete.");
                }

                var tcs = new TaskCompletionSource<IDevice?>();

                void Handler(object sender, DeviceEventArgs e)
                {
                    if (e.Device?.Name?.StartsWith(deviceName) == true)
                    {
                        tcs.TrySetResult(new Device(e.Device));
                    }
                }

                try
                {
                    Adapter.ScanMode = ScanMode.LowLatency;
                    Adapter.DeviceDiscovered += Handler;

                    await Adapter.StartScanningForDevicesAsync(
                        scanFilterOptions: new ScanFilterOptions { DeviceNames = new[] { deviceName } },
                        cancellationToken: token
                    ).ConfigureAwait(false);

                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    timeoutCts.CancelAfter(timeout);

                    using (timeoutCts.Token.Register(() => tcs.TrySetResult(null)))
                    {
                        return await tcs.Task.ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    throw new DeviceException("BLE Scanning error", ex);
                }
                finally
                {
                    Adapter.DeviceDiscovered -= Handler;
                    if (Adapter.IsScanning)
                        await Adapter.StopScanningForDevicesAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                releaser.Dispose();
            }
        }

        private void VerifyTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout too short");
            if (timeout > TimeSpan.FromMinutes(1))
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout too long");
        }

        private IDevice? GetBoundedDevice(string deviceName)
        {
            // TODO: Do we really need this method? Perhaps we should only return scanned devices...
            // TODO: what about Adapter.BondedDevices?
            var device = Adapter.GetSystemConnectedOrPairedDevices()?
                .FirstOrDefault(d => d.Name?.StartsWith(deviceName) == true);
            return device != null ? new Device(device) : null;
        }
    }
}
