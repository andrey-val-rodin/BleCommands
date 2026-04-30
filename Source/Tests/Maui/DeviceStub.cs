using BleCommands.Core.Contracts;
using BleCommands.Maui;
using NativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;

namespace BleCommands.Tests.Maui
{
    internal class DeviceStub : IDevice<NativeDevice, Service>
    {
        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public NativeDevice? NativeDevice => throw new NotImplementedException();

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
