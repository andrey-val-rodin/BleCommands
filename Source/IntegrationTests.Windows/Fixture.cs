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
    public sealed class Fixture : IAsyncLifetime
    {
        public const string Id = "BluetoothLE#BluetoothLE90:e8:68:ad:f0:54-f8:b3:b7:22:09:3e";
        public const ulong MacAddress = 0xf8b3b722093e;
        public static readonly Guid ServiceUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B901");
        public static readonly Guid CommandCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B902");
        public static readonly Guid ResponseCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B903");
        public static readonly Guid ListeningCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B904");

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
            var transport = await ArduinoClient.CreateTransportAsync("Rotating Table");
            Assert.True(transport != null, "Turn on Rotating Table!");

            BleTransport = transport;
            Device = (Device)BleTransport.Device;
            Service = (Service)BleTransport.Service;
            CommandCharacteristic = (Characteristic)BleTransport.CommandCharacteristic;
            ResponseCharacteristic = (Characteristic)BleTransport.ResponseCharacteristic;
            ListeningCharacteristic = (Characteristic)BleTransport.ListeningCharacteristic;
            CharacteristicWithAttachedAggregator = (await Service.GetCharacteristicAsync(ListeningCharacteristicUuid))!;
            Assert.NotNull(CharacteristicWithAttachedAggregator);
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());

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

            await BleTransport.SendCommandAsync("STOP");

            Assert.True(await tcs.Task);
            BleTransport.ListeningTokenReceived -= Handler;
        }

        public ValueTask DisposeAsync()
        {
            BleTransport?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
