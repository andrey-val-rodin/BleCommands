using BleCommands.Windows;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// These tests use device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public class BleScannerTests : IDisposable
    {
        private BleScanner BleScanner { get; } = new BleScanner();

        [Fact]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNull()
        {
            // Timeout 1 second
            var device = await BleScanner.FindDeviceAsync("Unexistent Device", TimeSpan.FromSeconds(1));
            Assert.Null(device);
        }

        [Fact]
        public async Task FindDeviceAndConnect_Success()
        {
            using var device = await BleScanner.FindDeviceAsync("Rotating Table");

            Assert.NotNull(device);
            await device.ConnectAsync(TestContext.Current.CancellationToken);
            Assert.True(device.IsConnected, "Device should be connected");
        }

        public void Dispose()
        {
            BleScanner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
