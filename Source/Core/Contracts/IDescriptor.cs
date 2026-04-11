namespace Core.Contracts
{
    // TODO: remove this interface?
    public interface IDescriptor
    {
        Guid Id { get; }

        string Name { get; }

        byte[] Value { get; }

        ICharacteristic Characteristic { get; }

        Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);

        Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
    }
}
