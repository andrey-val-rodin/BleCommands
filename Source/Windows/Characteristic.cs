using Core.Contracts;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Windows
{
    public class Characteristic : ICharacteristic
    {
        public event EventHandler<ValueUpdatedEventArgs>? ValueUpdated;

        public Characteristic(GattCharacteristic characteristic)
        {
            ExternalCharacteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
        }

        private GattCharacteristic ExternalCharacteristic { get; }

        public Guid Id => ExternalCharacteristic.Uuid;

        public CharacteristicPropertyFlags Properties => (CharacteristicPropertyFlags)ExternalCharacteristic.CharacteristicProperties;

        /// <summary>
        /// Indicates whether the characteristic can be read or not.
        /// </summary>
        public bool CanRead => Properties.HasFlag(CharacteristicPropertyFlags.Read);

        /// <summary>
        /// Indicates whether the characteristic supports notify or not.
        /// </summary>
        public bool CanUpdate => Properties.HasFlag(CharacteristicPropertyFlags.Notify) |
                                 Properties.HasFlag(CharacteristicPropertyFlags.Indicate);

        /// <summary>
        /// Indicates whether the characteristic can be written or not.
        /// </summary>
        public bool CanWrite => Properties.HasFlag(CharacteristicPropertyFlags.Write) |
                                Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse);

        public Task<byte[]> ReadAsync(CancellationToken _ = default)
        {
            //var result = await ExternalCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            //return result.GetValueOrThrowIfError();

            throw new NotImplementedException();
        }

        public Task StartUpdatesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task StopUpdatesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
