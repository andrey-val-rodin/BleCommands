using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
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

        public IBleTransport<GattCharacteristic> BleTransport { get; private set; } = null!;

        public IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic> Device { get; private set; } = null!;
        
        public IService<GattDeviceService, GattCharacteristic> Service { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> CommandCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> ResponseCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> ListeningCharacteristic { get; private set; } = null!;

        public ICharacteristic<GattCharacteristic> CharacteristicWithAttachedAggregator { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            Device = new Device(Id);
            Assert.True(await Device.ConnectAsync(), "Turn on Rotating Table! Also, make sure the device is paired in Windows parameters.");
            Service = (await Device.GetServiceAsync(ServiceUuid))!;
            Assert.NotNull(Service);
            CommandCharacteristic = (await Service.GetCharacteristicAsync(WriteCharacteristicUuid))!;
            Assert.NotNull(CommandCharacteristic);
            ListeningCharacteristic = ResponseCharacteristic = (await Service.GetCharacteristicAsync(UpdatesCharacteristicUuid))!;
            Assert.NotNull(ResponseCharacteristic);
            Assert.NotNull(ListeningCharacteristic);
            CharacteristicWithAttachedAggregator = (await Service.GetCharacteristicAsync(UpdatesCharacteristicUuid))!;
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());
        }

        public ValueTask DisposeAsync()
        {
            Device?.Dispose();
            Service?.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    public class BleTransportTests(BleTransportFixture fixture) : IClassFixture<BleTransportFixture>
    {
        private BleTransportFixture Fixture { get; } = fixture;

        [Fact]
        public void Constructor_CommandCharacteristicIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    null!,
                    Fixture.ResponseCharacteristic,
                    Fixture.ListeningCharacteristic);
            });
            Assert.Equal("commandCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_WrongCommandCharacteristic_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    new CharacteristicStub(CharacteristicPropertyFlags.Notify),
                    Fixture.ResponseCharacteristic,
                    Fixture.ListeningCharacteristic);
            });
            Assert.Equal("commandCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ResponseCharacteristicIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    Fixture.CommandCharacteristic,
                    null!,
                    Fixture.ListeningCharacteristic);
            });
            Assert.Equal("responseCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_WrongResponseCharacteristic_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    Fixture.CommandCharacteristic,
                    new CharacteristicStub(0),
                    Fixture.ListeningCharacteristic);
            });
            Assert.Equal("responseCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ResponseCharacteristicWithAttachedAggregator_ArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new BleTransport(
                    Fixture.CommandCharacteristic,
                    Fixture.CharacteristicWithAttachedAggregator,
                    Fixture.ListeningCharacteristic);
            });
            Assert.Equal("responseCharacteristic", exception.ParamName);
        }

        [Fact]
        public void Constructor_ListeningCharacteristicIsNull_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new BleTransport(
                    Fixture.CommandCharacteristic,
                    Fixture.ResponseCharacteristic,
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
                    Fixture.CommandCharacteristic,
                    Fixture.ResponseCharacteristic,
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
                    Fixture.CommandCharacteristic,
                    Fixture.ResponseCharacteristic,
                    Fixture.CharacteristicWithAttachedAggregator);
            });
            Assert.Equal("listeningCharacteristic", exception.ParamName);
        }
    }
}
