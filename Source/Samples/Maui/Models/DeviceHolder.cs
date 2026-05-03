using BleCommands.Maui;
using Device = BleCommands.Maui.Device;

namespace MauiSample.Models
{
    public class DeviceHolder
    {
        private Device? _device;
        private readonly List<MyService> _services = [];

        public Device? Device
        {
            get => _device;
            set
            {
                Clean();
                _device = value;
            }
        }

        public IReadOnlyList<MyService> Services => _services;

        public void AddServices(IReadOnlyList<Service> services)
        {
            foreach (var service in services)
            {
                var myService = new MyService(service.NativeService);
                _services.Add(myService);
            }
        }

        private void Clean()
        {
            // Will initiate a cascading disposing of all underlying objects
            _device?.Dispose();
            _services.Clear();
        }
    }
}
