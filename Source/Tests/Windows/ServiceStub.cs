using BleCommands.Core.Contracts;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests.Windows
{
    public sealed class ServiceStub : IService<GattDeviceService, GattCharacteristic>
    {
        public Guid Id => throw new NotImplementedException();

        public GattDeviceService NativeService => throw new NotImplementedException();

        public Task<ICharacteristic<GattCharacteristic>?> GetCharacteristicAsync(
            Guid id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ICharacteristic<GattCharacteristic>>> GetCharacteristicsAsync(
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
