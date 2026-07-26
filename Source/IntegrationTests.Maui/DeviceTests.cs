using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plugin.BLE.Abstractions.Exceptions;
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
        public async Task ConnectAsync_NonExistentBluetoothAddress_Exception()
        {
            // Arrange
            var device = new Device(Guid.Empty);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DeviceConnectionException>(async () =>
            {
                await device.ConnectAsync(TestContext.CancellationToken);
            });

            Assert.AreEqual(
                "[Adapter] Device 00000000-0000-0000-0000-000000000000 not found.",
                exception.Message);
        }

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
            Assert.Contains(s => s.Id == Fixture.ServiceUuid, services);
        }

        [TestMethod]
        public async Task GetCharacteristics_Success()
        {
            var device = Fixture.Device;
            Assert.IsNotNull(device);
            var service = await device.GetServiceAsync(Fixture.ServiceUuid, TestContext.CancellationToken);
            Assert.IsNotNull(service);
            var characteristics = await service.GetCharacteristicsAsync(TestContext.CancellationToken);

            Assert.IsNotNull(characteristics);
            Assert.HasCount(2, characteristics);
            Assert.Contains(c => c.Id == Fixture.UpdatesCharacteristicUuid, characteristics);
            Assert.Contains(c => c.Id == Fixture.WriteCharacteristicUuid, characteristics);
        }

        [TestMethod]
        public async Task ConnectToKnownDevice_Success()
        {
            // Plugin.BLE stores devices in the cache, so we should not dispose our device object here
            var device = new Device(Fixture.DeviceUuid);
            await device.ConnectAsync(TestContext.CancellationToken);

            Assert.IsTrue(device.IsConnected);
            /*
             * If Assert.IsTrue fails, then instead of checking the connection status immediately,
             * you should use the following code:
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

        [TestMethod]
        public async Task GetStatusConcurrently_Success()
        {
            const int ThreadCount = 5;
            List<Task<string?>> tasks = [];
            async Task<string?> TaskProcAsync()
            {
                Assert.IsNotNull(Fixture.BleTransport);
                var response = await Fixture.BleTransport.SendCommandAsync(
                    "STATUS", TestContext.CancellationToken);
                return response;
            }

            for (int i = 0; i < ThreadCount; i++)
            {
                tasks.Add(TaskProcAsync());
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                Assert.AreEqual("READY", await task);
            }
        }
    }
}
