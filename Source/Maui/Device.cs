using BleCommands.Core.Contracts;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Contracts;
using INativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;
using INativeService = Plugin.BLE.Abstractions.Contracts.IService;
using INativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;
using IService = BleCommands.Core.Contracts.IService<
    Plugin.BLE.Abstractions.Contracts.IService,
    Plugin.BLE.Abstractions.Contracts.ICharacteristic>;

namespace BleCommands.Maui
{
    /// <summary>
    /// MAUI implementation of a Bluetooth Low Energy device.
    /// </summary>
    public class Device : IDevice<INativeDevice, INativeService, INativeCharacteristic>
    {
        private readonly Guid? _guid;
        private bool _connectionInvoked;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disconnected;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public event EventHandler? Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using device Guid.
        /// </summary>
        /// <param name="guid">A device Guid</param>
        /// <remarks>
        /// The <see cref="ConnectAsync(CancellationToken)"/> method will use
        /// <see cref="IAdapter.ConnectToKnownDeviceAsync(Guid, ConnectParameters, CancellationToken)"/>
        /// The recommended way for obtaining a device is using <see cref="BleScanner"/>.
        /// </remarks>
        public Device(Guid guid)
        {
            _guid = guid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using native device.
        /// The native device object is passed to the <see cref="IAdapter.DeviceDiscovered"/> event handler.
        /// </summary>
        public Device(INativeDevice nativeDevice)
        {
            NativeDevice = nativeDevice ?? throw new ArgumentNullException(nameof(nativeDevice));
        }

        public string Id => NativeDevice?.Id.ToString() ?? string.Empty;

        public string Name => NativeDevice?.Name ?? string.Empty;

        /// <summary>
        /// Gets platform-specific device
        /// </summary>
        public INativeDevice? NativeDevice { get; private set; }

        private static IAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        /// <summary>
        /// Initiates process of connection to the device.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the operation.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="DeviceConnectionException">Thrown on device connection errors.</exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task ConnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            await _semaphore.WaitAsync(token);
            try
            {
                if (_connectionInvoked)
                    return;

                // Different events can be fired on different platforms
                Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
                Adapter.DeviceConnectionLost += Adapter_DeviceDisconnected;

                if (NativeDevice != null)
                {
                    await ConnectAsync(NativeDevice, token).ConfigureAwait(false);
                }
                else if (_guid != null)
                {
                    await ConnectAsync(_guid.Value, token).ConfigureAwait(false);
                }

                _connectionInvoked = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            if (e.Device?.Id == NativeDevice?.Id)
            {
                lock (_lock)
                {
                    if (_disconnected)
                        return; // Disconnected event fired already

                    NativeDevice?.Dispose();
                    NativeDevice = null;
                    Disconnected?.Invoke(this, e);
                    _disconnected = true;
                }
            }
        }

        private static async Task ConnectAsync(
            INativeDevice nativeDevice, CancellationToken token = default)
        {
            var parameters = new ConnectParameters(false, forceBleTransport: true);
            await Adapter.ConnectToDeviceAsync(nativeDevice, parameters, token);
        }

        private async Task ConnectAsync(Guid guid, CancellationToken token = default)
        {
            NativeDevice = await Adapter.ConnectToKnownDeviceAsync(guid,
                new ConnectParameters(false, forceBleTransport: true), token);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (NativeDevice == null)
                throw new InvalidOperationException("Device not connected.");

            var nativeServices = await NativeDevice.GetServicesAsync(token);
            return nativeServices == null
                ? new List<IService>()
                : nativeServices.Select(s => new Service(s)).ToList<IService>();
        }

        /// <inheritdoc/>
        public async Task<IService?> GetServiceAsync(Guid id, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (NativeDevice == null)
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
                    Adapter.DeviceDisconnected -= Adapter_DeviceDisconnected;
                    Adapter.DeviceConnectionLost -= Adapter_DeviceDisconnected;
                    NativeDevice?.Dispose();
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