namespace Core.Contracts
{
    public interface IService : IDisposable
    {
        Guid Id { get; }

        Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync();

        Task<ICharacteristic?> GetCharacteristicAsync(Guid id);
    }
}
