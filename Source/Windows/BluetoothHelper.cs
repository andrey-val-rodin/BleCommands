using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace BleCommands.Windows
{
    public class BluetoothHelper
    {
        /// <summary>
        /// Returns true if Bluetooth is available
        /// </summary>
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
        /// Returns true if Bluetooth is on
        /// </summary>
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
