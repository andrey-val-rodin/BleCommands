using BleCommands.Core.Contracts;

namespace BleCommands.Core.Events
{
    /// <summary>
    /// Provides data for the <see cref="ICharacteristic.ValueReceived"/> event.
    /// </summary>
    public class ByteArrayEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayEventArgs"/> class.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bytes"/> is <c>null</c>.
        /// </exception>
        public ByteArrayEventArgs(byte[] bytes)
        {
            Value = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        /// <summary>
        /// Gets the array of bytes associated with the event.
        /// </summary>
        public byte[] Value { get; }
    }
}
