using Windows;

namespace Tests.Windows
{
    public class BleScannerTests
    {
        /// <summary>
        /// Temporary test for Windows development
        /// </summary>
        [Fact]
        public async Task FindDeviceAsync_Found()
        {
            var finder = new BleScanner();
            using var cts = new CancellationTokenSource();
            using var device = await finder.FindDeviceAsync("Rotating Table", TimeSpan.FromSeconds(5), cts.Token);

            Assert.NotNull(device);
            await device.ConnectAsync(cts.Token);
            Assert.True(device.IsConnected);
            var uuid = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
            using var service = await device.GetServiceAsync(uuid, cts.Token);
            Assert.NotNull(service);
            var characteristics = await service.GetCharacteristicsAsync();
            Assert.Equal(2, characteristics?.Count);
        }
    }
}
