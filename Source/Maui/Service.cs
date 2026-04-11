using Core.Contracts;
using ExternalService = Plugin.BLE.Abstractions.Contracts.IService;

namespace Maui
{
    public class Service : IService, IDisposable
    {
        private readonly ExternalService _externalService;
        private bool _disposed = false;

        public Service(ExternalService externalService)
        {
            _externalService = externalService;
        }

        public Guid Id => _externalService.Id;
        public string Name => _externalService.Name;
        internal ExternalService ExternalService => _externalService;

        public Task<ICharacteristic?> GetCharacteristicAsync(Guid id)
        {
            ThrowIfDisposed();
            //TODO
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
#pragma warning disable VSTHRD114 // Avoid returning a null Task
            return null;
#pragma warning restore VSTHRD114 // Avoid returning a null Task
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        public Task<IReadOnlyList<ICharacteristic>?> GetCharacteristicsAsync()
        {
            ThrowIfDisposed();
            //TODO
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
#pragma warning disable VSTHRD114 // Avoid returning a null Task
            return null;
#pragma warning restore VSTHRD114 // Avoid returning a null Task
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
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
