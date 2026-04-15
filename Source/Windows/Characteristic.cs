using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
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
        public event EventHandler<ByteArrayEventArgs>? ValueUpdated;

        private BleStream? _stream;

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

        public BleStream? Stream => _stream;

        public async Task<byte[]> ReadAsync(CancellationToken token = default)
        {
            var result = await NativeCharacteristic
                .ReadValueAsync()
                .AsTask(token);
            return result.GetValueOrThrowIfError();
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
                .AsTask(token);
            result.ThrowIfError();

            NativeCharacteristic.ValueChanged += NativeCharacteristic_ValueChanged;
        }

        public async Task StopUpdatesAsync(CancellationToken token = default)
        {
            var result = await NativeCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorWithResultAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None)
                .AsTask(token);
            result.ThrowIfError();

            NativeCharacteristic.ValueChanged -= NativeCharacteristic_ValueChanged;
        }

        public async Task WriteAsync(byte[] data, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (!CanWrite)
                throw new InvalidOperationException("The characteristic is neither Write nor Write without response.");

            IBuffer value = CryptographicBuffer.CreateFromByteArray(data);
            GattWriteOption option = Properties.HasFlag(CharacteristicPropertyFlags.Write)
                ? GattWriteOption.WriteWithResponse
                : GattWriteOption.WriteWithoutResponse;
            var result = await NativeCharacteristic
                .WriteValueWithResultAsync(value, option)
                .AsTask(token);
            result.ThrowIfError();
        }

        private void NativeCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var bytes = args.CharacteristicValue.ToArray();
            ValueUpdated?.Invoke(this, new ByteArrayEventArgs(bytes));

            var stream = Interlocked.CompareExchange(ref _stream, null, null);
            if (stream != null)
            {
                var text = ToString(bytes);
                stream.Append(text);
            }
        }

        private static string ToString(byte[] value)
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

        public void AttachCommandStream(BleStream stream)
        {
            if (!CanUpdate)
                throw new InvalidOperationException("The characteristic is neither Update nor Indicate.");

            ArgumentNullException.ThrowIfNull(stream);

            var original = Interlocked.CompareExchange(ref _stream, stream, null);
            if (original != null)
                throw new InvalidOperationException("CommandStream is already attached. Call DetachCommandStream first.");
        }

        public void DetachCommandStream()
        {
            Interlocked.Exchange(ref _stream, null);
        }
    }
}