namespace Core.Contracts
{
    public interface IScanner
    {
        event EventHandler<DeviceEventArgs> DeviceDiscovered;

        TimeSpan SkanTimeout { get; set; }
        ScanMode ScanMode { get; set; }

        Task<bool> StartScanningAsync(CancellationToken token = default);
        Task StopScanningAsync();
    }
}
