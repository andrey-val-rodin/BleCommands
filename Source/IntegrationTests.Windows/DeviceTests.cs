using BleCommands.Windows;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public class DeviceTests(Fixture fixture)
    {
        private Fixture Fixture { get; } = fixture;

        private BleScanner BleScanner => Fixture.BleScanner;

        [Fact]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNull()
        {
            // Timeout 100 milliseconds
            var device = await BleScanner.FindDeviceAsync("Unexistent Device", TimeSpan.FromMilliseconds(100));
            Assert.Null(device);
        }

        [Fact]
        public async Task FindDeviceAndConnect_Success()
        {
            using var device = await BleScanner.FindDeviceAsync("Rotating Table");

            Assert.NotNull(device);
            await device.ConnectAsync(TestContext.Current.CancellationToken);
            Assert.True(device.IsConnected);
            /*
             * TODO: Instead of checking the connection status immediately, you should use the following code:
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.UtcNow;

            while (!device.IsConnected && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }

            if (!device.IsConnected)
                throw new TimeoutException("Device did not connect within timeout");
            */
        }

        [Fact]
        public async Task GetServices_Success()
        {
            var device = Fixture.Device;
            Assert.NotNull(device);
            var services = await device.GetServicesAsync(TestContext.Current.CancellationToken);

            Assert.NotNull(services);
            Assert.Equal(3, services.Count);
            Assert.Contains(services, s => s.Id == new Guid("00001801-0000-1000-8000-00805f9b34fb"));
            Assert.Contains(services, s => s.Id == new Guid("00001800-0000-1000-8000-00805f9b34fb"));
            Assert.Contains(services, s => s.Id == new Guid("0000ffe0-0000-1000-8000-00805f9b34fb"));
        }

        [Fact]
        public async Task ConnectToKnownDeviceAsync_Success()
        {
            using var device = new Device(Fixture.MacAddress);
            await device.ConnectAsync(TestContext.Current.CancellationToken);
            Assert.True(device.IsConnected);
            /*
             * TODO: Instead of checking the connection status immediately, you should use the following code:
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.UtcNow;

            while (!device.IsConnected && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }

            if (!device.IsConnected)
                throw new TimeoutException("Device did not connect within timeout");
            */
        }
    }
}
