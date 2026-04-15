namespace Core.Exceptions
{
    public class CommandStreamException : Exception
    {
        public CommandStreamException() : base()
        {
        }

        public CommandStreamException(string message) : base(message)
        {
        }

        public CommandStreamException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
