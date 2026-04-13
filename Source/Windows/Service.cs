using Core.Contracts;
using Core.Exceptions;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Windows
{
    public class Service : IService
    {
        private bool _disposed = false;

        public Service(GattDeviceService externalService)
        {
            ExternalService = externalService ?? throw new ArgumentNullException(nameof(externalService));
        }

        public Guid Id => ExternalService.Uuid;
        private GattDeviceService ExternalService { get; }

        public async Task<ICharacteristic?> GetCharacteristicAsync(Guid id)
        {
            ThrowIfDisposed();

            try
            {
                var result = await ExternalService.GetCharacteristicsForUuidAsync(id);
                var externalService = result.Characteristics.Count > 0 ? result.Characteristics[0] : null;

                return externalService == null ? null : new Characteristic(externalService);
            }
            catch (Exception ex)
            {
                throw new DeviceException("GetCharacteristicAsync() failed", ex);
            }
        }

        public async Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync()
        {
            ThrowIfDisposed();

            try
            {
                var result = await ExternalService.GetCharacteristicsAsync();
                var externalCharacteristics = result?.Characteristics;
                return externalCharacteristics == null
                    ? new List<ICharacteristic>()
                    : externalCharacteristics.Select(c => new Characteristic(c)).ToList<ICharacteristic>();
            }
            catch (Exception ex)
            {
                throw new DeviceException("GetCharacteristicsAsync() failed", ex);
            }
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
