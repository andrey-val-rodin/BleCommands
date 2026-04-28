using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BleCommands.Tests.Maui
{
#nullable disable
    public class AdapterStub : IAdapter
    {
        public bool IsScanning => throw new NotImplementedException();

        public int ScanTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ScanMode ScanMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ScanMatchMode ScanMatchMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IReadOnlyList<IDevice> DiscoveredDevices => throw new NotImplementedException();

        public IReadOnlyList<IDevice> ConnectedDevices => throw new NotImplementedException();

        public IReadOnlyList<IDevice> BondedDevices => throw new NotImplementedException();

        public event EventHandler<DeviceEventArgs> DeviceAdvertised { add { } remove { } }
        public event EventHandler<DeviceEventArgs> DeviceDiscovered { add { } remove { } }
        public event EventHandler<DeviceEventArgs> DeviceConnected { add { } remove { } }
        public event EventHandler<DeviceEventArgs> DeviceDisconnected { add { } remove { } }
        public event EventHandler<DeviceErrorEventArgs> DeviceConnectionLost { add { } remove { } }
        public event EventHandler<DeviceErrorEventArgs> DeviceConnectionError { add { } remove { } }
        public event EventHandler<DeviceBondStateChangedEventArgs> DeviceBondStateChanged { add { } remove { } }
        public event EventHandler ScanTimeoutElapsed { add { } remove { } }

        public Task BondAsync(IDevice device)
        {
            throw new NotImplementedException();
        }

        public void ClearDeviceRegistries()
        {
            throw new NotImplementedException();
        }

        public Task ConnectToDeviceAsync(IDevice device, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectDeviceAsync(IDevice device, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDevice> GetKnownDevicesByIds(Guid[] ids)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
        {
            throw new NotImplementedException();
        }

        public Task StartScanningForDevicesAsync(ScanFilterOptions scanFilterOptions = null, Func<IDevice, bool> deviceFilter = null, bool allowDuplicatesKey = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task StartScanningForDevicesAsync(Guid[] serviceUuids, Func<IDevice, bool> deviceFilter = null, bool allowDuplicatesKey = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task StopScanningForDevicesAsync()
        {
            throw new NotImplementedException();
        }

        public bool SupportsCodedPHY()
        {
            throw new NotImplementedException();
        }

        public bool SupportsExtendedAdvertising()
        {
            throw new NotImplementedException();
        }
    }
#nullable restore
}
