using BleCommands.Core.Exceptions;
using BleCommands.Windows;
using BleCommands.Windows.Events;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [Collection("IntegrationTests.Windows")]
    public class BleScannerTests(Fixture fixture)
    {
        private BleScanner BleScanner => fixture.BleScanner;

        [Fact]
        public async Task FindDeviceAsync_NonExistentDeviceAndInsufficientTimeout_ReturnsNull()
        {
            var device = await BleScanner.FindDeviceAsync(
                "Non-existent Device", TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
            Assert.Null(device);
        }

        [Fact]
        public async Task FindDeviceAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();

            var searchTask = BleScanner.FindDeviceAsync(
                "Non-existent device for cancellation test",
                TimeSpan.FromSeconds(30),
                cts.Token);

            await Task.Delay(20, TestContext.Current.CancellationToken);
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await searchTask);
        }

        [Fact]
        public async Task ScanAsync_UntilRotatingTableIsFoundOrTimeRunsOut_RotatingTableIsFound()
        {
            bool rotatingTableFound = false;
            using var cts = new CancellationTokenSource(5000);
            async void Handler(object? sender, DeviceDiscoveredEventArgs e)
            {
                try
                {
                    if (rotatingTableFound || cts.IsCancellationRequested)
                        return;

                    using var device = new Device(e.BluetoothAddress);
                    await device.ConnectAsync(TestContext.Current.CancellationToken);
                    if (device.Name == "Rotating Table")
                    {
                        rotatingTableFound = true;
                        if (!cts.IsCancellationRequested)
                            cts.Cancel();
                    }
                }
                catch (DeviceException)
                {
                    // failed to connect
                }
            }

            try
            {
                BleScanner.DeviceDiscovered += Handler;
                await BleScanner.ScanAsync(token: cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Canceled in handler
            }
            finally
            {
                BleScanner.DeviceDiscovered -= Handler;
            }

            Assert.True(rotatingTableFound);
        }
    }
}
