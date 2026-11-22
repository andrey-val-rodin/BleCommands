using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BleCommands.Tests.Maui
{
    internal sealed class FakeNativeCharacteristic(
        CharacteristicPropertyType properties,
        CharacteristicWriteType writeType) : ICharacteristic
    {
        public event EventHandler<CharacteristicUpdatedEventArgs>? ValueUpdated
        {
            add { }
            remove { }
        }

        public Guid Id { get; } = Guid.NewGuid();

        public string Uuid => Id.ToString();

        public string Name => "Fake characteristic";

        public byte[] Value { get; private set; } = [];

        public string StringValue => System.Text.Encoding.UTF8.GetString(Value);

        public CharacteristicPropertyType Properties { get; } = properties;

        public CharacteristicWriteType WriteType { get; set; } = writeType;

        public bool CanRead => false;

        public bool CanWrite => true;

        public bool CanUpdate => false;

        public IService Service => null!;

        public CharacteristicWriteType? WriteTypeDuringWrite { get; private set; }

        public byte[] ValueWritten => Value;

        public Task<(byte[] data, int resultCode)> ReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult((Value, 0));
        }

        public Task<int> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteTypeDuringWrite = WriteType;
            Value = data;
            return Task.FromResult(data.Length);
        }

        public Task StartUpdatesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task StopUpdatesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<IDescriptor>> GetDescriptorsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<IDescriptor>>([]);
        }

        public Task<IDescriptor> GetDescriptorAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IDescriptor>(null!);
        }
    }
}
