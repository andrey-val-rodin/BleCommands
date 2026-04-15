using BleCommands.Core.Contracts;
using INativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;
using INativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace BleCommands.Maui
{
    public class Service : IService<INativeService, INativeCharacteristic>
    {
        private bool _disposed = false;

        public Service(INativeService nativeService)
        {
            NativeService = nativeService;
        }

        public Guid Id => NativeService.Id;

        public INativeService NativeService { get; }

        public Task<ICharacteristic<INativeCharacteristic>?> GetCharacteristicAsync(Guid id)
        {
            ThrowIfDisposed();
            //TODO
            return Task.FromResult<ICharacteristic<INativeCharacteristic>?>(null);
        }

        public Task<IReadOnlyList<ICharacteristic<INativeCharacteristic>>> GetCharacteristicsAsync()
        {
            ThrowIfDisposed();
            //TODO
            return Task.FromResult<IReadOnlyList<ICharacteristic<INativeCharacteristic>>>(new List<ICharacteristic<INativeCharacteristic>>());
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
