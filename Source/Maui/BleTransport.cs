using BleCommands.Core;
using BleCommands.Core.Contracts;
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
            IDevice<NativeDevice, Service> device,
            IService<NativeService, Characteristic> service,
            ICharacteristic<NativeCharacteristic> commandCharacteristic,
            ICharacteristic<NativeCharacteristic> responseCharacteristic,
            ICharacteristic<NativeCharacteristic> listeningCharacteristic,
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
