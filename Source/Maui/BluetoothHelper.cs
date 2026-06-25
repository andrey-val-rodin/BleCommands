using Plugin.BLE;

namespace BleCommands.Maui
{
    /// <summary>
    /// Provides helper methods for checking Bluetooth state.
    /// </summary>
    public class BluetoothHelper
    {
        /// <summary>
        /// Returns a value indicating whether Bluetooth hardware is available on the device.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Bluetooth hardware is available; <c>false</c> otherwise.
        /// </returns>
        public static Task<bool> IsBluetoothAvailableAsync()
        {
            return Task.FromResult(CrossBluetoothLE.Current.IsAvailable);
        }

        /// <summary>
        /// Returns a value indicating whether Bluetooth is currently powered on.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Bluetooth is turned on; <c>false</c> otherwise.
        /// </returns>
        public static Task<bool> IsBluetoothOnAsync()
        {
            return Task.FromResult(CrossBluetoothLE.Current.IsOn);
        }
    }
}
