using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using Microsoft.VisualStudio.Threading;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using INativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;
using INativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using INativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace BleCommands.Maui
{
    public class BleScanner : IBleScanner<INativeDevice, INativeService, INativeCharacteristic>
    {
        private const int DefaultTimeoutSeconds = 5;
        private const int MaxTimeoutSeconds = 60;
        private const int MinTimeoutSeconds = 1;

        private readonly AsyncSemaphore _scanLock = new(1);

        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        /// <inheritdoc />
        public async Task<IDevice<INativeDevice, INativeService, INativeCharacteristic>?> FindDeviceAsync(
            string deviceName)
        {
            return await FindDeviceAsync(deviceName, TimeSpan.FromSeconds(DefaultTimeoutSeconds)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IDevice<INativeDevice, INativeService, INativeCharacteristic>?> FindDeviceAsync(
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
                // Таймаут — возвращаем null
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IDevice<INativeDevice, INativeService, INativeCharacteristic>?> FindDeviceAsync(
            string deviceName, CancellationToken token)
        {
            ValidateDeviceName(deviceName);
            return await FindDeviceInternalAsync(deviceName, token).ConfigureAwait(false);
        }

        private async Task<IDevice<INativeDevice, INativeService, INativeCharacteristic>?> FindDeviceInternalAsync(
            string deviceName,
            CancellationToken token)
        {
            var releaser = await _scanLock.EnterAsync(token).ConfigureAwait(false);
            try
            {
                var tcs = new TaskCompletionSource<IDevice<INativeDevice, INativeService, INativeCharacteristic>?>();

                void Handler(object sender, DeviceEventArgs args)
                {
                    if (args.Device?.Name == deviceName)
                    {
                        tcs.TrySetResult(new Device(args.Device));
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

                    using (token.Register(() => tcs.TrySetException(new OperationCanceledException(token))))
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
                throw new ArgumentOutOfRangeException(nameof(timeout), $"Timeout too short. Minimum is {MinTimeoutSeconds} second.");

            if (timeout > TimeSpan.FromSeconds(MaxTimeoutSeconds))
                throw new ArgumentOutOfRangeException(nameof(timeout), $"Timeout too long. Maximum is {MaxTimeoutSeconds} seconds.");
        }
    }
}
