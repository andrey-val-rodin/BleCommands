using Core.Contracts;
using ExternalService = Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService;

namespace Windows
{
    public class Service : IService, IDisposable
    {
        private readonly ExternalService _externalService;
        private bool _disposed = false;

        public Service(ExternalService externalService)
        {
            _externalService = externalService;
        }

        public Guid Id => _externalService.Uuid;
        public string Name => _externalService.Uuid.ToString();
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
            ObjectDisposedException.ThrowIf(_disposed, this);
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
