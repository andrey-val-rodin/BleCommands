using BleCommands.Core;
using BleCommands.Core.Enums;
using BleTransport = BleCommands.Maui.BleTransport;

namespace BleCommands.Tests.Maui
{
    public class BleTransportTests : IDisposable
    {
        public BleTransportTests()
        {
            CharacteristicWithAttachedAggregator = new CharacteristicStub(CharacteristicPropertyFlags.Indicate);
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());
        }

        private CharacteristicStub CharacteristicWithAttachedAggregator { get; }

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
                    CharacteristicWithAttachedAggregator,
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
        public void Constructor_ListeningCharacteristicWithAttachedAggregator_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    CharacteristicWithAttachedAggregator);
            });
            Assert.Equal("listeningCharacteristic", exception.ParamName);
        }

        [Fact]
        public async Task StartAsync_Disposed_ObjectDisposedException()
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                using var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));
                transport.Dispose();

                await transport.StartAsync(TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public async Task StartAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                using var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));

                using var cts = new CancellationTokenSource();
                cts.Cancel();

                await transport.StartAsync(cts.Token);
            });
        }

        [Fact]
        public async Task SendCommandAsync_Disposed_ObjectDisposedException()
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                using var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));
                transport.Dispose();

                await transport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public async Task SendCommandAsync_NotStarted_InvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                using var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));

                await transport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public async Task StartListening_ZeroTimeout_ArgumentOutOfRangeException()
        {
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify));

            await transport.StartAsync(TestContext.Current.CancellationToken);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => transport.StartListening(TimeSpan.Zero));
        }

        [Fact]
        public async Task StartListening_ReceivedToken_RaisesListeningTokenReceived()
        {
            var listeningCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Notify);
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                listeningCharacteristic);
            await transport.StartAsync(TestContext.Current.CancellationToken);

            var received = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            transport.ListeningTokenReceived += (_, args) =>
                received.TrySetResult(args.Text);

            transport.StartListening(TimeSpan.FromSeconds(1));

            listeningCharacteristic.EmulateReceiving("TOKEN\n");

            var result = await received.Task.WaitAsync(
                TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

            Assert.Equal("TOKEN", result);
            Assert.True(transport.IsListening);
        }

        [Fact]
        public async Task Listening_FragmentedToken_RaisesOnlyAfterDelimiter()
        {
            var listeningCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Notify);
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                listeningCharacteristic);
            await transport.StartAsync(TestContext.Current.CancellationToken);

            var received = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            transport.ListeningTokenReceived += (_, args) =>
                received.TrySetResult(args.Text);

            transport.StartListening(TimeSpan.FromSeconds(1));

            listeningCharacteristic.EmulateReceiving("HEL");
            listeningCharacteristic.EmulateReceiving("LO\n");

            Assert.Equal("HELLO", await received.Task.WaitAsync(
                TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task SendCommandAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                using var transport = new BleTransport(
                    new DeviceStub(),
                    new ServiceStub(),
                    new CharacteristicStub(CharacteristicPropertyFlags.Write),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify));

                await transport.StartAsync(TestContext.Current.CancellationToken);
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(50));

                await transport.SendCommandAsync("STATUS", cts.Token);
            });
        }

        [Fact]
        public void ResponseTimeout_ZeroTimeout_ArgumentOutOfRangeException()
        {
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify));

            void act() => transport.ResponseTimeout = TimeSpan.Zero;
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void StartListening_NotStarted_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var transport = new BleTransport(
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
                using var transport = new BleTransport(
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
        public async Task StartListening_WhenAlreadyListening_ThrowsInvalidOperationException()
        {
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify));
            await transport.StartAsync(TestContext.Current.CancellationToken);

            transport.StartListening(TimeSpan.FromSeconds(1));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                transport.StartListening(TimeSpan.FromSeconds(1)));
            Assert.Equal("Listening is already in progress.", exception.Message);
        }

        [Fact]
        public async Task StartListening_StartsListening()
        {
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify));
            await transport.StartAsync(TestContext.Current.CancellationToken);

            transport.StartListening(TimeSpan.FromSeconds(1));
            Assert.True(transport.IsListening);
        }

        [Fact]
        public async Task StopListening_StopsListening()
        {
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify));
            await transport.StartAsync(TestContext.Current.CancellationToken);

            transport.StartListening(TimeSpan.FromSeconds(1));
            transport.StopListening();

            Assert.False(transport.IsListening);
        }

        [Fact]
        public async Task StopListening_WhenNotListening_DoesNothing()
        {
            using var transport = new BleTransport(
                new DeviceStub(),
                new ServiceStub(),
                new CharacteristicStub(CharacteristicPropertyFlags.Write),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                new CharacteristicStub(CharacteristicPropertyFlags.Notify));
            await transport.StartAsync(TestContext.Current.CancellationToken);

            transport.StopListening();
            transport.StopListening();

            Assert.False(transport.IsListening);
        }

        [Fact]
        public void Instance_WithSpecifiedTokenDelimiter_AggregatorsUseThisTokenDelimiter()
        {
            const char delimiter = '\x04'; // EOT
            var device = new DeviceStub();
            var service = new ServiceStub();
            var commandCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Write);
            var responseCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Notify);
            var listeningCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Notify);
            using var transport = new BleTransport(
                device,
                service,
                commandCharacteristic,
                responseCharacteristic,
                listeningCharacteristic,
                delimiter);

            Assert.Equal(delimiter, transport.TokenDelimiter);
            Assert.Equal(delimiter, transport.ResponseCharacteristic.TokenAggregator?.TokenDelimiter);
            Assert.Equal(delimiter, transport.ListeningCharacteristic.TokenAggregator?.TokenDelimiter);
        }

        [Fact]
        public void Instance_Dispose_EverythingIsDisposed()
        {
            var device = new DeviceStub();
            var service = new ServiceStub();
            var commandCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Write);
            var responseCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Notify);
            var listeningCharacteristic = new CharacteristicStub(CharacteristicPropertyFlags.Notify);
            using var transport = new BleTransport(
                device,
                service,
                commandCharacteristic,
                responseCharacteristic,
                listeningCharacteristic);
            transport.Dispose();

            Assert.True(device.Disposed);
            Assert.True(service.Disposed);
            Assert.True(commandCharacteristic.Disposed);
            Assert.True(responseCharacteristic.Disposed);
            Assert.True(listeningCharacteristic.Disposed);
        }

        public void Dispose()
        {
            CharacteristicWithAttachedAggregator.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
