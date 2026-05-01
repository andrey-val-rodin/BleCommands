using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace IntegrationTests.Uwp
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public class DeviceTests
    {
        public TestContext TestContext { get; set; }

        private BleScanner BleScanner => Fixture.BleScanner;

        [TestMethod]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNull()
        {
            // Timeout 100 milliseconds
            var device = await BleScanner.FindDeviceAsync("Unexistent Device", TimeSpan.FromMilliseconds(100));
            Assert.IsNull(device);
        }

        [TestMethod]
        public async Task GetServices_Success()
        {
            var device = Fixture.Device;
            Assert.IsNotNull(device);
            var services = await device.GetServicesAsync(TestContext.CancellationToken);

            Assert.IsNotNull(services);
            Assert.HasCount(3, services);
            Assert.Contains(s => s.Id == new Guid("00001801-0000-1000-8000-00805f9b34fb"), services);
            Assert.Contains(s => s.Id == new Guid("00001800-0000-1000-8000-00805f9b34fb"), services);
            Assert.Contains(s => s.Id == new Guid("0000ffe0-0000-1000-8000-00805f9b34fb"), services);
        }

        [TestMethod]
        public async Task ConnectToKnownDeviceAsync_Success()
        {
            using var device = new Device(Fixture.Id)
        }
    }
}
