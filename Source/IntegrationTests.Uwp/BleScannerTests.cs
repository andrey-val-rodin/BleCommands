
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace BleCommands.IntegrationTests.Uwp
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public class BleScannerTests : IDisposable
    {
        private BleScanner BleScanner { get; } = new BleScanner();

        [TestMethod]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNullAsync()
        {
            // Timeout 1 second
            var device = await BleScanner.FindDeviceAsync("Unexistent Device", TimeSpan.FromSeconds(1));
            Assert.IsNull(device);
        }

        [TestMethod]
        public async Task FindDeviceAndConnect_SuccessAsync()
        {
            using var device = await BleScanner.FindDeviceAsync("Rotating Table");

            Assert.IsNotNull(device);
            await device.ConnectAsync(TestContext.CancellationToken);
            Assert.IsTrue(device.IsConnected);
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

        public void Dispose()
        {
            BleScanner.Dispose();
            GC.SuppressFinalize(this);
        }

        public TestContext TestContext { get; set; }
    }
}
