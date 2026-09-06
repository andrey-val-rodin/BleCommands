using BleCommands.Core.Exceptions;
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationTests.Maui
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public class BleScannerTests()
    {
        public TestContext TestContext { get; set; }

        private static BleScanner BleScanner => Fixture.BleScanner;

        [TestMethod]
        public async Task FindDevice_NonExistentDeviceAndInsufficientTimeout_ReturnsNull()
        {
            var device = await BleScanner.FindDeviceAsync("Non-existent Device", TimeSpan.FromMilliseconds(1));
            Assert.IsNull(device);
        }

        [TestMethod]
        public async Task ScanAsync_UntilRotatingTableIsFoundOrTimeRunsOut_RotatingTableIsFound()
        {
            bool rotatingTableFound = false;
            using var cts = new CancellationTokenSource(5000);
            void Handler(object? sender, DeviceEventArgs e)
            {
                try
                {
                    if (e.Device.Name == "Rotating Table")
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
                Assert.IsTrue(rotatingTableFound);
            }
            finally
            {
                BleScanner.DeviceDiscovered -= Handler;
            }
        }
    }
}
