using BleCommands.Maui;
using NativeService = Plugin.BLE.Abstractions.Contracts.IService;

namespace MauiSample.Models
{
    public partial class MyService(NativeService nativeService) : Service(nativeService)
    {
        public string Name
        {
            get
            {
                var name = NativeService.Name;
                if (name == "Unknown Service")
                    name = "Custom Service";
                return name;
            }
        }
    }
}
