using Core.Contracts;
using Core.Exceptions;
using Microsoft.VisualStudio.Threading;
using Windows.Devices.Enumeration;

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
        /// <remarks>The method may return a cached device even after the device becomes unavailable.</remarks>
        public async Task<IDevice?> FindDeviceAsync(
            string deviceName, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            VerifyTimeout(timeout);

            var releaser = await _scanLock.EnterAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var tcs = new TaskCompletionSource<IDevice?>();
                string aqsFilter = $"System.ItemNameDisplay:=\"{deviceName}\"";
                var deviceWatcher = DeviceInformation.CreateWatcher(
                    aqsFilter,
                    null,
                    DeviceInformationKind.AssociationEndpoint);

                void AddedHandler(DeviceWatcher sender, DeviceInformation deviceInfo)
                {
                    if (deviceInfo?.Name != deviceName)
                        return;

                    tcs.TrySetResult(new Device(deviceInfo.Id));
                }
                void DummyHandler(DeviceWatcher sender, DeviceInformationUpdate args) { }

                // Register event handlers before starting the watcher. We must subscribe to all events.
                deviceWatcher.Added += AddedHandler;
                deviceWatcher.Updated += DummyHandler;
                deviceWatcher.Removed += DummyHandler;

                try
                {
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
                    deviceWatcher.Added -= AddedHandler;
                    deviceWatcher.Updated -= DummyHandler;
                    deviceWatcher.Removed -= DummyHandler;
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
