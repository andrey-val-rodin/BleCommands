namespace Core.Contracts
{
    public interface ICharacteristic
    {
        event EventHandler<ValueUpdatedEventArgs> ValueUpdated;

        Guid Id { get; }

        CharacteristicPropertyFlags Properties { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool CanUpdate { get; }

        Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);

        Task<bool> WriteAsync(byte[] data, CancellationToken cancellationToken = default);

        Task StartUpdatesAsync(CancellationToken cancellationToken = default);

        Task StopUpdatesAsync(CancellationToken cancellationToken = default);
    }
}
