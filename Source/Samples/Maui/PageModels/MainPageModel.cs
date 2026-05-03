using BleCommands.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;

namespace MauiSample.PageModels
{
    public partial class MainPageModel(DeviceHolder deviceHolder) : ObservableObject
    {
        DeviceHolder DeviceHolder { get; set; } = deviceHolder;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isPermissionsGranted;

        [ObservableProperty]
        //TODO: remove "Rotating Table"
        string _deviceName = "Rotating Table";//string.Empty;

        [ObservableProperty]
        string _error = string.Empty;

        partial void OnDeviceNameChanged(string value)
        {
            Error = string.Empty;
        }

        [RelayCommand]
        async Task ConnectAsync()
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