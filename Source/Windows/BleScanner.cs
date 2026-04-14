using Core.Contracts;
using Core.Exceptions;
using Microsoft.VisualStudio.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Windows
{
    public class BleScanner : IBleScanner<BluetoothLEDevice, GattDeviceService, GattCharacteristic>
    {
        private readonly AsyncSemaphore _scanLock = new(1);
        private const int DefaultTimeoutSeconds = 5;
        private const int MaxTimeoutSeconds = 60;
        private const int MinTimeoutSeconds = 1;

        /// <inheritdoc />
        public async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceAsync(
            string deviceName)
        {
            return await FindDeviceAsync(deviceName, TimeSpan.FromSeconds(DefaultTimeoutSeconds)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceAsync(
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
        public async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceAsync(
            string deviceName, CancellationToken cancellationToken)
        {
            ValidateDeviceName(deviceName);
            return await FindDeviceInternalAsync(deviceName, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?> FindDeviceInternalAsync(
            string deviceName,
            CancellationToken cancellationToken)
        {
            var releaser = await _scanLock.EnterAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var tcs = new TaskCompletionSource<IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>?>();

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

                    using (cancellationToken.Register(() => tcs.TrySetException(new OperationCanceledException(cancellationToken))))
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
                throw new DeviceException("BLE scanning error", ex);
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
