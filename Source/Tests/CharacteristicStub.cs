using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Tests
{
    public sealed class CharacteristicStub(CharacteristicPropertyFlags properties)
        : ICharacteristic<GattCharacteristic>
    {
        public event EventHandler<ByteArrayEventArgs>? ValueReceived { add { } remove { } }

        public Guid Id { get; set; }

        public CharacteristicPropertyFlags Properties { get; } = properties;

        public GattCharacteristic NativeCharacteristic => throw new NotImplementedException();

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

        public Task StartUpdatesAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task StopUpdatesAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task WriteAsync(string data, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
