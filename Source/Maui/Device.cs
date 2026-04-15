using Core.Exceptions;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using IDevice = BleCommands.Core.Contracts.IDevice<
    Plugin.BLE.Abstractions.Contracts.IDevice,
    Plugin.BLE.Abstractions.Contracts.IService,
    Plugin.BLE.Abstractions.Contracts.ICharacteristic>;
using INativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using IService = BleCommands.Core.Contracts.IService<
    Plugin.BLE.Abstractions.Contracts.IService,
    Plugin.BLE.Abstractions.Contracts.ICharacteristic>;

namespace BleCommands.Maui
{
    public class Device : IDevice
    {
        private readonly string? _id;
        private bool _disposed = false;

        public Device(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (Guid.TryParse(id, out Guid _))
                throw new ArgumentException("The specified id is not Guid.");

            _id = id;
        }

        internal Device(INativeDevice nativeDevice)
        {
            NativeDevice = nativeDevice ?? throw new ArgumentNullException(nameof(nativeDevice));
        }

        public string Id => _id ?? NativeDevice?.Id.ToString() ?? string.Empty;

        public string Name => NativeDevice?.Name ?? string.Empty;

        /// <summary>
        /// Gets platform-specific device
        /// </summary>
        public INativeDevice? NativeDevice { get; private set; }

        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        public bool IsConnected { get; private set; }

        public async Task<bool> ConnectAsync(CancellationToken token = default)
        {
            if (NativeDevice != null)
            {
                IsConnected = await ConnectAsync(NativeDevice, token).ConfigureAwait(false);
            }
            else if (_id != null)
            {
                IsConnected = await ConnectAsync(_id, token).ConfigureAwait(false);
            }

            return IsConnected;
        }

        private static async Task<bool> ConnectAsync(
            INativeDevice nativeDevice, CancellationToken token = default)
        {
            var parameters = new ConnectParameters(false, forceBleTransport: true);
            try
            {
                await Adapter.ConnectToDeviceAsync(nativeDevice, parameters, token);
                return true;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error.", ex);
            }
        }

        private async Task<bool> ConnectAsync(string id, CancellationToken token = default)
        {
            try
            {
                Guid guid = Guid.Parse(id);
                NativeDevice = await Adapter.ConnectToKnownDeviceAsync(guid,
                    new ConnectParameters(false, forceBleTransport: true), token);
                return true;
            }
            catch (Exception ex)
            {
                throw new DeviceException("Device connection error.", ex);
            }
        }

        public async Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!IsConnected || NativeDevice == null)
                throw new InvalidOperationException("Device not connected.");

            var nativeServices = await NativeDevice.GetServicesAsync(token);
            return nativeServices == null
                ? new List<IService>()
                : nativeServices.Select(s => new Service(s)).ToList<IService>();
        }

        public async Task<IService?> GetServiceAsync(Guid id, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!IsConnected || NativeDevice == null)
                throw new InvalidOperationException("Device not connected.");

            var nativeService = await NativeDevice.GetServiceAsync(id, token);
            return nativeService == null ? null : new Service(nativeService);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Device));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    NativeDevice?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}