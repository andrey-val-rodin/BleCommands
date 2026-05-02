namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a Bluetooth GATT service.
    /// </summary>
    public interface IService : IDisposable, IChildDisposer
    {
        /// <summary>
        /// Gets the unique identifier (UUID) of the service.
        /// </summary>
        Guid Id { get; }
    }

    /// <summary>
    /// Represents a generic Bluetooth GATT service.
    /// </summary>
    /// <typeparam name="TNativeService">The underlying platform-specific service type.</typeparam>
    /// <typeparam name="TCharacteristic">
    /// A specific characteristic implementation.
    /// </typeparam>
    public interface IService<TNativeService, TCharacteristic> : IService
        where TCharacteristic : ICharacteristic
    {
        /// <summary>
        /// Gets the native platform-specific service object.
        /// </summary>
        TNativeService NativeService { get; }

        /// <summary>
        /// Retrieves a read-only list of all characteristics belonging to this service.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of characteristics.
        /// </returns>
        Task<IReadOnlyList<TCharacteristic>> GetCharacteristicsAsync(CancellationToken token = default);

        /// <summary>
        /// Retrieves a specific characteristic by its UUID.
        /// </summary>
        /// <param name="id">The UUID of the characteristic to retrieve.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the characteristic if found;
        /// otherwise, <c>null</c>.
        /// </returns>
        Task<TCharacteristic?> GetCharacteristicAsync(Guid id, CancellationToken token = default);
    }
}
