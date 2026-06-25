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
        /// <summary>
        /// The default token delimiter character (newline).
        /// </summary>
        public const char DefaultTokenDelimiter = '\n';

        private readonly StringBuilder _buffer = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenAggregator"/> class
        /// with the specified delimiter character.
        /// </summary>
        /// <param name="delimiter">
        /// The character that marks the end of a token.
        /// Defaults to <see cref="DefaultTokenDelimiter"/>.
        /// </param>
        public TokenAggregator(char delimiter = DefaultTokenDelimiter)
        {
            TokenDelimiter = delimiter;
        }

        /// <summary>
        /// Gets the character that marks the end of a token.
        /// </summary>
        public char TokenDelimiter { get; }

        /// <summary>
        /// Occurs when a complete token has been accumulated.
        /// </summary>
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