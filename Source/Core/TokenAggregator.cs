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
        private readonly List<string> _tokensToRaise = new();

        public TokenAggregator(char delimiter = DefaultTokenDelimiter)
        {
            TokenDelimiter = delimiter;
        }

        public event EventHandler<TextEventArgs>? TokenReceived;

        public char TokenDelimiter { get; }

        public void Append(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            lock (_buffer)
            {
                Parse(text);
            }

            foreach (var token in _tokensToRaise)
            {
                System.Diagnostics.Debug.WriteLine("Token: " + token);
                TokenReceived?.Invoke(this, new TextEventArgs(token));
            }
            _tokensToRaise.Clear();
        }

        protected void Parse(string text)
        {
            foreach (char c in text)
            {
                if (c == TokenDelimiter)
                {
                    _tokensToRaise.Add(_buffer.ToString());
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