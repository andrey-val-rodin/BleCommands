using Core.Contracts;
using ExternalDevice = Plugin.BLE.Abstractions.Contracts.IDevice;

namespace Maui.Adapters
{
    public class DeviceAdapter : IDevice
    {
        private readonly ExternalDevice _externalDevice;
        private bool _disposed = false;

        public DeviceAdapter(ExternalDevice externalDevice)
        {
            _externalDevice = externalDevice ?? throw new ArgumentNullException(nameof(externalDevice));
        }

        public Guid Id => _externalDevice.Id;
        public string Name => _externalDevice.Name;
        public int Rssi => _externalDevice.Rssi;
        internal ExternalDevice ExternalDevice => _externalDevice;

        public async Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var externalServices = await _externalDevice.GetServicesAsync(cancellationToken);
            return externalServices.Select(s => new ServiceAdapter(s)).ToList();
        }

        public async Task<IService> GetServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var externalService = await _externalDevice.GetServiceAsync(id, cancellationToken);
            return new ServiceAdapter(externalService);
        }

        public async Task<bool> UpdateRssiAsync()
        {
            return await _externalDevice.UpdateRssiAsync();
        }

        public async Task<int> RequestMtuAsync(int requestValue)
        {
            return await _externalDevice.RequestMtuAsync(requestValue);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DeviceAdapter));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _externalDevice.Dispose();
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