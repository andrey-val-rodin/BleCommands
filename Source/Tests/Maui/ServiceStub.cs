using BleCommands.Core.Contracts;
using BleCommands.Maui;
using NativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace BleCommands.Tests.Maui
{
    internal class ServiceStub : IService<NativeService, Characteristic>
    {
        public bool Disposed { get; private set; }

        public Guid Id => throw new NotImplementedException();

        public NativeService NativeService => null!;

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
