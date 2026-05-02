using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
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
        /// A constructor.
        /// </summary>
        /// <param name="device">A Bluetooth LE device.</param>
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
        /// <exception cref="ArgumentNullException">Thrown if any characteristic is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any characteristic has invalid properties.</exception>
        public BleTransport(
            IDevice<BluetoothLEDevice, Service> device,
            IService<GattDeviceService, Characteristic> service,
            ICharacteristic<GattCharacteristic> commandCharacteristic,
            ICharacteristic<GattCharacteristic> responseCharacteristic,
            ICharacteristic<GattCharacteristic> listeningCharacteristic,
            char tokenDelimiter = TokenAggregator.DefaultTokenDelimiter)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(commandCharacteristic);
            ArgumentNullException.ThrowIfNull(responseCharacteristic);
            ArgumentNullException.ThrowIfNull(listeningCharacteristic);

            if (!CheckParent(device, commandCharacteristic))
                throw new ArgumentException($"{nameof(commandCharacteristic)} does not belong to the specified device",
                    nameof(commandCharacteristic));
            if (!CheckParent(device, responseCharacteristic))
                throw new ArgumentException($"{nameof(responseCharacteristic)} does not belong to the specified device",
                    nameof(responseCharacteristic));
            if (!CheckParent(device, listeningCharacteristic))
                throw new ArgumentException($"{nameof(listeningCharacteristic)} does not belong to the specified device",
                    nameof(listeningCharacteristic));

            if (!commandCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Write) &&
                !commandCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse))
                throw new ArgumentException(
                    $"{nameof(commandCharacteristic)} is neither Write nor Write without response.",
                    nameof(commandCharacteristic));
            if (!responseCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Notify) &&
                !responseCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Indicate))
                throw new ArgumentException(
                    $"{nameof(responseCharacteristic)} is neither Update nor Indicate.",
                    nameof(responseCharacteristic));
            if (!listeningCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Notify) &&
                !listeningCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Indicate))
                throw new ArgumentException(
                    $"{nameof(listeningCharacteristic)} is neither Update nor Indicate.",
                    nameof(listeningCharacteristic));

            if (responseCharacteristic.TokenAggregator != null)
                throw new ArgumentException(
                    $"{nameof(responseCharacteristic)} has attached TokenAggregator already.",
                    nameof(responseCharacteristic));
            if (listeningCharacteristic.TokenAggregator != null)
                throw new ArgumentException(
                    $"{nameof(listeningCharacteristic)} has attached TokenAggregator already.",
                    nameof(listeningCharacteristic));

            Device = device;
            Service = service;
            CommandCharacteristic = commandCharacteristic;
            ResponseCharacteristic = responseCharacteristic;
            ListeningCharacteristic = listeningCharacteristic;
            TokenDelimiter = tokenDelimiter;

            if (ResponseCharacteristic == ListeningCharacteristic)
            {
                ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator());
            }
            else
            {
                ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator());
                ListeningCharacteristic.AttachTokenAggregator(new TokenAggregator());
            }
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

        private static bool CheckParent(IDevice<BluetoothLEDevice, Service> device,
            ICharacteristic<GattCharacteristic> characteristic)
        {
            var nativeParent = device?.NativeDevice;
            var nativeCharacteristic = characteristic?.NativeCharacteristic;
            return nativeParent == nativeCharacteristic?.Service?.Device;
        }
    }
}
