using BleCommands.Core.Contracts;
using BleCommands.Maui;
using NativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace BleCommands.Tests.Maui
{
    internal class ServiceStub : IService<NativeService, Characteristic>
    {
        public Guid Id => throw new NotImplementedException();

        public NativeService NativeService => null!;

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
