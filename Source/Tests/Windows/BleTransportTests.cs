using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using BleCommands.Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests.Windows
{
    /// <summary>
    /// Temporary tests for Windows.BleScanner. Will be moved to IntegrationTests.
    /// These tests use device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    public sealed class BleTransportFixture : IAsyncLifetime
    {
        private const string Id = "BluetoothLE#BluetoothLE90:e8:68:ad:f0:54-f8:b3:b7:22:09:3e";
        private const ulong MacAddress = 0xf8b3b722093e;
        private static readonly Guid ServiceUuid = new("0000ffe0-0000-1000-8000-00805f9b34fb");
        private static readonly Guid UpdatesCharacteristicUuid = new("0000ffe1-0000-1000-8000-00805f9b34fb");
        private static readonly Guid WriteCharacteristicUuid = new("0000ffe2-0000-1000-8000-00805f9b34fb");

        public IBleTransport<BluetoothLEDevice, GattDeviceService, GattCharacteristic> BleTransport { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> CommandCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> ResponseCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> ListeningCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> CharacteristicWithAttachedAggregator { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            var device = new Device(Id);
            Assert.True(await device.ConnectAsync(), "Turn on Rotating Table! Also, make sure the device is paired in Windows parameters.");
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
        public void Constructor_DeviceIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    null!,
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("device", exception.ParamName);
        }

        [Fact]
        public void Constructor_ServiceIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    null!,
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("service", exception.ParamName);
        }

        [Fact]
        public void Constructor_CommandCharacteristicIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    null!,
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("commandCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_WrongCommandCharacteristic_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("commandCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ResponseCharacteristicIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    null!,
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("responseCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_WrongResponseCharacteristic_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(0),
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("responseCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ResponseCharacteristicWithAttachedAggregator_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    Fixture.CharacteristicWithAttachedAggregator,
                    new CharacteristicStub(CharacteristicPropertyFlags.Indicate));
            });
            Assert.Equal("responseCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ListeningCharacteristicIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    null!);
            });
            Assert.Equal("listeningCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_WrongListeningCharacteristic_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(0));
            });
            Assert.Equal("listeningCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ListeningeCharacteristicWithAttachedAggregator_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    Fixture.CharacteristicWithAttachedAggregator);
            });
            Assert.Equal("listeningCharacteristic", exception.ParamName);
        }

        [Fact]
        public async Task StartAsync_Disposed_ObjectDisposedException()
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));
                transport.Dispose();

                var cts = new CancellationTokenSource();
                await transport.BeginAsync(cts.Token);
            });
        }

        [Fact]
        public async Task SendCommandAsync_Disposed_ObjectDisposedException()
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));
                transport.Dispose();

                var cts = new CancellationTokenSource();
                await transport.SendCommandAsync("STATUS", cts.Token);
            });
        }

        [Fact]
        public async Task SendCommandAsync_NotStarted_InvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));

                var cts = new CancellationTokenSource();
                await transport.SendCommandAsync("STATUS", cts.Token);
            });
        }

        [Fact]
        public void StartListening_NotStarted_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));

                transport.StartListening(TimeSpan.FromSeconds(1));
            });
        }

        [Fact]
        public void StartListening_Disposed_ObjectDisposedException()
        {
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));
                transport.Dispose();

                transport.StartListening(TimeSpan.FromSeconds(1));
            });
        }

        [Fact]
        public async Task SendCommandAsync_Status_ValidResponse()
        {
            var cts = new CancellationTokenSource();
            Assert.Equal("READY", await BleTransport.SendCommandAsync("STATUS", cts.Token));
        }

        [Fact]
        public async Task SendCommandAsync_InsufficientTimeout_Null()
        {
            var cts = new CancellationTokenSource();
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
                Assert.Null(await BleTransport.SendCommandAsync("STATUS", cts.Token));

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
            var cts = new CancellationTokenSource();
            Assert.Equal("OK", await BleTransport.SendCommandAsync("RUN FM", cts.Token));
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
                Assert.Equal("OK", await BleTransport.SendCommandAsync("FM 1", cts.Token));
                Assert.True(await tcs.Task);
                await Fixture.StopTableAsync();
                Assert.Contains(tokens, s => s.StartsWith("POS"));
                Assert.Equal("READY", await BleTransport.SendCommandAsync("STATUS", cts.Token));
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
