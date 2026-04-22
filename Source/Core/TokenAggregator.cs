using BleCommands.Core.Events;
using System.Text;

namespace BleCommands.Core
{
    /// <summary>
    /// Aggregates text fragments into complete tokens delimited by the specified character.
    /// </summary>
    /// <remarks>
    /// Thread-safe. Call <see cref="Append"/> with incoming data. When delimiter is encountered,
    /// the accumulated token is raised via <see cref="TokenReceived"/> event.
    /// </remarks>
    public class TokenAggregator
    {
        public const char DefaultTokenDelimiter = '\n';

        private readonly char _delimiter;
        private readonly StringBuilder _buffer = new();

        public TokenAggregator(char delimiter = DefaultTokenDelimiter)
        {
            _delimiter = delimiter;
        }

        public char TokenDelimiter => _delimiter;

        public event EventHandler<TextEventArgs>? TokenReceived;

        public void Append(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            List<string> tokensToRaise = new();

            lock (_buffer)
            {
                Parse(text, tokensToRaise);
            }

            foreach (var token in tokensToRaise)
            {
                System.Diagnostics.Debug.WriteLine("Token: " + token);
                TokenReceived?.Invoke(this, new TextEventArgs(token));
            }
        }

        protected void Parse(string text, List<string> tokensToRaise)
        {
            foreach (char c in text)
            {
                if (c == TokenDelimiter)
                {
                    tokensToRaise.Add(_buffer.ToString());
                    _buffer.Clear();
                }
                else
                {
                    _buffer.Append(c);
                }
            }
        }
    }
}