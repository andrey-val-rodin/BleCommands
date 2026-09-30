using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// A transport for the Rotating Table test device, which uses a single Update
    /// characteristic for both command responses and listening tokens.
    /// </summary>
    public class RotatingTableTransport : BleTransport<
        IDevice<BluetoothLEDevice, Service>,
        IService<GattDeviceService, Characteristic>,
        ICharacteristic<GattCharacteristic>>
    {
        /// <summary>
        /// A constructor.
        /// </summary>
        /// <param name="device">A Bluetooth LE device. The device must be connected.</param>
        /// <param name="service"> A service.</param>
        /// <param name="commandCharacteristic">
        /// Characteristic for sending commands to the device (Write or WriteWithoutResponse).
        /// </param>
        /// <param name="updatesCharacteristic">
        /// Characteristic for receiving command responses and messages from the device (Notify or Indicate).
        /// </param>
        /// <param name="tokenDelimiter">Token separator. Typically, character '\n' is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any characteristic has invalid properties.</exception>
        public RotatingTableTransport(
            IDevice<BluetoothLEDevice, Service> device,
            IService<GattDeviceService, Characteristic> service,
            ICharacteristic<GattCharacteristic> commandCharacteristic,
            ICharacteristic<GattCharacteristic> updatesCharacteristic,
            char tokenDelimiter = TokenAggregator.DefaultTokenDelimiter)
        {
            VerifyParameters(
                device,
                service,
                commandCharacteristic,
                updatesCharacteristic,
                updatesCharacteristic);

            Device = device;
            Service = service;
            CommandCharacteristic = commandCharacteristic;
            ListeningCharacteristic = ResponseCharacteristic = updatesCharacteristic;
            TokenDelimiter = tokenDelimiter;
        }

        /// <inheritdoc />
        public override IDevice<BluetoothLEDevice, Service> Device { get; }

        /// <inheritdoc />
        public override IService<GattDeviceService, Characteristic> Service { get; }

        /// <inheritdoc />
        public override ICharacteristic<GattCharacteristic> CommandCharacteristic { get; }

        /// <inheritdoc />
        public override ICharacteristic<GattCharacteristic> ResponseCharacteristic { get; }

        /// <inheritdoc />
        public override ICharacteristic<GattCharacteristic> ListeningCharacteristic { get; }

        public override async Task StartAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (IsStarted)
                return;

            // Rotating Table has only one Update characteristic
            Assert.Equal(ResponseCharacteristic, ListeningCharacteristic);

            ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator(TokenDelimiter));
            await ResponseCharacteristic.StartReceivingAsync(token).ConfigureAwait(false);

            IsStarted = true;
        }
    }
}
