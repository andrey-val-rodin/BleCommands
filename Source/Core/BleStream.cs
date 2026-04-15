using BleCommands.Core.Events;
using BleCommands.Core.Exceptions;
using System.Text;

namespace BleCommands.Core
{
    public class BleStream
    {
        private const int Capacity = 1024;
        private const char Terminator = Constants.Terminator;

        private readonly StringBuilder _buffer = new();

        public event EventHandler<TextEventArgs>? TokenReceived;

        public void Append(string text)
        {
            lock (_buffer)
            {
                if (_buffer.Length + text.Length > Capacity)
                    throw new StreamException("Maximum buffer capacity exceeded.");

                _buffer.Append(text);
                Parse();
            }
        }

        private void Parse()
        {
            StringBuilder currentToken = new();
            for (int i = 0; i < _buffer.Length; i++)
            {
                char c = _buffer[i];
                if (c == Terminator)
                {
                    var token = currentToken.ToString();
                    TokenReceived?.Invoke(this, new TextEventArgs(token));
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }

            // Keep incomplete token in the buffer
            _buffer.Clear();
            _buffer.Append(currentToken.ToString());
        }
    }
}