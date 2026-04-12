using Core.Contracts;
using Core.Exceptions;
using Microsoft.VisualStudio.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace Windows
{
    public class BleScanner : IBleScanner
    {
        private readonly AsyncSemaphore _scanLock = new(1);

        /// <summary>
        /// Searches for a Bluetooth device by name.
        /// </summary>
        /// <param name="deviceName">Name to search for.</param>
        /// <param name="timeout">Maximum time to scan (from 1 second to 1 minute).</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found device or null if timeout expired.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified timeout is out of the range</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when Bluetooth scanning is already in progress.
        /// Use <see cref="Adapter.StopScanningForDevicesAsync"/> to stop existing scan.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors</exception>
        public async Task<IDevice?> FindDeviceAsync(
            string deviceName, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            VerifyTimeout(timeout);

            var releaser = await _scanLock.EnterAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var tcs = new TaskCompletionSource<IDevice?>();

                var deviceWatcher = new BluetoothLEAdvertisementWatcher();

#pragma warning disable IDE0079
#pragma warning disable VSTHRD100 // Avoid async void methods
                // Reason: This handler is called synchronously by BluetoothLEAdvertisementWatcher 
                // in a non-UI thread (no SynchronizationContext). The async void pattern is safe here 
                // because we quickly await the async operation and any exceptions are caught inside.
                // Using async Task would be incompatible with the event signature (expects void).
                async void Handler(object sender, BluetoothLEAdvertisementReceivedEventArgs args)
#pragma warning restore VSTHRD100
#pragma warning restore IDE0079
                {
                    try
                    {
                        using var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                        if (device?.Name == deviceName)
                            tcs.TrySetResult(new Device(device.DeviceId));
                    }
                    catch { /* Just skip device */}
                }

                try
                {
                    deviceWatcher.Received += Handler;
                    deviceWatcher.Start();
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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
                    deviceWatcher.Received -= Handler;
                    deviceWatcher.Stop();
                }
            }
            finally
            {
                releaser.Dispose();
            }
        }

        private static void VerifyTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout too short");
            if (timeout > TimeSpan.FromMinutes(1))
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout too long");
        }
    }
}
