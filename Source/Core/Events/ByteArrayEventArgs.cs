namespace BleCommands.Core.Events
{
    public class ByteArrayEventArgs : EventArgs
    {
        public ByteArrayEventArgs(byte[] bytes)
        {
            Value = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public byte[] Value { get; }
    }
}
