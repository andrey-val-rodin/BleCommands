using BleCommands.Core.Contracts;
using BleCommands.Windows;
using Windows.Devices.Bluetooth;

namespace BleCommands.Tests.Windows
{
    internal class DeviceStub : IDevice<BluetoothLEDevice, Service>
    {
        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public BluetoothLEDevice? NativeDevice => null!;

        public event EventHandler? Disconnected { add { } remove { } }

        public Task ConnectAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<Service?> GetServiceAsync(Guid id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Service>> GetServicesAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
