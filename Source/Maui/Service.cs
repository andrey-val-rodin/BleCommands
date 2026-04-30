using BleCommands.Core.Contracts;
using NativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace BleCommands.Maui
{
    /// <summary>
    /// MAUI implementation of <see cref="IService{TService,TCharacteristic}"/>
    /// using the Plugin.BLE abstraction layer.
    /// </summary>
    public class Service : IService<NativeService, Characteristic>
    {
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="nativeService">
        /// The native <see cref="Plugin.BLE.Abstractions.Contracts.IService"/> instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nativeService"/> is <c>null</c>.
        /// </exception>
        public Service(NativeService nativeService)
        {
            NativeService = nativeService ?? throw new ArgumentNullException(nameof(nativeService));
            Id = NativeService.Id;
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
        public Guid Id { get; private set; }

        /// <inheritdoc/>
        public NativeService NativeService { get; }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the service has been disposed.
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth-level errors.</exception>
        public async Task<Characteristic?> GetCharacteristicAsync(
            Guid id, CancellationToken token = default)
        {
            ThrowIfDisposed();

            var nativeCharacteristic = await NativeService.GetCharacteristicAsync(id, token);
            return nativeCharacteristic == null ? null : new Characteristic(nativeCharacteristic);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the service has been disposed.
        /// </exception>
        /// <exception cref="Exception">Thrown on Bluetooth-level errors.</exception>
        public async Task<IReadOnlyList<Characteristic>> GetCharacteristicsAsync(
            CancellationToken token = default)
        {
            ThrowIfDisposed();

            var nativeCharacteristics = await NativeService.GetCharacteristicsAsync(token);
            return nativeCharacteristics == null
                ? new List<Characteristic>()
                : nativeCharacteristics.Select(static c => new Characteristic(c)).ToList();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(typeof(Service).FullName);
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
