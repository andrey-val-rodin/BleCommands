using Core.Contracts;
using Core.Exceptions;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Windows
{
    public class Device : IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>
    {
        private BluetoothLEDevice? _nativeDevice;
        private GattSession? _gattSession;
        private bool _disposed = false;

        public Device(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; private set; }

        public string Name => NativeDevice?.Name ?? string.Empty;

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets platform-specific device
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessed before calling <see cref="ConnectAsync"/>.
        /// </exception>
        public BluetoothLEDevice NativeDevice => _nativeDevice ?? throw new InvalidOperationException("Native device is not ready. Call ConnectAsync()");

        /// <summary>
        /// Connects to the device by the specified id.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if device is successfully connected; false otherwise.</returns>
        /// <remarks>This method must be called from a UI thread.</remarks>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            try
            {
                _nativeDevice = await BluetoothLEDevice.FromIdAsync(Id);
                if (_nativeDevice == null)
                    return false;

                // Create and configure GATT session to maintain connection
                _gattSession = await GattSession.FromDeviceIdAsync(NativeDevice.BluetoothDeviceId);
                _gattSession.MaintainConnection = true;

                // Monitor connection status
                NativeDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

                // Retrieve services to establish actual connection
                var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (result?.Status != GattCommunicationStatus.Success)
                    return false;

                IsConnected = NativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                return IsConnected;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error", ex);
            }
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            IsConnected = sender.ConnectionStatus == BluetoothConnectionStatus.Connected;
        }

        public async Task<IService<GattDeviceService, GattCharacteristic>?> GetServiceAsync(
            Guid id, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (!IsConnected || NativeDevice == null)
                throw new InvalidOperationException("Device not connected");

            try
            {
                var result = await NativeDevice.GetGattServicesForUuidAsync(id, BluetoothCacheMode.Cached);
                var nativeService = result?.Services?.Count > 0 ? result.Services[0] : null;

                return nativeService == null ? null : new Service(nativeService);
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get service", ex);
            }
        }

        public async Task<IReadOnlyList<IService<GattDeviceService, GattCharacteristic>>> GetServicesAsync(
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (!IsConnected || NativeDevice == null)
                throw new InvalidOperationException("Device not connected");

            try
            {
                var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                var nativeServices = result?.Services;
                return nativeServices == null
                    ? new List<IService<GattDeviceService, GattCharacteristic>>()
                    : nativeServices.Select(s => new Service(s)).ToList();
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get services", ex);
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