using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using BleCommands.Core.Exceptions;
using BleCommands.Windows.Extensions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace BleCommands.Windows
{
    /// <summary>
    /// Represents a GATT characteristic.
    /// </summary>
    /// <remarks>
    /// This class wraps the Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic
    /// and provides a higher-level interface for BLE operations.
    /// </remarks>
    public class Characteristic : ICharacteristic<GattCharacteristic>
    {
        private TokenAggregator? _tokenAggregator;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Characteristic"/> class
        /// using a native GATT characteristic.
        /// </summary>
        /// <param name="characteristic">The native GATT characteristic to wrap.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="characteristic"/> is <c>null</c>.
        /// </exception>
        public Characteristic(GattCharacteristic characteristic)
        {
            NativeCharacteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
            Id = NativeCharacteristic.Uuid;
            Properties = (CharacteristicPropertyFlags)NativeCharacteristic.CharacteristicProperties;
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

        /// <summary>
        /// Occurs when the characteristic value is received from the device.
        /// </summary>
        /// <remarks>
        /// This event is raised when the device sends a notification or indication
        /// with the characteristic's new value. To receive these events, you must first
        /// call <see cref="StartReceivingAsync"/>.
        /// </remarks>
        public event EventHandler<ByteArrayEventArgs>? ValueReceived;

        /// <summary>
        /// Gets the native GATT characteristic object.
        /// </summary>
        public GattCharacteristic NativeCharacteristic { get; }

        /// <summary>
        /// Gets the unique identifier (UUID) of the characteristic.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the properties of the characteristic.
        /// </summary>
        public CharacteristicPropertyFlags Properties { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the characteristic can be read.
        /// </summary>
        public bool CanRead => Properties.HasFlag(CharacteristicPropertyFlags.Read);

        /// <summary>
        /// Gets a value indicating whether the characteristic supports notifications or indications.
        /// </summary>
        public bool CanUpdate => Properties.HasFlag(CharacteristicPropertyFlags.Notify) ||
                                 Properties.HasFlag(CharacteristicPropertyFlags.Indicate);

        /// <summary>
        /// Gets a value indicating whether the characteristic can be written.
        /// </summary>
        public bool CanWrite => Properties.HasFlag(CharacteristicPropertyFlags.Write) ||
                                Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse);

        /// <summary>
        /// Gets the <see cref="TokenAggregator"/> attached to this characteristic.
        /// </summary>
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
        /// <exception cref="DeviceException">
        /// Thrown if the read operation fails at the Bluetooth level.
        /// </exception>
        public async Task<string> ReadAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!CanRead)
                throw new InvalidOperationException("The characteristic is not Read.");

            var result = await NativeCharacteristic
                .ReadValueAsync()
                .AsTask(token)
                .ConfigureAwait(false);
            var bytes = result.GetValueOrThrowIfError();
            return ConvertToString(bytes);
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
        /// <exception cref="DeviceException">
        /// Thrown if the write operation fails at the Bluetooth level.
        /// </exception>
        public async Task WriteAsync(string text, CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!CanWrite)
                throw new InvalidOperationException("The characteristic is neither Write nor Write without response.");

            ArgumentNullException.ThrowIfNull(text);

            var bytes = Encoding.UTF8.GetBytes(text);
            IBuffer value = CryptographicBuffer.CreateFromByteArray(bytes);
            GattWriteOption option = Properties.HasFlag(CharacteristicPropertyFlags.Write)
                ? GattWriteOption.WriteWithResponse
                : GattWriteOption.WriteWithoutResponse;
            var result = await NativeCharacteristic
                .WriteValueWithResultAsync(value, option)
                .AsTask(token)
                .ConfigureAwait(false);
            result.ThrowIfError();
        }

        /// <summary>
        /// Handles the ValueChanged event from the native GATT characteristic.
        /// </summary>
        /// <param name="sender">The native characteristic that raised the event.</param>
        /// <param name="args">The event arguments containing the characteristic value.</param>
        /// <remarks>
        /// This method converts the received bytes to a UTF-8 string, raises the
        /// <see cref="ValueReceived"/> event, and appends the string to the attached
        /// token aggregator if one exists.
        /// </remarks>
        protected void NativeCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var bytes = args.CharacteristicValue.ToArray();
            ValueReceived?.Invoke(this, new ByteArrayEventArgs(bytes));
            var text = ConvertToString(bytes);

            var tokenAggregator = Interlocked.CompareExchange(ref _tokenAggregator, null, null);
            tokenAggregator?.Append(text);
        }

        /// <summary>
        /// Converts a byte array to a UTF-8 string.
        /// </summary>
        /// <param name="value">The byte array to convert.</param>
        /// <returns>
        /// The converted string, or an empty string if the conversion fails.
        /// </returns>
        protected static string ConvertToString(byte[] value)
        {
            try
            {
                return Encoding.UTF8.GetString(value);
            }
            catch (Exception ex) when (ex is DecoderFallbackException or ArgumentException or ArgumentNullException)
            {
                return string.Empty;
            }
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
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Update nor Indicate.");

            ArgumentNullException.ThrowIfNull(tokenAggregator);

            var original = Interlocked.CompareExchange(ref _tokenAggregator, tokenAggregator, null);
            if (original != null)
                throw new InvalidOperationException("TokenAggregator is already attached. Call DetachTokenAggregator first.");
        }

        /// <summary>
        /// Detaches the currently attached token aggregator.
        /// </summary>
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
        /// <exception cref="DeviceException">
        /// Thrown if the operation fails at the Bluetooth level.
        /// </exception>
        public async Task StartReceivingAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // Logic as in Plugin.BLE.Windows.Characteristic:
            GattClientCharacteristicConfigurationDescriptorValue descriptor;
            if (Properties.HasFlag(CharacteristicPropertyFlags.Notify))
                descriptor = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            else if (Properties.HasFlag(CharacteristicPropertyFlags.Indicate))
                descriptor = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            else
                throw new InvalidOperationException("The characteristic is neither Update nor Indicate.");

            var result = await NativeCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorWithResultAsync(descriptor)
                .AsTask(token)
                .ConfigureAwait(false);
            result.ThrowIfError();

            NativeCharacteristic.ValueChanged += NativeCharacteristic_ValueChanged;
        }

        /// <summary>
        /// Stops receiving notifications or indications from the characteristic.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the characteristic has been disposed.
        /// </exception>
        /// <exception cref="DeviceException">
        /// Thrown if the operation fails at the Bluetooth level.
        /// </exception>
        public async Task StopReceivingAsync(CancellationToken token = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var result = await NativeCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorWithResultAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None)
                .AsTask(token)
                .ConfigureAwait(false);
            result.ThrowIfError();

            NativeCharacteristic.ValueChanged -= NativeCharacteristic_ValueChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (NativeCharacteristic != null)
                    {
                        NativeCharacteristic.ValueChanged -= NativeCharacteristic_ValueChanged;
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
