using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using BleCommands.Windows.Extensions;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    public class Device : IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>
    {
        private readonly ulong _bluetoothAddress;
        private GattSession? _gattSession;
        private bool _disposed = false;

        public Device(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public Device(ulong bluetoothAddress)
        {
            Id = string.Empty;
            _bluetoothAddress = bluetoothAddress;
        }

        public string Id { get; private set; }

        public string Name => NativeDevice?.Name ?? string.Empty;

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets platform-specific device
        /// </summary>
        public BluetoothLEDevice? NativeDevice { get; private set; }

        /// <summary>
        /// Connects to the device by the specified id.
        /// </summary>
        /// <param name="token">Token to cancel the operation.</param>
        /// <returns>True if device is successfully connected; false otherwise.</returns>
        /// <remarks>This method must be called from a UI thread.</remarks>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<bool> ConnectAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            try
            {
                NativeDevice = await GetNativeDeviceAsync();
                if (NativeDevice == null)
                    return false;

                // Create and configure GATT session to maintain connection
                _gattSession = await GattSession.FromDeviceIdAsync(NativeDevice.BluetoothDeviceId);
                _gattSession.MaintainConnection = true;

                // Monitor connection status
                NativeDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

                // Retrieve services to establish actual connection
                var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                result.ThrowIfError();

                IsConnected = NativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                return IsConnected;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error.", ex);
            }
        }

        private async Task<BluetoothLEDevice> GetNativeDeviceAsync()
        {
            return string.IsNullOrEmpty(Id)
                ? await BluetoothLEDevice.FromBluetoothAddressAsync(_bluetoothAddress)
                : await BluetoothLEDevice.FromIdAsync(Id);
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            IsConnected = sender.ConnectionStatus == BluetoothConnectionStatus.Connected;
        }

        public async Task<IService<GattDeviceService, GattCharacteristic>?> GetServiceAsync(
            Guid id, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            try
            {
                var result = await NativeDevice.GetGattServicesForUuidAsync(id, BluetoothCacheMode.Cached);
                result.ThrowIfError();
                var nativeService = result.Services?.Count > 0 ? result.Services[0] : null;

                return nativeService == null ? null : new Service(nativeService);
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get service.", ex);
            }
        }

        public async Task<IReadOnlyList<IService<GattDeviceService, GattCharacteristic>>> GetServicesAsync(
            CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            try
            {
                var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                result.ThrowIfError();
                var nativeServices = result.Services;

                return nativeServices == null
                    ? new List<IService<GattDeviceService, GattCharacteristic>>()
                    : nativeServices.Select(s => new Service(s)).ToList();
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get services.", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (NativeDevice != null)
                    {
                        NativeDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
                        NativeDevice.Dispose();
                    }

                    _gattSession?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}