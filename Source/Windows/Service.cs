using Core.Contracts;
using Core.Exceptions;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Extensions;

namespace Windows
{
    public class Service : IService<GattDeviceService, GattCharacteristic>
    {
        private bool _disposed = false;

        public Service(GattDeviceService nativeService)
        {
            NativeService = nativeService ?? throw new ArgumentNullException(nameof(nativeService));
        }

        public Guid Id => NativeService.Uuid;

        public GattDeviceService NativeService { get; }

        public async Task<ICharacteristic<GattCharacteristic>?> GetCharacteristicAsync(Guid id)
        {
            ThrowIfDisposed();

            try
            {
                var result = await NativeService.GetCharacteristicsForUuidAsync(id);
                result.ThrowIfError();
                var nativeService = result.Characteristics.Count > 0 ? result.Characteristics[0] : null;

                return nativeService == null ? null : new Characteristic(nativeService);
            }
            catch (Exception ex)
            {
                throw new DeviceException("GetCharacteristicAsync() failed.", ex);
            }
        }

        public async Task<IReadOnlyList<ICharacteristic<GattCharacteristic>>> GetCharacteristicsAsync()
        {
            ThrowIfDisposed();

            try
            {
                var result = await NativeService.GetCharacteristicsAsync();
                result.ThrowIfError();
                var nativeCharacteristics = result.Characteristics;

                return nativeCharacteristics == null
                    ? new List<ICharacteristic<GattCharacteristic>>()
                    : nativeCharacteristics.Select(c => new Characteristic(c)).ToList<ICharacteristic<GattCharacteristic>>();
            }
            catch (Exception ex)
            {
                throw new DeviceException("GetCharacteristicsAsync() failed.", ex);
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
                    NativeService.Dispose();
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
