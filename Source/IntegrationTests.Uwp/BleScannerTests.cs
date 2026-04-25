
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationTests.Uwp
{
    /// <summary>
    /// These tests use device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public class BleScannerTests : IDisposable
    {
        private BleScanner BleScanner { get; } = new BleScanner();

        [TestMethod]
        public async Task FindDeviceWithCts_Cancel_TaskCanceledExceptionAsync()
        {
            using var cts = new CancellationTokenSource();

            // Cancel in 100 мс
            cts.CancelAfter(100);

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
            {
                await BleScanner.FindDeviceAsync("Unexistent Device", cts.Token);
            });
        }

        [TestMethod]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNullAsync()
        {
            // Timeout 1 second
            var device = await BleScanner.FindDeviceAsync("Unexistent Device", TimeSpan.FromSeconds(1));
            Assert.IsNull(device);
        }

        [TestMethod]
        public async Task FindDevice_FoundAsync()
        {
            using var cts = new CancellationTokenSource();
            using var device = await BleScanner.FindDeviceAsync("Rotating Table", TimeSpan.FromSeconds(3));

            Assert.IsNotNull(device);
            await device.ConnectAsync(cts.Token);
            Assert.IsTrue(device.IsConnected);
        }

        public void Dispose()
        {
            BleScanner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
