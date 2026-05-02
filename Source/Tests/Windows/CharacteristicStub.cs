using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests.Windows
{
    internal class CharacteristicStub(CharacteristicPropertyFlags properties)
        : ICharacteristic<GattCharacteristic>
    {
        public event EventHandler<ByteArrayEventArgs>? ValueReceived { add { } remove { } }

        public bool Disposed { get; private set; }

        public Guid Id { get; set; }

        public CharacteristicPropertyFlags Properties { get; } = properties;

        public GattCharacteristic NativeCharacteristic => null!;

        public bool CanRead => throw new NotImplementedException();

        public bool CanWrite => throw new NotImplementedException();

        public bool CanUpdate => throw new NotImplementedException();

        public TokenAggregator? TokenAggregator { get; private set; }

        public void EmulateReceiving(string text)
        {
            TokenAggregator?.Append(text);
        }

        public void AttachTokenAggregator(TokenAggregator tokenAggregator)
        {
            TokenAggregator = tokenAggregator;
        }

        public void DetachTokenAggregator()
        {
            TokenAggregator = null;
        }

        public Task<string> ReadAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string data, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task StartReceivingAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
