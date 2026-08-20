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
    public class BleScannerTests()
    {
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

            await scanner.ScanAsync(cts.Token, BluetoothLEScanningMode.Active, filter);

            foreach (var device in devices)
            {
                await device.ConnectAsync(TestContext.Current.CancellationToken);
            }
        }
    }
}
