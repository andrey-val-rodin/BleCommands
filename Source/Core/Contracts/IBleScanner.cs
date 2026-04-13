namespace Core.Contracts
{
    public interface IBleScanner<TDevice, TService, TCharacteristic>
    {
        Task<IDevice<TDevice, TService, TCharacteristic>?> FindDeviceAsync(
            string deviceName, TimeSpan timeout, CancellationToken token = default);
    }
}
