using BleCommands.Core.Enums;

namespace BleCommands.Core.Contracts
{
    public interface ICharacteristic<TCharacteristic> : IDisposable
    {
        Guid Id { get; }

        CharacteristicPropertyFlags Properties { get; }

        TCharacteristic NativeCharacteristic { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool CanUpdate { get; }

        TokenAggregator? TokenAggregator { get; }

        Task<string> ReadAsync(CancellationToken token = default);

        Task WriteAsync(string data, CancellationToken token = default);

        Task StartUpdatesAsync(CancellationToken token = default);

        Task StopUpdatesAsync(CancellationToken token = default);

        void AttachTokenAggregator(TokenAggregator tokenAggregator);

        void DetachTokenAggregator();
    }
}
