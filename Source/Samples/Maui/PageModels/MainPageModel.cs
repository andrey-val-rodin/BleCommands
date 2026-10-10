using BleCommands.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;

namespace MauiSample.PageModels
{
    public partial class MainPageModel(DeviceHolder deviceHolder) : ObservableObject
    {
        private DeviceHolder DeviceHolder { get; set; } = deviceHolder;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isPermissionsGranted;

        [ObservableProperty]
        private string _deviceName = string.Empty;

        [ObservableProperty]
        private string _error = string.Empty;

        partial void OnDeviceNameChanged(string value)
        {
            Error = string.Empty;
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            Error = string.Empty;
            IsBusy = true;
            try
            {
                var scanner = new BleScanner();
                var device = await scanner.FindDeviceAsync(DeviceName);
                if (device == null)
                {
                    Error = $"Failed to connect to '{DeviceName}'";
                    return;
                }

                await device.ConnectAsync();
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