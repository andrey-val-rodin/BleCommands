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
        private readonly List<IDisposable> _children = new();
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

            var gattResultresult = await NativeService.GetCharacteristicsForUuidAsync(id)
                .AsTask(token);
            gattResultresult.ThrowIfError();

            var nativeCharacteristic = gattResultresult.Characteristics.Count > 0 ? gattResultresult.Characteristics[0] : null;
            if (nativeCharacteristic == null)
                return null;

            var result = new Characteristic(nativeCharacteristic);
            ((IChildDisposer)this).RegisterChild(result);

            return result;
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

            var gattResult = await NativeService.GetCharacteristicsAsync()
                .AsTask(token);
            gattResult.ThrowIfError();
            var nativeCharacteristics = gattResult.Characteristics;

            var result = nativeCharacteristics == null
                ? new List<Characteristic>()
                : nativeCharacteristics.Select(static c => new Characteristic(c)).ToList();

            foreach (var characteristic in result)
            {
                ((IChildDisposer)this).RegisterChild(characteristic);
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    NativeService?.Dispose();

                    foreach (var child in _children)
                    {
                        child?.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Explicit interface implementation. Registers a child element for future disposing.
        /// </summary>
        /// <param name="child">A child.</param>
        void IChildDisposer.RegisterChild(IDisposable child)
        {
            _children.Add(child);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
