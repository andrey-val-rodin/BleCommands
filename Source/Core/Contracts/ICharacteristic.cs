using BleCommands.Core.Enums;
using BleCommands.Core.Events;

namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a GATT characteristic on a Bluetooth LE device.
    /// </summary>
    /// <typeparam name="TCharacteristic">The platform-specific native characteristic type.</typeparam>
    public interface ICharacteristic<TCharacteristic> : IDisposable
    {
        /// <summary>
        /// Occurs when the characteristic value is received via notification.
        /// </summary>
        event EventHandler<ByteArrayEventArgs>? ValueReceived;

        /// <summary>
        /// Gets the unique identifier (UUID) of the characteristic.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the properties (Read, Write, Notify, etc.) supported by this characteristic.
        /// </summary>
        CharacteristicPropertyFlags Properties { get; }

        /// <summary>
        /// Gets the platform-specific native characteristic object.
        /// </summary>
        TCharacteristic NativeCharacteristic { get; }

        /// <summary>
        /// Gets a value indicating whether the characteristic supports the Read operation.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether the characteristic supports the Write operation.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Gets a value indicating whether the characteristic supports Notify or Indicate operations.
        /// </summary>
        bool CanUpdate { get; }

        /// <summary>
        /// Gets the attached token aggregator, or <c>null</c> if none is attached.
        /// </summary>
        TokenAggregator? TokenAggregator { get; }

        /// <summary>
        /// Reads the characteristic value as a UTF-8 string.
        /// </summary>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <returns>The characteristic value as a UTF-8 string.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CanRead"/> is <c>false</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task<string> ReadAsync(CancellationToken token = default);

        /// <summary>
        /// Writes a UTF-8 string to the characteristic.
        /// </summary>
        /// <param name="data">The string value to write.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CanWrite"/> is <c>false</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task WriteAsync(string data, CancellationToken token = default);

        /// <summary>
        /// Attaches a token aggregator to collect notification/indication values.
        /// </summary>
        /// <param name="tokenAggregator">The token aggregator to attach.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CanUpdate"/> is <c>false</c> or an aggregator is already attached.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tokenAggregator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        void AttachTokenAggregator(TokenAggregator tokenAggregator);

        /// <summary>
        /// Detaches the currently attached token aggregator.
        /// </summary>
        void DetachTokenAggregator();

        /// <summary>
        /// Starts receiving notifications from the characteristic.
        /// </summary>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CanUpdate"/> is <c>false</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task StartReceivingAsync(CancellationToken token = default);
    }
}
