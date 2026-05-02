using BleCommands.Windows;
using System.Reflection;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public class DeviceTests(Fixture fixture) : IDisposable
    {
        private readonly List<object> _disposableObjects = [];

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
            RegisterDisposableObject(device);
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

            var services = await device.GetServicesAsync(TestContext.Current.CancellationToken);

            Assert.NotNull(services);
            Assert.Equal(3, services.Count);
            Assert.Contains(services, s => s.Id == new Guid("00001801-0000-1000-8000-00805f9b34fb"));
            Assert.Contains(services, s => s.Id == new Guid("00001800-0000-1000-8000-00805f9b34fb"));
            Assert.Contains(services, s => s.Id == new Guid("0000ffe0-0000-1000-8000-00805f9b34fb"));

            // Register children to ensure they are all disposed
            foreach (var service in services)
            {
                RegisterDisposableObject(service);
                if (service.Id == new Guid("0000ffe0-0000-1000-8000-00805f9b34fb"))
                    continue; // otherwise we will get Access Denied

                var characteristics = await service.GetCharacteristicsAsync(
                    TestContext.Current.CancellationToken);
                Assert.NotNull(characteristics);

                foreach (var characteristic in characteristics)
                {
                    RegisterDisposableObject(characteristic);
                }
            }
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
            RegisterDisposableObject(device);
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

        void RegisterDisposableObject(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            _disposableObjects.Add(obj);
        }

        private static void VerifyDisposeWasCalled(object obj)
        {
            TestContext.Current.AddWarning($"VerifyDisposeWasCalled");
            var type = obj.GetType();

            var disposedField = type.GetField("_disposed",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var value = disposedField?.GetValue(obj);
            if (value != null)
            {
                var isDisposed = (bool)value;
                Assert.True(isDisposed, $"The {type.Name} object was not disposed");
            }
            else
            {
                if (obj is IDisposable)
                {
                    Assert.Fail($"The {type.Name} object implements IDisposable, " +
                        "but the _disposed field was not found.");
                }
            }
        }

        public void Dispose()
        {
            foreach (var obj in _disposableObjects)
            {
                VerifyDisposeWasCalled(obj);
            }

            GC.SuppressFinalize(this);
        }
    }
}
