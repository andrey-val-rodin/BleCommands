using Plugin.BLE;

namespace BleCommands.Maui
{
    /// <summary>
    /// Provides helper methods for checking Bluetooth state.
    /// </summary>
    public static class BluetoothHelper
    {
        /// <summary>
        /// Returns a value indicating whether Bluetooth hardware is available on the device.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Bluetooth hardware is available; <c>false</c> otherwise.
        /// </returns>
        public static bool IsBluetoothAvailable() => CrossBluetoothLE.Current.IsAvailable;

        /// <summary>
        /// Returns a value indicating whether Bluetooth is currently powered on.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Bluetooth is turned on; <c>false</c> otherwise.
        /// </returns>
        public static bool IsBluetoothOn() => CrossBluetoothLE.Current.IsOn;
    }
}
