using BleCommands.Core;
using BleCommands.Core.Events;
using BleCommands.IntegrationTests.Windows;
using BleCommands.Windows;
using BleTransport = BleCommands.Windows.BleTransport;

[assembly: AssemblyFixture(typeof(Fixture))]
namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// The test fixture uses real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public class Fixture : IAsyncLifetime
    {
        public const string Id = "BluetoothLE#BluetoothLE90:e8:68:ad:f0:54-f8:b3:b7:22:09:3e";
        public const ulong MacAddress = 0xf8b3b722093e;
        public static readonly Guid DeviceUuid                  = new("00000000-0000-0000-8000-f8b3b722093e");
        public static readonly Guid ServiceUuid                 = new("0000ffe0-0000-1000-8000-00805f9b34fb");
        public static readonly Guid UpdatesCharacteristicUuid   = new("0000ffe1-0000-1000-8000-00805f9b34fb");
        public static readonly Guid WriteCharacteristicUuid     = new("0000ffe2-0000-1000-8000-00805f9b34fb");

        public BleScanner BleScanner { get; } = new BleScanner();

        public Device Device { get; private set; } = null!;

        public Service Service { get; private set; } = null!;

        public BleTransport BleTransport { get; private set; } = null!;

        public Characteristic CommandCharacteristic { get; private set; } = null!;

        public Characteristic ResponseCharacteristic { get; private set; } = null!;

        public Characteristic ListeningCharacteristic { get; private set; } = null!;

        public Characteristic CharacteristicWithAttachedAggregator { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            var scanner = new BleScanner();
            var device = await scanner.FindDeviceAsync("Rotating Table");
            Assert.True(device != null, "Turn on Rotating Table!");
            Device = device;
            await Device.ConnectAsync();
            // Verify connection
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.UtcNow;

            while (!device.IsConnected && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }

            if (!device.IsConnected)
                throw new TimeoutException("Device did not connect within timeout");

            Service = (await Device.GetServiceAsync(ServiceUuid))!;
            Assert.NotNull(Service);
            CommandCharacteristic = (await Service.GetCharacteristicAsync(WriteCharacteristicUuid))!;
            Assert.NotNull(CommandCharacteristic);
            ListeningCharacteristic = ResponseCharacteristic = (await Service.GetCharacteristicAsync(UpdatesCharacteristicUuid))!;
            Assert.NotNull(ResponseCharacteristic);
            Assert.NotNull(ListeningCharacteristic);
            CharacteristicWithAttachedAggregator = (await Service.GetCharacteristicAsync(UpdatesCharacteristicUuid))!;
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());
            BleTransport = new BleTransport(
                Device,
                Service,
                CommandCharacteristic,
                ResponseCharacteristic,
                ListeningCharacteristic,
                '\n');

            await BleTransport.StartAsync();
            await StopTableAsync();
        }

        public async Task StopTableAsync()
        {
            var status = await BleTransport.SendCommandAsync("STATUS");
            if (status != "BUSY")
                return;

            var tcs = new TaskCompletionSource<bool>();
            BleTransport.ListeningTokenReceived += Handler;
            void Handler(object? sender, TextEventArgs args)
            {
                if (args.Text == "END")
                    tcs.TrySetResult(true);
            }

            var response = await BleTransport.SendCommandAsync("STOP");
            if (response != "OK")
                tcs.TrySetResult(true);

            Assert.True(await tcs.Task);
            BleTransport.ListeningTokenReceived -= Handler;
        }

        public ValueTask DisposeAsync()
        {
            BleScanner.Dispose();
            CommandCharacteristic?.Dispose();
            ResponseCharacteristic?.Dispose();
            ListeningCharacteristic?.Dispose();
            CharacteristicWithAttachedAggregator?.Dispose();
            BleTransport?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
