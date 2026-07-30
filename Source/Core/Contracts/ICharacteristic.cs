using BleCommands.Core.Enums;
using BleCommands.Core.Events;

namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Represents a GATT characteristic on a Bluetooth LE device.
    /// </summary>
    public interface ICharacteristic : IDisposable
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
        /// Gets a value indicating whether receiving of notifications or indications is in progress.
        /// </summary>
        bool IsReceiving { get; }

        /// <summary>
        /// Gets the attached token aggregator, or <c>null</c> if none is attached.
        /// </summary>
        TokenAggregator? TokenAggregator { get; }

        /// <summary>
        /// Reads the characteristic value.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the read operation.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation.
        /// The task result contains the characteristic value as a UTF-8 string.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CanRead"/> is <c>false</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task<string> ReadAsync(CancellationToken token = default);

        /// <summary>
        /// Writes a string value to the characteristic.
        /// </summary>
        /// <param name="text">The string value to write.</param>
        /// <param name="token">A cancellation token to cancel the write operation.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CanWrite"/> is <c>false</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="text"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task WriteAsync(string text, CancellationToken token = default);

        /// <summary>
        /// Attaches a token aggregator to collect notification/indication values.
        /// </summary>
        /// <param name="tokenAggregator">The token aggregator to attach.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="CanUpdate"/> is <c>false</c> or an aggregator is already attached.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tokenAggregator"/> is <c>null</c>.
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
        /// Starts receiving notifications or indications from the characteristic.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the characteristic does not support Notify or Indicate operations
        /// or <see cref="IsReceiving"/> is true.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task StartReceivingAsync(CancellationToken token = default);

        /// <summary>
        /// Stops receiving notifications or indications from the characteristic.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="IsReceiving"/> is false.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        Task StopReceivingAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Represents a generic GATT characteristic on a Bluetooth LE device.
    /// </summary>
    /// <typeparam name="TNativeCharacteristic">The platform-specific native characteristic type.</typeparam>
    public interface ICharacteristic<TNativeCharacteristic> : ICharacteristic
    {
        /// <summary>
        /// Gets the platform-specific native characteristic object.
        /// </summary>
        TNativeCharacteristic NativeCharacteristic { get; }
    }
}
