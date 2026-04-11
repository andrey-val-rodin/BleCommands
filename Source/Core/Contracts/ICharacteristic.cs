namespace Core.Contracts
{
    public interface ICharacteristic
    {
        event EventHandler<ValueUpdatedEventArgs> ValueUpdated;

        Guid Id { get; }

        string Uuid { get; }

        string Name { get; }

        string Value { get; }

        CharacteristicPropertyType Properties { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool CanUpdate { get; }

        IService Service { get; }

        Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);

        Task<bool> WriteAsync(byte[] data, CancellationToken cancellationToken = default);

        Task StartUpdatesAsync(CancellationToken cancellationToken = default);

        Task StopUpdatesAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<IDescriptor>> GetDescriptorsAsync(CancellationToken cancellationToken = default);

        Task<IDescriptor> GetDescriptorAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
