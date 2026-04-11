namespace Core.Contracts
{
    public interface IDevice
    {
        string Id { get; }

        string Name { get; }

        bool IsConnected { get; }

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<IService>?> GetServicesAsync(CancellationToken cancellationToken = default);

        Task<IService?> GetServiceAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
