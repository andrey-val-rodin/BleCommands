using Core.Exceptions;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using ExternalDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using IDevice = Core.Contracts.IDevice;
using IService = Core.Contracts.IService;

namespace Maui
{
    public class Device : IDevice
    {
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

        internal Device(ExternalDevice externalDevice)
        {
            ExternalDevice = externalDevice ?? throw new ArgumentNullException(nameof(externalDevice));
        }

        public string Id => _id ?? ExternalDevice?.Id.ToString() ?? string.Empty;
        public string Name => ExternalDevice?.Name ?? string.Empty;
        internal ExternalDevice? ExternalDevice { get; }
        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (ExternalDevice != null)
            {
                IsConnected = await ConnectAsync(ExternalDevice, cancellationToken).ConfigureAwait(false);
            }
            else if (_id != null)
            {
                IsConnected = await ConnectAsync(_id, cancellationToken).ConfigureAwait(false);
            }

            return IsConnected;
        }

        private static async Task<bool> ConnectAsync(
            ExternalDevice externalDevice, CancellationToken cancellationToken = default)
        {
            var parameters = new ConnectParameters(false, forceBleTransport: true);
            try
            {
                await Adapter.ConnectToDeviceAsync(externalDevice, parameters, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error", ex);
            }
        }

        private static async Task<bool> ConnectAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                Guid guid = Guid.Parse(id);
                await Adapter.ConnectToKnownDeviceAsync(guid,
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
            if (!IsConnected || ExternalDevice == null)
                throw new InvalidOperationException("Device not connected");

            var externalServices = await ExternalDevice.GetServicesAsync(cancellationToken);
            return externalServices == null
                ? new List<IService>()
                : externalServices.Select(s => new Service(s)).ToList<IService>();
        }

        public async Task<IService?> GetServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (!IsConnected || ExternalDevice == null)
                throw new InvalidOperationException("Device not connected");

            var externalService = await ExternalDevice.GetServiceAsync(id, cancellationToken);
            return externalService == null ? null : new Service(externalService);
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
                    ExternalDevice?.Dispose();
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