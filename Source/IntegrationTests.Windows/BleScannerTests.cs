using BleCommands.Windows;
using BleCommands.Windows.Events;
using Windows.Devices.Bluetooth.Advertisement;

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
        public async Task FindDeviceWithTimeout_InsufficientTimeout_ReturnsNull()
        {
            var device = await BleScanner.FindDeviceAsync("Rotating Table", TimeSpan.FromMilliseconds(1));
            Assert.Null(device);
        }

        [Fact]
        public async Task FindDevice_NonExistentDevice_ReturnsNull()
        {
            var device = await BleScanner.FindDeviceAsync("Non-existent Device", TimeSpan.FromMilliseconds(500));
            Assert.Null(device);
        }
        /*
        [Fact]
        public async Task Test()
        {
            var scanner = new BleScanner();
            var devices = new List<Device>();
            scanner.DeviceDiscovered += Handler;
            void Handler(object? sender, DeviceDiscoveredEventArgs e)
            {
                devices.Add(new Device(e.BluetoothAddress));
            }

            var cts = new CancellationTokenSource(5000);
            var filter = new BluetoothLEAdvertisementFilter
            {
                Advertisement = new BluetoothLEAdvertisement
                {
                    LocalName = "Rotating Table"
                }
            };

            await scanner.ScanAsync(BluetoothLEScanningMode.Active, filter, cts.Token);

            foreach (var device in devices)
            {
                await device.ConnectAsync(TestContext.Current.CancellationToken);
            }
        }
        */
    }
}
