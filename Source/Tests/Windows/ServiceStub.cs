using BleCommands.Core.Contracts;
using BleCommands.Windows;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests.Windows
{
    internal class ServiceStub : IService<GattDeviceService, Characteristic>
    {
        public Guid Id => throw new NotImplementedException();

        public GattDeviceService NativeService => throw new NotImplementedException();

        public Task<Characteristic?> GetCharacteristicAsync(
            Guid id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Characteristic>> GetCharacteristicsAsync(
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
