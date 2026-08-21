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
        public async Task FindDevice_NonExistentDeviceAndInsufficientTimeout_ReturnsNull()
        {
            var device = await BleScanner.FindDeviceAsync("Non-existent Device", TimeSpan.FromMilliseconds(10));
            Assert.Null(device);
        }

        [Fact]
        public async Task ScanAsync_UntilRotatingTableIsFoundOrTimeRunsOut_Found()
        {
            var devices = new List<Device>();
            var cts = new CancellationTokenSource(5000);
            async Task Handler(object? sender, DeviceDiscoveredEventArgs e)
            {
                var device = new Device(e.BluetoothAddress);
                await device.ConnectAsync();
                devices.Add(device);
                if (device.Name == "Rotating Table")
                    cts.Cancel();
            }

            BleScanner.DeviceDiscovered += async (s, a) => await Handler(s, a);

            await BleScanner.ScanAsync(token: cts.Token);

            Assert.NotNull(devices.FirstOrDefault(d => d.Name == "Rotating Table"));
        }

        [Fact]
        public async Task ScanConcurrently_NoExceptions()
        {
            const int ThreadCount = 5;
            List<Task<Exception?>> tasks = [];
            async Task<Exception?> TaskProcAsync()
            {
                try
                {
                    var cts = new CancellationTokenSource(100);
                    await BleScanner.ScanAsync(token: cts.Token);
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }

            for (int i = 0; i < ThreadCount; i++)
            {
                tasks.Add(TaskProcAsync());
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                Assert.Null(await task);
            }
        }
    }
}
