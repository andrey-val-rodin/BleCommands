namespace BleCommands.Windows
{
    public static class BleCommandsClient
    {
        public static readonly Guid ServiceUuid                 = new("DB341FB3-8977-4C2D-AC6C-74540BD8B901");
        public static readonly Guid CommandCharacteristicUuid   = new("DB341FB3-8977-4C2D-AC6C-74540BD8B902");
        public static readonly Guid ResponseCharacteristicUuid  = new("DB341FB3-8977-4C2D-AC6C-74540BD8B903");
        public static readonly Guid ListeningCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B904");

        public static async Task<BleTransportHolder?> CreateTransportAsync(string deviceName, CancellationToken token = default)
        {
            if (!await BluetoothHelper.IsBluetoothAvailableAsync() ||
                !await BluetoothHelper.IsBluetoothOnAsync())
                return null;

            var device = await CreateDeviceAsync(deviceName, token).ConfigureAwait(false);
            if (device == null)
                return null;

            var service = await CreateServiceAsync(device, token).ConfigureAwait(false);
            if (service == null)
            {
                device.Dispose();
                return null;
            }

            var commandCharacteristic = await service.GetCharacteristicAsync(CommandCharacteristicUuid).ConfigureAwait(false);
            var responseCharacteristic = await service.GetCharacteristicAsync(ResponseCharacteristicUuid).ConfigureAwait(false);
            var listeningCharacteristic = await service.GetCharacteristicAsync(ListeningCharacteristicUuid).ConfigureAwait(false);

            if (commandCharacteristic == null ||
                responseCharacteristic == null ||
                listeningCharacteristic == null)
            {
                // Unable to get characteristics
                device.Dispose();
                service.Dispose();
                return null;
            }

            var transport = new BleTransport(commandCharacteristic, responseCharacteristic, listeningCharacteristic);
            return new BleTransportHolder(device, service, transport);
        }

        private static async Task<Device?> CreateDeviceAsync(string deviceName, CancellationToken token)
        {
            using var scanner = new BleScanner();
            var device = await scanner.FindDeviceAsync(deviceName, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (device == null)
            {
                // Device not found
                return null;
            }

            if (!await device.ConnectAsync(token).ConfigureAwait(false))
            {
                // Unable to connect
                return null;
            }

            return device as Device;
        }

        private static async Task<Service?> CreateServiceAsync(Device device, CancellationToken token)
        {
            var service = await device.GetServiceAsync(ServiceUuid, token).ConfigureAwait(false);
            if (service == null)
            {
                // Unable to get service
                device.Dispose();
                return null;
            }

            return service as Service;
        }
    }
}
