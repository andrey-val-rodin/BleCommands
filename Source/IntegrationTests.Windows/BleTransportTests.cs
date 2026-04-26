using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Events;
using BleCommands.Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// These tests use device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public sealed class BleTransportFixture : IAsyncLifetime
    {
        //private const string Id = "BluetoothLE#BluetoothLE90:e8:68:ad:f0:54-f8:b3:b7:22:09:3e";
        //private const ulong MacAddress = 0xf8b3b722093e;
        private static readonly Guid ServiceUuid                = new("0000ffe0-0000-1000-8000-00805f9b34fb");
        private static readonly Guid UpdatesCharacteristicUuid  = new("0000ffe1-0000-1000-8000-00805f9b34fb");
        private static readonly Guid WriteCharacteristicUuid    = new("0000ffe2-0000-1000-8000-00805f9b34fb");

        public IBleTransport<BluetoothLEDevice, GattDeviceService, GattCharacteristic> BleTransport { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> CommandCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> ResponseCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> ListeningCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> CharacteristicWithAttachedAggregator { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            var scanner = new BleScanner();
            var device = await scanner.FindDeviceAsync("Rotating Table");
            Assert.True(device != null, "Turn on Rotating Table!");
            Assert.True(await device.ConnectAsync(), "Connection failed");
            var service = (await device.GetServiceAsync(ServiceUuid))!;
            Assert.NotNull(service);
            CommandCharacteristic = (await service.GetCharacteristicAsync(WriteCharacteristicUuid))!;
            Assert.NotNull(CommandCharacteristic);
            ListeningCharacteristic = ResponseCharacteristic = (await service.GetCharacteristicAsync(UpdatesCharacteristicUuid))!;
            Assert.NotNull(ResponseCharacteristic);
            Assert.NotNull(ListeningCharacteristic);
            CharacteristicWithAttachedAggregator = (await service.GetCharacteristicAsync(UpdatesCharacteristicUuid))!;
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());
            BleTransport = new BleTransport(
                device,
                service,
                CommandCharacteristic,
                ResponseCharacteristic,
                ListeningCharacteristic,
                '\n');

            await BleTransport.BeginAsync();
            await StopTableAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (ResponseCharacteristic != null)
                await ResponseCharacteristic.StopUpdatesAsync();
            BleTransport?.Dispose();
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
    }

    public sealed class BleTransportTests(BleTransportFixture fixture)
        : IClassFixture<BleTransportFixture>
    {
        private BleTransportFixture Fixture { get; } = fixture;
        private IBleTransport<BluetoothLEDevice, GattDeviceService, GattCharacteristic> BleTransport => Fixture.BleTransport;

        [Fact]
        public async Task SendCommandAsync_Status_ValidResponse()
        {
            Assert.Equal("READY", await BleTransport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task SendCommandAsync_InsufficientTimeout_Null()
        {
            var oldTimeout = BleTransport.ResponseTimeout;
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object? sender, TextEventArgs args)
            {
                tcs.SetResult(true);
            }

            try
            {
                BleTransport.ResponseTimeout = TimeSpan.FromMilliseconds(1);
                BleTransport.ListeningTokenReceived += Handler;
                Assert.Null(await BleTransport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken));

                // Wait until we receive an actual response
                await tcs.Task;
            }
            finally
            {
                BleTransport.ListeningTokenReceived -= Handler;
                BleTransport.ResponseTimeout = oldTimeout;
            }
        }

        [Fact]
        public async Task ListeningIsInProgress_RotateTable_ReceiveTokens()
        {
            Assert.Equal("OK", await BleTransport.SendCommandAsync("RUN FM", TestContext.Current.CancellationToken));
            var tokens = new List<string>();
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object? sender, TextEventArgs args)
            {
                tokens.Add(args.Text);
                if (args.Text == "MOVERR")
                    tcs.SetResult(false); // Is Rotating Table turned on?
                else if (args.Text == "ENDSTEP")
                    tcs.TrySetResult(true);
            }

            void TimeoutHandler(object? sender, System.Timers.ElapsedEventArgs e)
            {
                tcs.TrySetResult(false); // Listening timeout
            }

            try
            {
                BleTransport.ListeningTokenReceived += Handler;
                BleTransport.ListeningTimeoutElapsed += TimeoutHandler;
                BleTransport.StartListening(TimeSpan.FromSeconds(3));
                Assert.Equal("OK", await BleTransport.SendCommandAsync("FM 1", TestContext.Current.CancellationToken));
                Assert.True(await tcs.Task);
                await Fixture.StopTableAsync();
                Assert.Contains(tokens, s => s.StartsWith("POS"));
                Assert.Equal("READY", await BleTransport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken));
            }
            finally
            {
                BleTransport.StopListening();
                BleTransport.ListeningTokenReceived -= Handler;
                BleTransport.ListeningTimeoutElapsed -= TimeoutHandler;
                BleTransport.StopListening();
            }
        }
    }
}
