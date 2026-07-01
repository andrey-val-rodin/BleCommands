using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntegrationTests.Maui
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public class DeviceTests
    {
        public TestContext TestContext { get; set; }

        private static BleScanner BleScanner => Fixture.BleScanner;

        [TestMethod]
        public async Task FindDeviceWithTimeout_Timeout_ReturnsNull()
        {
            // Timeout 100 milliseconds
            var device = await BleScanner.FindDeviceAsync("Non-existent Device", TimeSpan.FromMilliseconds(100));
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
        public async Task GetCharacteristics_Success()
        {
            var device = Fixture.Device;
            Assert.IsNotNull(device);
            var service = await device.GetServiceAsync(
                new Guid("0000ffe0-0000-1000-8000-00805f9b34fb"), TestContext.CancellationToken);
            Assert.IsNotNull(service);
            var characteristics = await service.GetCharacteristicsAsync(TestContext.CancellationToken);

            Assert.IsNotNull(characteristics);
            Assert.HasCount(2, characteristics);
            Assert.Contains(c => c.Id == new Guid("0000ffe1-0000-1000-8000-00805f9b34fb"), characteristics);
            Assert.Contains(c => c.Id == new Guid("0000ffe2-0000-1000-8000-00805f9b34fb"), characteristics);
        }

        // This test hangs due to known issue: https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le/issues/1003
        //[TestMethod]
        //public async Task ConnectToKnownDevice_Success()
        //{
        //    using var device = new Device(Fixture.DeviceUuid);
        //    await device.ConnectAsync(TestContext.CancellationToken);
        //
        //    Assert.IsTrue(device.IsConnected);
        //    /*
        //     * If Assert.IsTrue fails, then instead of checking the connection status immediately,
        //     * you should use the following code:
        //    var timeout = TimeSpan.FromSeconds(5);
        //    var start = DateTime.UtcNow;
        //
        //    while (!device.IsConnected && DateTime.UtcNow - start < timeout)
        //    {
        //        await Task.Delay(50, TestContext.Current.CancellationToken);
        //    }
        //
        //    if (!device.IsConnected)
        //        throw new TimeoutException("Device did not connect within timeout");
        //    */
        //}

        [TestMethod]
        public async Task GetStatusConcurrently_Success()
        {
            const int ThreadCount = 5;
            List<Task<string?>> tasks = [];
            async Task<string?> TaskProc()
            {
                Assert.IsNotNull(Fixture.BleTransport);
                var response = await Fixture.BleTransport.SendCommandAsync(
                    "STATUS", TestContext.CancellationToken);
                return response;
            }

            for (int i = 0; i < ThreadCount; i++)
            {
                tasks.Add(TaskProc());
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                Assert.AreEqual("READY", await task);
            }
        }
    }
}
