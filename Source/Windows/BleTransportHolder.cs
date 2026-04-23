namespace BleCommands.Windows
{
    /// <summary>
    /// Holds references to Bluetooth Low Energy device, service, and transport objects,
    /// and ensures proper resource disposal.
    /// </summary>
    public class BleTransportHolder : IDisposable
    {
        private bool _disposed;

        public BleTransportHolder(Device device, Service service, BleTransport transport)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(transport);

            Device = device;
            Service = service;
            Transport = transport;
        }

        public Device Device { get; }

        public Service Service { get; }

        public BleTransport Transport { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Transport?.Dispose();
                    Service?.Dispose();
                    Device?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
