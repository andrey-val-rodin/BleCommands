namespace BleCommands.Core.Events
{
    /// <summary>
    /// Provides data for events that transmit text-based information.
    /// </summary>
    public class TextEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextEventArgs"/> class.
        /// </summary>
        /// <param name="text">The text value to be transmitted with the event.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> is <c>null</c>.
        /// </exception>
        public TextEventArgs(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Gets the text value associated with the event.
        /// </summary>
        public string Text { get; }
    }
}
