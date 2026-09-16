using BleCommands.Core.Contracts;
using BleCommands.Windows;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests.Windows
{
    internal class ServiceStub : IService<GattDeviceService, Characteristic>
    {
        public bool Disposed { get; private set; }

        public Guid Id => throw new NotImplementedException();

        public GattDeviceService NativeService => null!;

        public Task<Characteristic?> GetCharacteristicAsync(
            Guid id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult<Characteristic?>(null!);
        }

        public Task<IReadOnlyList<Characteristic>> GetCharacteristicsAsync(
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<Characteristic>>([]);
        }

        public void RegisterChild(IDisposable child)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
