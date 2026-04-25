using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using BleCommands.Windows.Extensions;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    /// <summary>
    /// Windows implementation of a Bluetooth Low Energy device.
    /// </summary>
    public class Device : IDevice<BluetoothLEDevice, GattDeviceService, GattCharacteristic>
    {
        private readonly ulong _bluetoothAddress;
        private GattSession? _gattSession;
        private bool _disposed = false;

        /// <inheritdoc/>
        public event EventHandler? Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using a device identifier.
        /// </summary>
        /// <param name="id">
        /// The device identifier string.
        /// For the first time, this value can be obtained from the device data returned by BleScanner.
        /// </param>
        /// <remarks>
        /// The <see cref="ConnectAsync(CancellationToken)"/> method will fail
        /// if the device isn't paired and it isn't found in the system cache.
        /// The recommended way for obtaining a device is using <see cref="BleScanner"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null or whitespace.</exception>
        public Device(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using a Bluetooth address.
        /// </summary>
        /// <param name="bluetoothAddress">
        /// The Bluetooth address of the device.
        /// For the first time, this value can be obtained from the device data returned by BleScanner.
        /// </param>
        /// <remarks>
        /// The <see cref="ConnectAsync(CancellationToken)"/> method will fail
        /// if the device isn't paired and it isn't found in the system cache.
        /// The recommended way for obtaining a device is using <see cref="BleScanner"/>.
        /// </remarks>
        public Device(ulong bluetoothAddress)
        {
            Id = string.Empty;
            _bluetoothAddress = bluetoothAddress;
        }

        /// <inheritdoc/>
        public string Id { get; protected set; }

        /// <inheritdoc/>
        public string Name => NativeDevice?.Name ?? string.Empty;

        /// <inheritdoc/>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// Gets the platform-specific Windows Bluetooth LE device.
        /// </summary>
        public BluetoothLEDevice? NativeDevice { get; protected set; }

        /// <summary>
        /// Connects to the device.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>True if the device is successfully connected; false otherwise.</returns>
        /// <remarks>
        /// This method must be called from a UI thread.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="DeviceException">Thrown on Bluetooth errors.</exception>
        public async Task<bool> ConnectAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (IsConnected)
                return true;

            NativeDevice = await GetNativeDeviceAsync();
            if (NativeDevice == null)
                return false;

            // Create and configure GATT session to maintain connection
            _gattSession = await GattSession.FromDeviceIdAsync(NativeDevice.BluetoothDeviceId);
            _gattSession.MaintainConnection = true;

            // Monitor connection status
            NativeDevice.ConnectionStatusChanged += NativeDevice_ConnectionStatusChanged;

            // Retrieve services to establish actual connection
            var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            result.ThrowIfError();

            // Dispose obtained services; Windows cache will keep them
            if (result.Services != null)
            {
                foreach (var service in result.Services)
                {
                    service.Dispose();
                }
            }

            IsConnected = NativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
            // Refresh Id with actual value
            Id = NativeDevice.DeviceId;
            return IsConnected;
        }

        protected async Task<BluetoothLEDevice> GetNativeDeviceAsync()
        {
            return string.IsNullOrEmpty(Id)
                ? await BluetoothLEDevice.FromBluetoothAddressAsync(_bluetoothAddress)
                : await BluetoothLEDevice.FromIdAsync(Id);
        }

        protected void NativeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            IsConnected = sender.ConnectionStatus == BluetoothConnectionStatus.Connected;
            if (!IsConnected)
                Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public async Task<IService<GattDeviceService, GattCharacteristic>?> GetServiceAsync(
            Guid id, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            try
            {
                var result = await NativeDevice.GetGattServicesForUuidAsync(id, BluetoothCacheMode.Cached);
                result.ThrowIfError();
                var nativeService = result.Services?.Count > 0 ? result.Services[0] : null;

                return nativeService == null ? null : new Service(nativeService);
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get service.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IService<GattDeviceService, GattCharacteristic>>> GetServicesAsync(
            CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            try
            {
                var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                result.ThrowIfError();
                var nativeServices = result.Services;

                return nativeServices == null
                    ? new List<IService<GattDeviceService, GattCharacteristic>>()
                    : nativeServices.Select(s => new Service(s)).ToList();
            }
            catch (Exception ex)
            {
                throw new DeviceException("Failed to get services.", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (NativeDevice != null)
                    {
                        NativeDevice.ConnectionStatusChanged -= NativeDevice_ConnectionStatusChanged;
                        NativeDevice.Dispose();
                    }

                    _gattSession?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the device.
        /// Don't forget to call Dispose or use a using statement to free all system resources of the BLE device.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
