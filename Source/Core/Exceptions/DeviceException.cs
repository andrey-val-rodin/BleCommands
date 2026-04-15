namespace BleCommands.Core.Exceptions
{
    public class DeviceException : Exception
    {
        public DeviceException() : base()
        {
        }

        public DeviceException(string message) : base(message)
        {
        }

        public DeviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
