namespace Core.Contracts
{
    public class ByteArrayEventArgs
    {
        public ByteArrayEventArgs(byte[] value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public byte[] Value { get; }
    }
}
