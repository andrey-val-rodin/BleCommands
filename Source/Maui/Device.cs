using Core.Exceptions;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using IDevice = Core.Contracts.IDevice<
    Plugin.BLE.Abstractions.Contracts.IDevice,
    Plugin.BLE.Abstractions.Contracts.IService,
    Plugin.BLE.Abstractions.Contracts.ICharacteristic>;
using INativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using IService = Core.Contracts.IService<
    Plugin.BLE.Abstractions.Contracts.IService,
    Plugin.BLE.Abstractions.Contracts.ICharacteristic>;

namespace Maui
{
    public class Device : IDevice
    {
        private INativeDevice? _nativeDevice;
        private readonly string? _id;
        private bool _disposed = false;

        public Device(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (Guid.TryParse(id, out Guid _))
                throw new ArgumentException("The specified id is not Guid");

            _id = id;
        }

        internal Device(INativeDevice nativeDevice)
        {
            _nativeDevice = nativeDevice ?? throw new ArgumentNullException(nameof(nativeDevice));
        }

        public string Id => _id ?? _nativeDevice?.Id.ToString() ?? string.Empty;

        public string Name => _nativeDevice?.Name ?? string.Empty;

        /// <summary>
        /// Gets platform-specific device
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessed before calling <see cref="ConnectAsync"/>.
        /// </exception>
        public INativeDevice NativeDevice => _nativeDevice ?? throw new InvalidOperationException("Native device is not ready. Call ConnectAsync()");

        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_nativeDevice != null)
            {
                IsConnected = await ConnectAsync(_nativeDevice, cancellationToken).ConfigureAwait(false);
            }
            else if (_id != null)
            {
                IsConnected = await ConnectAsync(_id, cancellationToken).ConfigureAwait(false);
            }

            return IsConnected;
        }

        private static async Task<bool> ConnectAsync(
            INativeDevice nativeDevice, CancellationToken cancellationToken = default)
        {
            var parameters = new ConnectParameters(false, forceBleTransport: true);
            try
            {
                await Adapter.ConnectToDeviceAsync(nativeDevice, parameters, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error", ex);
            }
        }

        private async Task<bool> ConnectAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                Guid guid = Guid.Parse(id);
                _nativeDevice = await Adapter.ConnectToKnownDeviceAsync(guid,
                    new ConnectParameters(false, forceBleTransport: true), cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error", ex);
            }
        }

        public async Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (!IsConnected || _nativeDevice == null)
                throw new InvalidOperationException("Device not connected");

            var nativeServices = await _nativeDevice.GetServicesAsync(cancellationToken);
            return nativeServices == null
                ? new List<IService>()
                : nativeServices.Select(s => new Service(s)).ToList<IService>();
        }

        public async Task<IService?> GetServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (!IsConnected || _nativeDevice == null)
                throw new InvalidOperationException("Device not connected");

            var nativeService = await _nativeDevice.GetServiceAsync(id, cancellationToken);
            return nativeService == null ? null : new Service(nativeService);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Device));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _nativeDevice?.Dispose();
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