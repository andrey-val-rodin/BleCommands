using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using Plugin.BLE.Abstractions.EventArgs;
using System.Text;
using NativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;

namespace BleCommands.Maui
{
    /// <summary>
    /// MAUI implementation of <see cref="ICharacteristic{TNativeCharacteristic}"/>
    /// using the Plugin.BLE abstraction layer.
    /// </summary>
    public class Characteristic : ICharacteristic<NativeCharacteristic>
    {
        private TokenAggregator? _tokenAggregator;
        private readonly object _lock = new();
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Characteristic"/> class
        /// using a native <see cref="Plugin.BLE.Abstractions.Contracts.ICharacteristic"/>.
        /// </summary>
        /// <param name="characteristic">The <see cref="Plugin.BLE.Abstractions.Contracts.ICharacteristic"/> to wrap.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="characteristic"/> is <c>null</c>.
        /// </exception>
        public Characteristic(NativeCharacteristic characteristic)
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
        public NativeCharacteristic NativeCharacteristic { get; }

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
        public bool IsReceiving { get; private set; }

        /// <inheritdoc/>
        public TokenAggregator? TokenAggregator => _tokenAggregator;

        /// <inheritdoc/>
        /// <exception cref="Exception">
        /// Thrown if the read operation fails at the Bluetooth level.
        /// </exception>
        public async Task<string> ReadAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (!CanRead)
                throw new InvalidOperationException("The characteristic is not Read.");

            await NativeCharacteristic.ReadAsync(token).ConfigureAwait(false);
            return ConvertToString(NativeCharacteristic.Value);
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">
        /// Thrown if the write operation fails at the Bluetooth level.
        /// </exception>
        public async Task WriteAsync(string text, CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (!CanWrite)
                throw new InvalidOperationException(
                    "The characteristic is neither Write nor WriteWithoutResponse.");

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var bytes = Encoding.UTF8.GetBytes(text);
            await NativeCharacteristic.WriteAsync(bytes, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a byte array to a UTF-8 string.
        /// </summary>
        /// <param name="value">The byte array to convert.</param>
        /// <returns>
        /// The converted string, or an empty string if the conversion fails.
        /// </returns>
        public static string ConvertToString(byte[] value)
        {
            try
            {
                return Encoding.UTF8.GetString(value);
            }
            catch (Exception ex) when (ex is DecoderFallbackException or
                                             ArgumentException or
                                             ArgumentNullException)
            {
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public void AttachTokenAggregator(TokenAggregator tokenAggregator)
        {
            ThrowIfDisposed();

            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Notify nor Indicate.");

            if (tokenAggregator == null)
                throw new ArgumentNullException(nameof(tokenAggregator));

            lock (_lock)
            {
                if (_tokenAggregator != null)
                    throw new InvalidOperationException(
                        "TokenAggregator is already attached. Call DetachTokenAggregator first.");

                _tokenAggregator = tokenAggregator;
            }
        }

        /// <inheritdoc/>
        public void DetachTokenAggregator()
        {
            lock (_lock)
            {
                _tokenAggregator = null;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">
        /// Thrown if the operation fails at the Bluetooth level.
        /// </exception>
        public async Task StartReceivingAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (IsReceiving)
                throw new InvalidOperationException("Receiving is in progress already.");

            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Notify nor Indicate.");

            await NativeCharacteristic.StartUpdatesAsync(token).ConfigureAwait(false);
            NativeCharacteristic.ValueUpdated += NativeCharacteristic_ValueUpdated;
            IsReceiving = true;
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">
        /// Thrown if the operation fails at the Bluetooth level.
        /// </exception>
        public async Task StopReceivingAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!IsReceiving)
                throw new InvalidOperationException("Receiving is not in progress.");

            await NativeCharacteristic.StopUpdatesAsync(token).ConfigureAwait(false);
            NativeCharacteristic.ValueUpdated -= NativeCharacteristic_ValueUpdated;
            IsReceiving = false;
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

            TokenAggregator? tokenAggregator;
            lock (_lock)
            {
                tokenAggregator = _tokenAggregator;
            }

            tokenAggregator?.Append(e.Characteristic.StringValue);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(typeof(Characteristic).FullName);
        }

        /// <summary>
        /// Releases managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
