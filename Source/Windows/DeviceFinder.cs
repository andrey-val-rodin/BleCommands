using Core.Contracts;
using Windows.Devices.Enumeration;

namespace Windows
{
    public class DeviceFinder : IDeviceFinder, IDisposable
    {
        private const int _millisecondsTimeout = 5000;

        private DeviceWatcher? _deviceWatcher;
        private bool _disposed;
        private readonly AutoResetEvent _event = new(false);
        private DeviceInformation? _foundTable;

        public async Task<IDevice?> FindDeviceAsync(
            string deviceName, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            _foundTable = null;
            StartBleDeviceWatcher();
            await Task.Run(() => _event.WaitOne(_millisecondsTimeout));
            return _foundTable == null ? null : new Device(_foundTable.Id);
        }

        private void StartBleDeviceWatcher()
        {
            string aqsFilter = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\" AND System.ItemNameDisplay:~<\"Rotating\"";

            _deviceWatcher = DeviceInformation.CreateWatcher(
                aqsFilter,
                null,
                DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher. We must subscribe to all events.
            _deviceWatcher.Added += DeviceWatcher_Added;
            _deviceWatcher.Updated += DeviceWatcher_Updated;
            _deviceWatcher.Removed += DeviceWatcher_Removed;

            // Start the watcher. Active enumeration is limited to approximately 30 seconds.
            // This limits power usage and reduces interference with other Bluetooth activities.
            // To monitor for the presence of Bluetooth LE devices for an extended period,
            // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
            // sample for an example.
            _deviceWatcher.Start();
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            if (deviceInfo?.Name != "Rotating Table")
                return;

            _event.Set();
            StopBleDeviceWatcher();
            _foundTable = deviceInfo;
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }

        private void StopBleDeviceWatcher()
        {
            if (_deviceWatcher != null)
            {
                // Unregister the event handlers.
                _deviceWatcher.Added -= DeviceWatcher_Added;
                _deviceWatcher.Updated -= DeviceWatcher_Updated;
                _deviceWatcher.Removed -= DeviceWatcher_Removed;

                // Stop the watcher.
                _deviceWatcher.Stop();
                _deviceWatcher = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _event.Dispose();
                    StopBleDeviceWatcher();
                }

                _deviceWatcher = null;
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
