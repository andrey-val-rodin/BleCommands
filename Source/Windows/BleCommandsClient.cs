namespace BleCommands.Windows
{
    public static class BleCommandsClient
    {
        public static readonly Guid ServiceUuid                 = new("DB341FB3-8977-4C2D-AC6C-74540BD8B901");
        public static readonly Guid CommandCharacteristicUuid   = new("DB341FB3-8977-4C2D-AC6C-74540BD8B902");
        public static readonly Guid ResponseCharacteristicUuid  = new("DB341FB3-8977-4C2D-AC6C-74540BD8B903");
        public static readonly Guid ListeningCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B904");

        public static async Task<BleTransport?> CreateTransportAsync(string deviceName, CancellationToken token = default)
        {
            if (!await BluetoothHelper.IsBluetoothAvailableAsync() ||
                !await BluetoothHelper.IsBluetoothOnAsync())
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
                // Unable to get characteristics
                commandCharacteristic?.Dispose();
                responseCharacteristic?.Dispose();
                listeningCharacteristic?.Dispose();
                device.Dispose();
                service.Dispose();
                return null;
            }

            return new BleTransport(device, service, commandCharacteristic, responseCharacteristic, listeningCharacteristic);
        }

        private static async Task<Device?> CreateDeviceAsync(string deviceName, CancellationToken token)
        {
            using var scanner = new BleScanner();
            var device = await scanner.FindDeviceAsync(deviceName);
            if (device == null)
            {
                // Device not found
                return null;
            }

            await device.ConnectAsync(token);
            return device;
        }
    }
}
