using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace BleCommands.Windows
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
        public static async Task<bool> IsBluetoothAvailableAsync()
        {
            try
            {
                var adapter = await BluetoothAdapter.GetDefaultAsync()
                    .AsTask()
                    .ConfigureAwait(false);

                return adapter != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth availability check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns a value indicating whether Bluetooth is currently powered on.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Bluetooth is turned on; <c>false</c> otherwise.
        /// </returns>
        public static async Task<bool> IsBluetoothOnAsync()
        {
            try
            {
                var adapter = await BluetoothAdapter.GetDefaultAsync()
                    .AsTask()
                    .ConfigureAwait(false);

                if (adapter == null)
                    return false;

                var radio = await adapter.GetRadioAsync()
                    .AsTask()
                    .ConfigureAwait(false);

                return radio != null && radio.State == RadioState.On;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth state check failed: {ex.Message}");
                return false;
            }
        }
    }
}
