using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using NativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;
using NativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using NativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace IntegrationTests.Maui
{
    /// <inheritdoc />
    public partial class RotatingTableTransport : BleTransport<
        IDevice<NativeDevice, Service>,
        IService<NativeService, Characteristic>,
        ICharacteristic<NativeCharacteristic>>
    {
        /// <summary>
        /// A constructor.
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
        public RotatingTableTransport(
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

        public override async Task StartAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (IsStarted)
                return;

            // Rotating Table has only one Update characteristic
            Assert.AreEqual(ResponseCharacteristic, ListeningCharacteristic);

            ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator(TokenDelimiter));
            await ResponseCharacteristic.StartReceivingAsync(token).ConfigureAwait(false);

            IsStarted = true;
        }
    }
}
