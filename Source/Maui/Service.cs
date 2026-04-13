using Core.Contracts;
using ExternalService = Plugin.BLE.Abstractions.Contracts.IService;

namespace Maui
{
    public class Service : IService
    {
        private bool _disposed = false;

        public Service(ExternalService externalService)
        {
            ExternalService = externalService;
        }

        public Guid Id => ExternalService.Id;
        internal ExternalService ExternalService { get; }

        public Task<ICharacteristic?> GetCharacteristicAsync(Guid id)
        {
            ThrowIfDisposed();
            //TODO
            return Task.FromResult<ICharacteristic?>(null);
        }

        public Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync()
        {
            ThrowIfDisposed();
            //TODO
            return Task.FromResult<IReadOnlyList<ICharacteristic>>(null);
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
                    ExternalService.Dispose();
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
