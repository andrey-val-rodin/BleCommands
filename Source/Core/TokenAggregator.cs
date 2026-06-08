using BleCommands.Core.Events;
using System.Text;

namespace BleCommands.Core
{
    /// <summary>
    /// Combines fragments of text into complete tokens delimited by the specified character.
    /// </summary>
    /// <remarks>
    /// Thread-safe. Call <see cref="Append"/> with incoming data. When delimiter is encountered,
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

        public void Append(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            IEnumerable<string> tokensToRaise;
            lock (_buffer)
            {
                tokensToRaise = Parse(text);
            }

            foreach (var token in tokensToRaise)
            {
                System.Diagnostics.Debug.WriteLine("Token: " + token);
                TokenReceived?.Invoke(this, new TextEventArgs(token));
            }
        }

        protected IEnumerable<string> Parse(string text)
        {
            List<string> result = new();
            foreach (char c in text)
            {
                if (c == TokenDelimiter)
                {
                    result.Add(_buffer.ToString());
                    _buffer.Clear();
                }
                else
                {
                    _buffer.Append(c);
                }
            }

            return result;
        }
    }
}