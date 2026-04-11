using Core.Contracts;
using Core.Exceptions;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using ExternalDevice = Windows.Devices.Bluetooth.BluetoothLEDevice;

namespace Windows
{
    public class Device : IDevice, IDisposable
    {
        private bool _disposed = false;
        private GattSession? _gattSession;

        public Device(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; private set; }
        public string Name => ExternalDevice?.Name ?? string.Empty;
        public bool IsConnected { get; private set; }
        private ExternalDevice? ExternalDevice { get; set; }

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
                ExternalDevice = await BluetoothLEDevice.FromIdAsync(Id);
                if (ExternalDevice == null)
                    return false;

                // Create and configure GATT session to maintain connection
                _gattSession = await GattSession.FromDeviceIdAsync(ExternalDevice.BluetoothDeviceId);
                _gattSession.MaintainConnection = true;

                // Monitor connection status
                ExternalDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

                // Retrieve services to establish actual connection
                var services = await ExternalDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                IsConnected = ExternalDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
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

        public async Task<IService?> GetServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (!IsConnected || ExternalDevice == null)
                throw new InvalidOperationException("Device not connected");

            try
            {
                var result = await ExternalDevice.GetGattServicesForUuidAsync(id, BluetoothCacheMode.Cached);
                var externalService = result.Services.Count > 0 ? result.Services[0] : null;
                if (externalService == null)
                    return null;

                return new Service(externalService);
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get service", ex);
            }
        }

        public async Task<IReadOnlyList<IService>?> GetServicesAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (!IsConnected || ExternalDevice == null)
                throw new InvalidOperationException("Device not connected");

            try
            {
                var result = await ExternalDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                var externalServices = result?.Services;
                if (externalServices == null)
                    return null;

                return externalServices.Select(s => new Service(s)).ToList();
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
                    if (ExternalDevice != null)
                    {
                        ExternalDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
                        ExternalDevice.Dispose();
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