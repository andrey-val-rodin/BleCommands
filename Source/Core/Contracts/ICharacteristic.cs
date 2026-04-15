namespace Core.Contracts
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

        CommandStream? CommandStream { get; }

        Task<byte[]> ReadAsync(CancellationToken token = default);

        Task WriteAsync(byte[] data, CancellationToken token = default);

        Task StartUpdatesAsync(CancellationToken token = default);

        Task StopUpdatesAsync(CancellationToken token = default);

        void AttachCommandStream(CommandStream stream);

        void DetachCommandStream();
    }
}
