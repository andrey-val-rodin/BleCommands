namespace BleCommands.Core.Contracts
{
    public interface IService<TService, TCharacteristic> : IDisposable
    {
        Guid Id { get; }

        TService NativeService { get; }

        Task<IReadOnlyList<ICharacteristic<TCharacteristic>>> GetCharacteristicsAsync();

        Task<ICharacteristic<TCharacteristic>?> GetCharacteristicAsync(Guid id);
    }
}
