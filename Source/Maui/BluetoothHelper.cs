using Plugin.BLE;

namespace BleCommands.Maui
{
    public class BluetoothHelper
    {
        /// <summary>
        /// Returns true if Bluetooth is available on this computer
        /// </summary>
        public static Task<bool> IsBluetoothAvailableAsync()
        {
            return Task.FromResult(CrossBluetoothLE.Current.IsAvailable);
        }

        /// <summary>
        /// Returns true if Bluetooth is on
        /// </summary>
        public static Task<bool> IsBluetoothOnAsync()
        {
            return Task.FromResult(CrossBluetoothLE.Current.IsOn);
        }
    }
}
