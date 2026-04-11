namespace Core.Contracts
{
    public interface IService : IDisposable
    {
        Guid Id { get; }

        string Name { get; }

        Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync();

        Task<ICharacteristic> GetCharacteristicAsync(Guid id);
    }
}
