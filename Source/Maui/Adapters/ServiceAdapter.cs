using Core.Contracts;
using ExternalService = Plugin.BLE.Abstractions.Contracts.IService;

namespace Maui.Adapters
{
    public class ServiceAdapter : IService
    {
        private readonly ExternalService _externalService;
        private bool _disposed = false;

        public ServiceAdapter(ExternalService externalService)
        {
            _externalService = externalService;
        }

        public Guid Id => _externalService.Id;
        public string Name => _externalService.Name;
        internal ExternalService ExternalService => _externalService;

        public Task<ICharacteristic> GetCharacteristicAsync(Guid id)
        {
            ThrowIfDisposed();
            //TODO
            return null;
        }

        public Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync()
        {
            ThrowIfDisposed();
            //TODO
            return null;
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
                    _externalService.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
