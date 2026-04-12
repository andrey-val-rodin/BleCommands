using Windows;

namespace Tests.Windows
{
    public class BleScannerTests
    {
        [Fact]
        public async Task FindDeviceAsync_Found()
        {
            var finder = new BleScanner();
            using var cts = new CancellationTokenSource();
            var result = await finder.FindDeviceAsync("Rotating Table", TimeSpan.FromSeconds(5), cts.Token);

            Assert.NotNull(result);
            await result.ConnectAsync(cts.Token);
            Assert.True(result.IsConnected);
        }
    }
}
