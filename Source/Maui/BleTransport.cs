using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using NativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;
using NativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using NativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace BleCommands.Maui
{
    /// <inheritdoc />
    public class BleTransport : BleTransport<
        IDevice<NativeDevice, Service>,
        IService<NativeService, Characteristic>,
        ICharacteristic<NativeCharacteristic>>
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
            IDevice<NativeDevice, Service> device,
            IService<NativeService, Characteristic> service,
            ICharacteristic<NativeCharacteristic> commandCharacteristic,
            ICharacteristic<NativeCharacteristic> responseCharacteristic,
            ICharacteristic<NativeCharacteristic> listeningCharacteristic,
            char tokenDelimiter = TokenAggregator.DefaultTokenDelimiter)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            if (commandCharacteristic == null) throw new ArgumentNullException(nameof(commandCharacteristic));
            if (responseCharacteristic == null) throw new ArgumentNullException(nameof(responseCharacteristic));
            if (listeningCharacteristic == null) throw new ArgumentNullException(nameof(listeningCharacteristic));

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

            CommandCharacteristic = commandCharacteristic;
            ResponseCharacteristic = responseCharacteristic;
            ListeningCharacteristic = listeningCharacteristic;
            TokenDelimiter = tokenDelimiter;

            if (ReferenceEquals(ResponseCharacteristic, ListeningCharacteristic))
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
        public override IDevice<NativeDevice, Service> Device { get; }

        /// <inheritdoc />
        public override IService<NativeService, Characteristic> Service { get; }

        /// <inheritdoc />
        public override ICharacteristic<NativeCharacteristic> CommandCharacteristic { get; }

        /// <inheritdoc />
        public override ICharacteristic<NativeCharacteristic> ResponseCharacteristic { get; }

        /// <inheritdoc />
        public override ICharacteristic<NativeCharacteristic> ListeningCharacteristic { get; }
    }
}
