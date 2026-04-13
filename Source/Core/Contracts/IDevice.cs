namespace Core.Contracts
{
    public interface IDevice<TDevice, TService, TCharacteristic> : IDisposable
    {
        string Id { get; }

        string Name { get; }

        bool IsConnected { get; }

        TDevice NativeDevice { get; }

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<IService<TService, TCharacteristic>>> GetServicesAsync(CancellationToken cancellationToken = default);

        Task<IService<TService, TCharacteristic>?> GetServiceAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
