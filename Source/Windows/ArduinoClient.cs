namespace BleCommands.Windows
{
    /// <summary>
    /// A client for working together with the accompanying BLECommands.Arduino library.
    /// </summary>
    public static class ArduinoClient
    {
        /// <summary>
        /// The service UUID.
        /// </summary>
        public static readonly Guid ServiceUuid                 = new("DB341FB3-8977-4C2D-AC6C-74540BD8B901");
        /// <summary>
        /// The command characteristic UUID.
        /// </summary>
        public static readonly Guid CommandCharacteristicUuid   = new("DB341FB3-8977-4C2D-AC6C-74540BD8B902");
        /// <summary>
        /// The response characteristic UUID.
        /// </summary>
        public static readonly Guid ResponseCharacteristicUuid  = new("DB341FB3-8977-4C2D-AC6C-74540BD8B903");
        /// <summary>
        /// The listening characteristic UUID.
        /// </summary>
        public static readonly Guid ListeningCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B904");

        /// <summary>
        /// Creates BleTransport object.
        /// </summary>
        /// <param name="deviceName">A device name.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A BleTransport object, or <c>null</c> if something went wrong.</returns>
        /// <remarks>
        /// Be sure to call the Dispose method or use the using statement on the transport object
        /// to release all system resources.
        /// </remarks>
        public static async Task<BleTransport?> CreateTransportAsync(string deviceName, CancellationToken token = default)
        {
            if (!await BluetoothHelper.IsBluetoothAvailableAsync().ConfigureAwait(false) ||
                !await BluetoothHelper.IsBluetoothOnAsync().ConfigureAwait(false))
                return null;

            var device = await CreateDeviceAsync(deviceName, token).ConfigureAwait(false);
            if (device == null)
                return null;

            var service = await device.GetServiceAsync(ServiceUuid, token).ConfigureAwait(false);
            if (service == null)
            {
                device.Dispose();
                return null;
            }

            var commandCharacteristic = await service.GetCharacteristicAsync(
                CommandCharacteristicUuid, token).ConfigureAwait(false);
            var responseCharacteristic = await service.GetCharacteristicAsync(
                ResponseCharacteristicUuid, token).ConfigureAwait(false);
            var listeningCharacteristic = await service.GetCharacteristicAsync(
                ListeningCharacteristicUuid, token).ConfigureAwait(false);

            if (commandCharacteristic == null ||
                responseCharacteristic == null ||
                listeningCharacteristic == null)
            {
                device.Dispose();
                return null;
            }

            return new BleTransport(device, service, commandCharacteristic, responseCharacteristic, listeningCharacteristic);
        }

        private static async Task<Device?> CreateDeviceAsync(string deviceName, CancellationToken token)
        {
            using var scanner = new BleScanner();
            var device = await scanner.FindDeviceAsync(deviceName).ConfigureAwait(false);
            if (device == null)
            {
                // Device not found
                return null;
            }

            await device.ConnectAsync(token).ConfigureAwait(false);
            return device;
        }
    }
}
