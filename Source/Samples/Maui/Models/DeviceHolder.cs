using BleCommands.Maui;
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
                Clean();
                _device = value;
            }
        }

        public MyService[] Services { get; private set; } = [];

        public MyService? SelectedService { get; set; }

        public void SetServices(IReadOnlyList<Service> services)
        {
            Services = new MyService[services.Count];
            for (int i = 0; i < services.Count; i++)
            {
                Services[i] = new MyService(services[i].NativeService);
            }
        }

        private void Clean()
        {
            // Will initiate a cascading disposing of all underlying objects
            _device?.Dispose();
        }
    }
}
