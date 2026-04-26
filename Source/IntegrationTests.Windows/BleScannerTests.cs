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
        public async Task FindDeviceWithCts_Cancel_TaskCanceledException()
        {
            using var cts = new CancellationTokenSource();

            // Cancel in 100 мс
            cts.CancelAfter(100);

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await BleScanner.FindDeviceAsync("Unexistent Device", cts.Token);
            });
        }

        [Fact]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNull()
        {
            // Timeout 1 second
            var device = await BleScanner.FindDeviceAsync("Unexistent Device", TimeSpan.FromSeconds(1));
            Assert.Null(device);
        }

        [Fact]
        public async Task FindDevice_Found()
        {
            using var device = await BleScanner.FindDeviceAsync("Rotating Table", TimeSpan.FromSeconds(1));

            Assert.NotNull(device);
            await device.ConnectAsync(TestContext.Current.CancellationToken);
            Assert.True(device.IsConnected);
        }

        public void Dispose()
        {
            BleScanner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
