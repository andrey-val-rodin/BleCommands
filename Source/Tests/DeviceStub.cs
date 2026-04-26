using BleCommands.Core.Contracts;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests
{
    public sealed class DeviceStub : IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>
    {
        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public BluetoothLEDevice? NativeDevice => throw new NotImplementedException();

        public event EventHandler? Disconnected { add { } remove { } }

        public Task<bool> ConnectAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IService<GattDeviceService, GattCharacteristic>?> GetServiceAsync(Guid id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IService<GattDeviceService, GattCharacteristic>>> GetServicesAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
