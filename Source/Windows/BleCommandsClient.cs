using BleCommands.Core.Contracts;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    public class BleCommandsClient : IDisposable
    {
        public static readonly Guid ServiceUuid                 = new("DB341FB3-8977-4C2D-AC6C-74540BD8B901");
        public static readonly Guid CommandCharacteristicUuid   = new("DB341FB3-8977-4C2D-AC6C-74540BD8B902");
        public static readonly Guid ResponseCharacteristicUuid  = new("DB341FB3-8977-4C2D-AC6C-74540BD8B903");
        public static readonly Guid ListeningCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B904");
        private bool _disposed;

        public async Task<bool> BeginAsync(string deviceName, CancellationToken token = default)
        {
            if (!await BluetoothHelper.IsBluetoothAvailableAsync() ||
                !await BluetoothHelper.IsBluetoothOnAsync())
                return false;

            using var scanner = new BleScanner();
            var device = await scanner.FindDeviceAsync(deviceName, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (device == null)
            {
                // Device not found
                return false;
            }

            if (!await device.ConnectAsync(token))
            {
                // Unable to connect
                return false;
            }
            Device = device;

            Service = await device.GetServiceAsync(ServiceUuid, token).ConfigureAwait(false);
            if (Service == null)
            {
                // Unable to get service
                return false;
            }

            var commandCharacteristic = await Service.GetCharacteristicAsync(CommandCharacteristicUuid).ConfigureAwait(false);
            var responseCharacteristic = await Service.GetCharacteristicAsync(ResponseCharacteristicUuid).ConfigureAwait(false);
            var listeningCharacteristic = await Service.GetCharacteristicAsync(ListeningCharacteristicUuid).ConfigureAwait(false);

            if (commandCharacteristic == null ||
                responseCharacteristic == null ||
                listeningCharacteristic == null)
            {
                // Unable to get characteristics
                return false;
            }

            Transport = new BleTransport(commandCharacteristic, responseCharacteristic, listeningCharacteristic);
            await Transport.BeginAsync(token);

            return true;
        }

        public TimeSpan ScanTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>? Device { get; private set; }

        public IService<GattDeviceService, GattCharacteristic>? Service { get; private set; }

        public BleTransport? Transport { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Device?.Dispose();
                    Service?.Dispose();
                    Transport?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
