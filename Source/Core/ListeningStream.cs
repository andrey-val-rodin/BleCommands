using Core.Contracts;

namespace Core
{
    public class ListeningStream : IDisposable
    {
        public event EventHandler<DeviceInputEventArgs>? TokenUpdated;

        private readonly MemoryStream _internalStream = new();
        private readonly byte[] _buffer = new byte[1024];
        private bool _disposedValue;

        public long Length => _internalStream.Length;

        public void Append(string bytes, EventHandler<DeviceInputEventArgs> eventHandler)
        {
            try
            {
                TokenUpdated += eventHandler;
                Append(bytes);
            }
            finally
            {
                TokenUpdated -= eventHandler;
            }
        }

        public void Append(string bytes)
        {
            lock (_internalStream)
            {
                var stringBytes = System.Text.Encoding.UTF8.GetBytes(bytes);
                _internalStream.Write(stringBytes, 0, stringBytes.Length);
                Parse();
            }
        }

        private void Parse()
        {
            _internalStream.Position = 0;
            int current = 0;
            int bytesToRemove = 0;
            while (true)
            {
                var b = _internalStream.ReadByte();
                if (b < 0)
                {
                    // End of stream
                    if (bytesToRemove > 0)
                    {
                        byte[] buf = _internalStream.GetBuffer();
                        Buffer.BlockCopy(buf, bytesToRemove, buf, 0, (int)_internalStream.Length - bytesToRemove);
                        _internalStream.SetLength(_internalStream.Length - bytesToRemove);
                    }
                    return;
                }

                if (b == Constants.Terminator)
                {
                    // Convert token to string and invoke handler
                    var text = AsciiBytesToString(_buffer, current);
                    System.Diagnostics.Debug.WriteLine("Token: " + text);
                    TokenUpdated?.Invoke(this, new DeviceInputEventArgs(text));
                    bytesToRemove += current + 1;
                    current = 0;
                    continue;
                }

                _buffer[current] = (byte)b;
                current++;
            }
        }

        private string AsciiBytesToString(byte[] buffer, int length)
        {
            return System.Text.Encoding.ASCII.GetString(buffer, 0, length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _internalStream.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
