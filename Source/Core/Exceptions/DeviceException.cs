namespace BleCommands.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur during device operations.
    /// </summary>
    public class DeviceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceException"/> class
        /// with the specified error message and a reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception,
        /// or <c>null</c> if no inner exception is specified.
        /// </param>
        public DeviceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
