using BleCommands.Core.Contracts;
using BleCommands.Core.Exceptions;
using BleCommands.Windows.Extensions;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    /// <summary>
    /// Windows implementation of <see cref="IService{TNativeService, TCharacteristic}"/>
    /// using the Windows.Devices.Bluetooth.GenericAttributeProfile abstraction layer.
    /// </summary>
    public class Service : IService<GattDeviceService, Characteristic>
    {
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="nativeService">The native <see cref="GattDeviceService"/> instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nativeService"/> is <c>null</c>.
        /// </exception>
        public Service(GattDeviceService nativeService)
        {
            NativeService = nativeService ?? throw new ArgumentNullException(nameof(nativeService));
            Id = NativeService.Uuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class for testing purposes.
        /// </summary>
        /// <remarks>
        /// This constructor is intended for unit testing only. It creates a characteristic
        /// without requiring an actual Bluetooth connection.
        /// </remarks>
        internal Service()
        {
            NativeService = null!;
            Id = Guid.Empty;
        }

        /// <inheritdoc/>
        public Guid Id { get; private init; }

        /// <inheritdoc/>
        public GattDeviceService NativeService { get; }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the service has been disposed.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on GATT-protocol errors.</exception>
        /// <exception cref="Exception">Thrown on Bluetooth-level errors.</exception>
        public async Task<Characteristic?> GetCharacteristicAsync(
            Guid id, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var result = await NativeService.GetCharacteristicsForUuidAsync(id)
                .AsTask(token);
            result.ThrowIfError();
            var nativeCharacteristic = result.Characteristics.Count > 0 ? result.Characteristics[0] : null;

            return nativeCharacteristic == null ? null : new Characteristic(nativeCharacteristic);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the service has been disposed.
        /// </exception>
        /// <exception cref="DeviceException">Thrown on GATT-protocol errors.</exception>
        /// <exception cref="Exception">Thrown on Bluetooth-level errors.</exception>
        public async Task<IReadOnlyList<Characteristic>> GetCharacteristicsAsync(
            CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var result = await NativeService.GetCharacteristicsAsync()
                .AsTask(token);
            result.ThrowIfError();
            var nativeCharacteristics = result.Characteristics;

            return nativeCharacteristics == null
                ? new List<Characteristic>()
                : nativeCharacteristics.Select(static c => new Characteristic(c)).ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    NativeService?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
