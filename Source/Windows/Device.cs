using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using BleCommands.Windows.Extensions;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    /// <summary>
    /// Windows implementation of <see cref="IDevice{TNativeDevice, TService}"/>
    /// using the Windows.Devices.Bluetooth abstraction layer.
    /// </summary>
    public class Device : IDevice<BluetoothLEDevice, Service>
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
        /// The <see cref="ConnectAsync"/> method will fail
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

        /// <inheritdoc/>
        public bool IsConnected => NativeDevice?.ConnectionStatus == BluetoothConnectionStatus.Connected;

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
        /// <exception cref="DeviceException">Thrown on GATT-protocol errors.</exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task ConnectAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

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

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ConnectAsync"/> has not been called
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task<IReadOnlyList<Service>> GetServicesAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (NativeDevice == null)
                throw new InvalidOperationException("Device is not connected.");

            var result = await NativeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached)
                .AsTask(token);
            result.ThrowIfError();
            var nativeServices = result.Services;

            return nativeServices == null
                ? new List<Service>()
                : nativeServices.Select(s => new Service(s)).ToList();
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ConnectAsync"/> has not been called
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task<Service?> GetServiceAsync(Guid id, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
