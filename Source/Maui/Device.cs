using BleCommands.Core.Contracts;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using NativeDevice = Plugin.BLE.Abstractions.Contracts.IDevice;

namespace BleCommands.Maui
{
    /// <summary>
    /// MAUI implementation of <see cref="IDevice{TNativeDevice, TService}"/>
    /// using the Plugin.BLE abstraction layer.
    /// </summary>
    public class Device : IDevice<NativeDevice, Service>
    {
        private readonly Guid? _guid;
        private bool _connectionInvoked;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disconnected;
        private readonly object _lock = new();
        private bool _disposed = false;

        /// <inheritdoc/>
        public event EventHandler? Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using device Guid.
        /// </summary>
        /// <param name="guid">A device Guid</param>
        /// <remarks>
        /// The <see cref="ConnectAsync"/> method will use
        /// <see cref="IAdapter.ConnectToKnownDeviceAsync"/>
        /// The recommended way for obtaining a device is using <see cref="BleScanner"/>.
        /// </remarks>
        public Device(Guid guid)
            : this(guid, Plugin.BLE.CrossBluetoothLE.Current.Adapter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class using native device.
        /// The native device object is passed to the <see cref="IAdapter.DeviceDiscovered"/> event handler.
        /// </summary>
        public Device(NativeDevice nativeDevice)
            : this(nativeDevice, Plugin.BLE.CrossBluetoothLE.Current.Adapter)
        {
        }

        internal Device(Guid guid, IAdapter adapter)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _guid = guid;
        }

        internal Device(NativeDevice nativeDevice, IAdapter adapter)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            NativeDevice = nativeDevice ?? throw new ArgumentNullException(nameof(nativeDevice));
        }

        /// <inheritdoc/>
        public string Id => NativeDevice?.Id.ToString() ?? string.Empty;

        /// <inheritdoc/>
        public string Name => NativeDevice?.Name ?? string.Empty;

        /// <inheritdoc/>
        public bool IsConnected => NativeDevice?.State == DeviceState.Connected;

        /// <summary>
        /// Gets platform-specific device
        /// </summary>
        public NativeDevice? NativeDevice { get; private set; }

        internal IAdapter Adapter { get; private set; }

        /// <summary>
        /// Initiates process of connection to the device.
        /// </summary>
        /// <remarks>The connection will be established shortly.</remarks>
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

                    Disconnected?.Invoke(this, e);
                    _disconnected = true;
                }
            }
        }

        private async Task ConnectAsync(
            NativeDevice nativeDevice, CancellationToken token = default)
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
        /// <exception cref="ObjectDisposedException">Thrown when the device has been disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ConnectAsync"/> has not been called
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth errors.</exception>
        public async Task<IReadOnlyList<Service>> GetServicesAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (NativeDevice == null)
                throw new InvalidOperationException("Device not connected.");

            var nativeServices = await NativeDevice.GetServicesAsync(token);
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
            ThrowIfDisposed();

            if (NativeDevice == null)
                throw new InvalidOperationException("Device not connected.");

            var nativeService = await NativeDevice.GetServiceAsync(id, token);
            return nativeService == null ? null : new Service(nativeService);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(typeof(Device).FullName);
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