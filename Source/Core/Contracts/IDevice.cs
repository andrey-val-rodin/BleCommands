namespace Core.Contracts
{
    public interface IDevice : IDisposable
    {
        Guid Id { get; }

        string Name { get; }

        int Rssi { get; }

        Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken cancellationToken = default);

        Task<IService> GetServiceAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> UpdateRssiAsync();

        Task<int> RequestMtuAsync(int requestValue);
    }
}
