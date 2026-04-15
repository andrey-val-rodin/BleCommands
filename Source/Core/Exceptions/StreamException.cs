namespace BleCommands.Core.Exceptions
{
    public class StreamException : Exception
    {
        public StreamException() : base()
        {
        }

        public StreamException(string message) : base(message)
        {
        }

        public StreamException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
