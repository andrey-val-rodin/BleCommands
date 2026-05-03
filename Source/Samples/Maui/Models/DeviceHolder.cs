using Device = BleCommands.Maui.Device;

namespace MauiSample.Models
{
    public class DeviceHolder
    {
        private Device? _device;

        public Device? Device
        {
            get => _device;
            set
            {
                // Will initiate a cascading disposing of all underlying objects
                _device?.Dispose();

                _device = value;
            }
        }
    }
}
