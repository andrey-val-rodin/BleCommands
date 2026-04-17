using BleCommands.Windows;

namespace BleCommands.Tests.Windows
{
    /// <summary>
    /// Temporary tests for Windows.BleScanner. Will be moved to IntegrationTests.
    /// These tests use device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public class BleScannerTests
    {
        private BleScanner BleScanner { get; } = new BleScanner();

        [Fact]
        public async Task FindDeviceWithCts_Cancel_OperationCanceledException()
        {
            using var cts = new CancellationTokenSource();

            // Cancel in 100 мс
            cts.CancelAfter(100);

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var device = await BleScanner.FindDeviceAsync("Rotating Table", cts.Token);

            Assert.NotNull(device);
            var n = device.Name;
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
