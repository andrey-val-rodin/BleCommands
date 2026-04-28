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
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;

        /// <inheritdoc/>
        public event EventHandler? Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using a Bluetooth address.
        /// </summary>
        /// <param name="bluetoothAddress">The Bluetooth address of the device.</param>
        /// <remarks>
        /// The <see cref="ConnectAsync(CancellationToken)"/> method will fail
        /// if the device isn't paired and it isn't found in the system cache.
        /// The recommended way for obtaining a device is using <see cref="BleScanner"/>.
        /// </remarks>
        public Device(ulong bluetoothAddress)
        {
            _bluetoothAddress = bluetoothAddress;
        }

        /// <inheritdoc/>
        public string Id => NativeDevice?.DeviceId ?? string.Empty;

        /// <inheritdoc/>
        public string Name => NativeDevice?.Name ?? string.Empty;

        /// <summary>
        /// Gets the platform-specific Windows Bluetooth LE device.
        /// </summary>
        public BluetoothLEDevice? NativeDevice { get; protected set; }

        /// <summary>
        /// Initiates process of connection to the device.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <remarks>
        /// This method must be called from a UI thread.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="DeviceException">Thrown on device connection errors.</exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task ConnectAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            await _semaphore.WaitAsync(token);
            try
            {
                if (NativeDevice != null)
                    return;

                NativeDevice = await BluetoothLEDevice
                    .FromBluetoothAddressAsync(_bluetoothAddress)
                    .AsTask(token);

                if (NativeDevice == null)
                    throw new DeviceException("Unable to find the device identified by bluetooth address " +
                        $"{_bluetoothAddress}. Specifically, if the device isn't paired " +
                        "and it isn't found in the system cache.");

                // Create and configure GATT session to maintain connection
                _gattSession = await GattSession.FromDeviceIdAsync(NativeDevice.BluetoothDeviceId)
                    .AsTask(token);
                _gattSession.MaintainConnection = true;

                // Monitor connection status
                NativeDevice.ConnectionStatusChanged += NativeDevice_ConnectionStatusChanged;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void NativeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Retrieves all GATT services available on the device asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>A read-only list of services exposed by the device.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ConnectAsync(CancellationToken)"/> has not been called
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task<IReadOnlyList<IService<GattDeviceService, GattCharacteristic>>> GetServicesAsync(
            CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached)
                .AsTask(token);
            result.ThrowIfError();
            var nativeServices = result.Services;

            return nativeServices == null
                ? new List<IService<GattDeviceService, GattCharacteristic>>()
                : nativeServices.Select(s => new Service(s)).ToList();
        }

        /// <summary>
        /// Retrieves a specific GATT service by its UUID asynchronously.
        /// </summary>
        /// <param name="id">The UUID of the service to retrieve.</param>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <returns>The requested service, or null if not found.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ConnectAsync(CancellationToken)"/> has not been called
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task<IService<GattDeviceService, GattCharacteristic>?> GetServiceAsync(
            Guid id, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Device));

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            var result = await NativeDevice.GetGattServicesForUuidAsync(id, BluetoothCacheMode.Cached)
                .AsTask(token);
            result.ThrowIfError();
            var nativeService = result.Services?.Count > 0 ? result.Services[0] : null;

            return nativeService == null ? null : new Service(nativeService);
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
                        NativeDevice = null;
                    }

                    _gattSession?.Dispose();
                    _semaphore?.Dispose();
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
