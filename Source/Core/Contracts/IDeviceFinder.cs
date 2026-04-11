namespace Core.Contracts
{
    public interface IDeviceFinder
    {
        Task<IDevice?> FindDeviceAsync(string deviceName, TimeSpan timeout, CancellationToken token = default);
    }
}
