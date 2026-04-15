using BleCommands.Core.Enums;
using BleCommands.Core.Events;

namespace BleCommands.Core.Contracts
{
    public interface ICharacteristic<TCharacteristic>
    {
        event EventHandler<ByteArrayEventArgs> ValueUpdated;

        Guid Id { get; }

        CharacteristicPropertyFlags Properties { get; }

        TCharacteristic NativeCharacteristic { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool CanUpdate { get; }

        BleStream? Stream { get; }

        Task<byte[]> ReadAsync(CancellationToken token = default);

        Task WriteAsync(byte[] data, CancellationToken token = default);

        Task StartUpdatesAsync(CancellationToken token = default);

        Task StopUpdatesAsync(CancellationToken token = default);

        void AttachCommandStream(BleStream stream);

        void DetachCommandStream();
    }
}
