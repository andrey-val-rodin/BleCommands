using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using Plugin.BLE.Abstractions.EventArgs;
using System.Text;
using INativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;

namespace BleCommands.Maui
{
    /// <summary>
    /// Represents a GATT characteristic.
    /// </summary>
    /// <remarks>
    /// This class wraps the Plugin.BLE.Abstractions.Contracts.ICharacteristic
    /// and provides a higher-level interface for BLE operations.
    /// </remarks>
    public class Characteristic : ICharacteristic<INativeCharacteristic>
    {
        private TokenAggregator? _tokenAggregator;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Characteristic"/> class
        /// using a native <see cref="INativeCharacteristic"/>.
        /// </summary>
        /// <param name="characteristic">The <see cref="INativeCharacteristic"/> to wrap.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="characteristic"/> is <c>null</c>.
        /// </exception>
        public Characteristic(INativeCharacteristic characteristic)
        {
            NativeCharacteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
            Id = NativeCharacteristic.Id;
            Properties = (CharacteristicPropertyFlags)NativeCharacteristic.Properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Characteristic"/> class for testing purposes.
        /// </summary>
        /// <param name="properties">The characteristic properties to simulate.</param>
        /// <remarks>
        /// This constructor is intended for unit testing only. It creates a characteristic
        /// without requiring an actual Bluetooth connection.
        /// </remarks>
        internal Characteristic(CharacteristicPropertyFlags properties)
        {
            NativeCharacteristic = null!;
            Properties = properties;
        }

        /// <inheritdoc/>
        public event EventHandler<ByteArrayEventArgs>? ValueReceived;

        /// <inheritdoc/>
        public INativeCharacteristic NativeCharacteristic { get; }

        /// <inheritdoc/>
        public Guid Id { get; private set; }

        /// <inheritdoc/>
        public CharacteristicPropertyFlags Properties { get; private set; }

        /// <inheritdoc/>
        public bool CanRead => Properties.HasFlag(CharacteristicPropertyFlags.Read);

        /// <inheritdoc/>
        public bool CanUpdate => Properties.HasFlag(CharacteristicPropertyFlags.Notify) ||
                                 Properties.HasFlag(CharacteristicPropertyFlags.Indicate);

        /// <inheritdoc/>
        public bool CanWrite => Properties.HasFlag(CharacteristicPropertyFlags.Write) ||
                                Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse);

        /// <inheritdoc/>
        public TokenAggregator? TokenAggregator => _tokenAggregator;

        /// <summary>
        /// Reads the characteristic value from the device.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the read operation.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation.
        /// The task result contains the characteristic value as a UTF-8 string.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the characteristic does not support the Read operation.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if the read operation fails at the Bluetooth level.
        /// </exception>
        public async Task<string> ReadAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (!CanRead)
                throw new InvalidOperationException("The characteristic is not Read.");

            await NativeCharacteristic.ReadAsync(token).ConfigureAwait(false);
            return NativeCharacteristic.StringValue;
        }

        /// <summary>
        /// Writes a string value to the characteristic.
        /// </summary>
        /// <param name="text">The string value to write. The string is encoded as UTF-8.</param>
        /// <param name="token">A cancellation token to cancel the write operation.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the characteristic does not support Write or WriteWithoutResponse operations.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="text"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if the write operation fails at the Bluetooth level.
        /// </exception>
        public async Task WriteAsync(string text, CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (!CanWrite)
                throw new InvalidOperationException("The characteristic is neither Write nor WriteWithoutResponse.");

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var bytes = Encoding.UTF8.GetBytes(text);
            await NativeCharacteristic.WriteAsync(bytes, token);
        }

        /// <summary>
        /// Attaches a token aggregator to collect incoming characteristic values.
        /// </summary>
        /// <param name="tokenAggregator">The token aggregator to attach.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the characteristic does not support Notify or Indicate operations,
        /// or if a token aggregator is already attached.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tokenAggregator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        public void AttachTokenAggregator(TokenAggregator tokenAggregator)
        {
            ThrowIfDisposed();

            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Notify nor Indicate.");

            if (tokenAggregator == null)
                throw new ArgumentNullException(nameof(tokenAggregator));

            var original = Interlocked.CompareExchange(ref _tokenAggregator, tokenAggregator, null);
            if (original != null)
                throw new InvalidOperationException("TokenAggregator is already attached. Call DetachTokenAggregator first.");
        }

        /// <inheritdoc/>
        public void DetachTokenAggregator()
        {
            Interlocked.Exchange(ref _tokenAggregator, null);
        }

        /// <summary>
        /// Starts receiving notifications or indications from the characteristic.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the characteristic does not support Notify or Indicate operations.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if the operation fails at the Bluetooth level.
        /// </exception>
        public async Task StartReceivingAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Notify nor Indicate.");

            await NativeCharacteristic.StartUpdatesAsync(token);
            NativeCharacteristic.ValueUpdated += NativeCharacteristic_ValueUpdated;
        }

        /// <summary>
        /// Handles the ValueUpdated event from the native characteristic.
        /// </summary>
        /// <remarks>
        /// This method converts the received bytes to a UTF-8 string, raises the
        /// <see cref="ValueReceived"/> event, and appends the string to the attached
        /// token aggregator if one exists.
        /// </remarks>
        private void NativeCharacteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            var bytes = e.Characteristic.Value;
            ValueReceived?.Invoke(this, new ByteArrayEventArgs(bytes));
            var text = e.Characteristic.StringValue;

            var tokenAggegater = Interlocked.CompareExchange(ref _tokenAggregator, null, null);
            tokenAggegater?.Append(text);
        }

        /// <summary>
        /// Stops receiving notifications or indications from the characteristic.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if the operation fails at the Bluetooth level.
        /// </exception>
        public async Task StopReceivingAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            await NativeCharacteristic.StopUpdatesAsync(token);
            NativeCharacteristic.ValueUpdated -= NativeCharacteristic_ValueUpdated;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("BleCommands.Maui.Characteristic");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (NativeCharacteristic != null)
                    {
                        NativeCharacteristic.ValueUpdated -= NativeCharacteristic_ValueUpdated;
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
