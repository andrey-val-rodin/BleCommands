using BleCommands.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;

namespace MauiSample.PageModels
{
    public partial class MainPageModel(DeviceHolder device) : ObservableObject
    {
        public DeviceHolder DeviceHolder { get; set; } = device;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isPermissionsGranted;

        [ObservableProperty]
        string _deviceName = string.Empty;

        [ObservableProperty]
        string _error = string.Empty;

        partial void OnDeviceNameChanged(string value)
        {
            Error = string.Empty;
        }

        [RelayCommand]
        async Task ConnectAsync()
        {
            IsBusy = true;
            try
            {
                var scanner = new BleScanner();
                using var device = await scanner.FindDeviceAsync(DeviceName, TimeSpan.FromSeconds(3));
                if (device == null)
                {
                    Error = $"Failed to connect to '{DeviceName}'";
                    return;
                }

                await device.ConnectAsync();
                var services = await device.GetServicesAsync();
                DeviceHolder.Device = device;

                await Shell.Current.GoToAsync("services", true);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}