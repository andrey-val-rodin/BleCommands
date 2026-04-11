using Core.Contracts;
using Maui.Adapters;
using ExternalAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;
using ExternalDeviceEventArgs = Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs;
using ExternalScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;

namespace Maui
{
    public class Scanner : IScanner
    {
        private readonly string _deviceName;
        private readonly Dictionary<EventHandler<DeviceEventArgs>, ProxyHandler> _proxies = new();
        
        public Scanner(string deviceName)
        {
            _deviceName = deviceName;
        }

        public TimeSpan SkanTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public ScanMode ScanMode { get; set; } = ScanMode.LowLatency;
        private ExternalAdapter Adapter => Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        public event EventHandler<DeviceEventArgs> DeviceDiscovered
        {
            add
            {
                void Handler(object sender, ExternalDeviceEventArgs e)
                {
                    var convertedArgs = new DeviceEventArgs(new DeviceAdapter(e.Device));
                    value(sender, convertedArgs);
                }

                var proxy = new ProxyHandler(Handler);
                _proxies.Add(value, proxy);
                Adapter.DeviceDiscovered += Handler;

            }
            remove
            {
                if (_proxies.TryGetValue(value, out var proxy))
                {
                    Adapter.DeviceDiscovered -= proxy.Handler;
                    _proxies.Remove(value);
                }
            }
        }

        private class ProxyHandler
        {
            public EventHandler<ExternalDeviceEventArgs> Handler { get; }

            public ProxyHandler(EventHandler<ExternalDeviceEventArgs> handler)
            {
                Handler = handler;
            }
        }

        public async Task<bool> StartScanningAsync(CancellationToken token = default)
        {
            try
            {
                Adapter.ScanTimeout = (int)SkanTimeout.TotalMilliseconds;
                Adapter.ScanMode = ToExternalScanMode(ScanMode);
                await Adapter.StartScanningForDevicesAsync(deviceFilter: (d) => d.Name == _deviceName);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                // This code is actual for UWP
//TODO                await _interaction.AlertAsync($"{ex.Message}\nУбедитесь, что Bluetooth включён", "Ошибка сканирования");
                throw;
            }
        }

        private static ExternalScanMode ToExternalScanMode(ScanMode mode)
        {
            return mode switch
            {
                ScanMode.Passive => ExternalScanMode.Passive,
                ScanMode.LowPower => ExternalScanMode.LowPower,
                ScanMode.Balanced => ExternalScanMode.Balanced,
                _ => ExternalScanMode.LowLatency,
            };
        }

        public async Task StopScanningAsync()
        {
            await Adapter.StopScanningForDevicesAsync();
        }
    }
}
