using BleCommands.Core.Events;
using System.Text;

namespace BleCommands.Core
{
    /// <summary>
    /// Combines fragments of text into complete tokens delimited by the specified character.
    /// </summary>
    /// <remarks>
    /// Thread-safe. When delimiter is encountered,
    /// the accumulated token is raised via <see cref="TokenReceived"/> event.
    /// </remarks>
    public class TokenAggregator
    {
        public const char DefaultTokenDelimiter = '\n';
        private readonly StringBuilder _buffer = new();

        public TokenAggregator(char delimiter = DefaultTokenDelimiter)
        {
            TokenDelimiter = delimiter;
        }

        public char TokenDelimiter { get; }

        public event EventHandler<TextEventArgs>? TokenReceived;

        /// <summary>
        /// Appends a chunk of text to the internal buffer and raises <see cref="TokenReceived"/>
        /// events for each complete token found.
        /// </summary>
        /// <param name="text">Text fragment to append. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        /// <remarks>
        /// This method is thread-safe and guarantees that tokens from concurrent calls
        /// are raised in the order they are received.
        /// </remarks>
        public void Append(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            lock (_buffer)
            {
                foreach (char c in text)
                {
                    if (c == TokenDelimiter)
                    {
                        string token = _buffer.ToString();
                        _buffer.Clear();

                        System.Diagnostics.Debug.WriteLine($"Token: {token}");
                        TokenReceived?.Invoke(this, new TextEventArgs(token));
                    }
                    else
                    {
                        _buffer.Append(c);
                    }
                }
            }
        }
    }
}