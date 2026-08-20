namespace BleCommands.Windows.Events
{
    /// <summary>
    /// Provides data for DeviceDiscovered event.
    /// </summary>
    public class DeviceDiscoveredEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDiscoveredEventArgs"/> class.
        /// </summary>
        /// <param name="bluetoothAddress">the bluetooth address of the device.</param>
        public DeviceDiscoveredEventArgs(ulong bluetoothAddress)
        {
            BluetoothAddress = bluetoothAddress;
        }

        /// <summary>
        /// Gets the bluetooth address of the device associated with the event.
        /// </summary>
        public ulong BluetoothAddress { get; }
    }
}
