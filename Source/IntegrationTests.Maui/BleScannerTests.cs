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
        public async Task FindDeviceAsync_NonExistentDeviceAndInsufficientTimeout_ReturnsNull()
        {
            var device = await BleScanner.FindDeviceAsync(
                "Non-existent Device", TimeSpan.FromMilliseconds(1), TestContext.CancellationToken);
            Assert.IsNull(device);
        }

        [TestMethod]
        public async Task FindDeviceAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();

            var searchTask = BleScanner.FindDeviceAsync(
                "Non-existent device for cancellation test",
                TimeSpan.FromSeconds(30),
                cts.Token);

            await Task.Delay(20, TestContext.CancellationToken);
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await searchTask);
        }

        [TestMethod]
        public async Task FindDeviceAsync_UsesRequestedTimeout()
        {
            var adapter = BleScanner.Adapter;
            var originalScanTimeout = adapter.ScanTimeout;

            try
            {
                adapter.ScanTimeout = 1;

                var started = DateTime.UtcNow;
                var device = await BleScanner.FindDeviceAsync(
                    "Non-existent device for timeout test",
                    TimeSpan.FromMilliseconds(50),
                    TestContext.CancellationToken);
                var elapsed = DateTime.UtcNow - started;

                Assert.IsNull(device);
                Assert.IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(30), elapsed);
            }
            finally
            {
                adapter.ScanTimeout = originalScanTimeout;
            }
        }

        [TestMethod]
        public async Task FindDeviceAsync_ExternalCancellation_RestoresScanTimeout()
        {
            var adapter = BleScanner.Adapter;
            var originalScanTimeout = adapter.ScanTimeout;
            adapter.ScanTimeout = 1234;

            try
            {
                using var cts = new CancellationTokenSource();
                var searchTask = BleScanner.FindDeviceAsync(
                    "Non-existent device for restoration test",
                    TimeSpan.FromSeconds(30),
                    cts.Token);

                cts.Cancel();

                await Assert.ThrowsAsync<OperationCanceledException>(
                    async () => await searchTask);

                Assert.AreEqual(1234, adapter.ScanTimeout);
            }
            finally
            {
                adapter.ScanTimeout = originalScanTimeout;
            }
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
                // Canceled in handler
            }
            finally
            {
                BleScanner.DeviceDiscovered -= Handler;
            }

            Assert.IsTrue(rotatingTableFound);
        }
    }
}
