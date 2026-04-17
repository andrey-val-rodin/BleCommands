using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Windows.Extensions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace BleCommands.Windows
{
    public class Characteristic : ICharacteristic<GattCharacteristic>
    {
        private TokenAggregator? _tokenAggregator;
        private bool _disposed;

        public Characteristic(GattCharacteristic characteristic)
        {
            NativeCharacteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
        }

        public GattCharacteristic NativeCharacteristic { get; }

        public Guid Id => NativeCharacteristic.Uuid;

        public CharacteristicPropertyFlags Properties =>
            (CharacteristicPropertyFlags)NativeCharacteristic.CharacteristicProperties;

        /// <summary>
        /// Indicates whether the characteristic can be read or not.
        /// </summary>
        public bool CanRead => Properties.HasFlag(CharacteristicPropertyFlags.Read);

        /// <summary>
        /// Indicates whether the characteristic supports notify or not.
        /// </summary>
        public bool CanUpdate => Properties.HasFlag(CharacteristicPropertyFlags.Notify) ||
                                 Properties.HasFlag(CharacteristicPropertyFlags.Indicate);

        /// <summary>
        /// Indicates whether the characteristic can be written or not.
        /// </summary>
        public bool CanWrite => Properties.HasFlag(CharacteristicPropertyFlags.Write) ||
                                Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse);

        public TokenAggregator? TokenAggregator => _tokenAggregator;

        public async Task<string> ReadAsync(CancellationToken token = default)
        {
            var result = await NativeCharacteristic
                .ReadValueAsync()
                .AsTask(token)
                .ConfigureAwait(false);
            var bytes = result.GetValueOrThrowIfError();
            return ConvertToString(bytes);
        }

        public async Task StartUpdatesAsync(CancellationToken token = default)
        {
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

        public async Task StopUpdatesAsync(CancellationToken token = default)
        {
            var result = await NativeCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorWithResultAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None)
                .AsTask(token)
                .ConfigureAwait(false);
            result.ThrowIfError();

            NativeCharacteristic.ValueChanged -= NativeCharacteristic_ValueChanged;
        }

        public async Task WriteAsync(string text, CancellationToken token = default)
        {
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

        protected void NativeCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var bytes = args.CharacteristicValue.ToArray();
            var text = ConvertToString(bytes);

            var tokenAggegater = Interlocked.CompareExchange(ref _tokenAggregator, null, null);
            tokenAggegater?.Append(text);
        }

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

        public void AttachTokenAggregator(TokenAggregator tokenAggegater)
        {
            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Update nor Indicate.");

            ArgumentNullException.ThrowIfNull(tokenAggegater);

            var original = Interlocked.CompareExchange(ref _tokenAggregator, tokenAggegater, null);
            if (original != null)
                throw new InvalidOperationException("TokenAggegater is already attached. Call DetachTokenAggregator first.");
        }

        public void DetachTokenAggregator()
        {
            Interlocked.Exchange(ref _tokenAggregator, null);
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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}