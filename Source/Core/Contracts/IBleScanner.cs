namespace Core.Contracts
{
    public interface IBleScanner
    {
        Task<IDevice?> FindDeviceAsync(string deviceName, TimeSpan timeout, CancellationToken token = default);
    }
}
