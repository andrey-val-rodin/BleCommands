using BleCommands.Core;
using BleCommands.Core.Enums;
using BleTransport = BleCommands.Windows.BleTransport;

namespace BleCommands.Tests.Windows
{
    public class BleTransportTests
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
        public void Constructor_ListeningeCharacteristicWithAttachedAggregator_ArgumentException()
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
                var transport = new BleTransport(
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

                await transport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken);
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

                await transport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken);
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
    }
}
