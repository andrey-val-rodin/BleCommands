using BleCommands.Core;
using BleCommands.Core.Contracts;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    /// <inheritdoc />
    public class BleTransport : BleTransport<
        IDevice<BluetoothLEDevice, Service>,
        IService<GattDeviceService, Characteristic>,
        ICharacteristic<GattCharacteristic>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BleTransport"/> class.
        /// </summary>
        /// <param name="device">A Bluetooth LE device. The device must be connected.</param>
        /// <param name="service"> A service.</param>
        /// <param name="commandCharacteristic">
        /// Characteristic for sending commands to the device (Write or WriteWithoutResponse).
        /// </param>
        /// <param name="responseCharacteristic">
        /// Characteristic for receiving command responses from the device (Notify or Indicate).
        /// </param>
        /// <param name="listeningCharacteristic">
        /// Characteristic for receiving token stream during listening (Notify or Indicate).
        /// </param>
        /// <param name="tokenDelimiter">Token separator. Typically, character '\n' is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any characteristic has invalid properties.</exception>
        public BleTransport(
            IDevice<BluetoothLEDevice, Service> device,
            IService<GattDeviceService, Characteristic> service,
            ICharacteristic<GattCharacteristic> commandCharacteristic,
            ICharacteristic<GattCharacteristic> responseCharacteristic,
            ICharacteristic<GattCharacteristic> listeningCharacteristic,
            char tokenDelimiter = TokenAggregator.DefaultTokenDelimiter)
        {
            VerifyParameters(
                device,
                service,
                commandCharacteristic,
                responseCharacteristic,
                listeningCharacteristic);

            Device = device;
            Service = service;
            CommandCharacteristic = commandCharacteristic;
            ResponseCharacteristic = responseCharacteristic;
            ListeningCharacteristic = listeningCharacteristic;
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
    }
}
