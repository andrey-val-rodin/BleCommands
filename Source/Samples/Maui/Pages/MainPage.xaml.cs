using BleCommands.Maui;
using MauiSample.PageModels;

//xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
namespace MauiSample.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await RequestBluetoothPermissionsAsync();
                if (BindingContext is MainPageModel model)
                    model.IsPermissionsGranted = true;
/*
                var scanner = new BleScanner();
                using var device = await scanner.FindDeviceAsync("Rotating Table", TimeSpan.FromSeconds(1));
                if (device == null)
                    return;

                await device.ConnectAsync();
                var ccc = device.IsConnected;
*/
            });
        }

        public static async Task<bool> RequestBluetoothPermissionsAsync()
        {
            // Check version of Android
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                var apiLevel = DeviceInfo.Current.Version.Major;

                if (apiLevel >= 12) // Android 12+
                {
                    var bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
                    return bluetoothStatus == PermissionStatus.Granted;
                }
                else if (apiLevel >= 10) // Android 10-11
                {
                    var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    return locationStatus == PermissionStatus.Granted;
                }
                else
                    return false;
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                var bleStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
                return bleStatus == PermissionStatus.Granted;
            }

            return true;
        }

        private void Entry_Completed(object sender, EventArgs e)
        {
            if (BindingContext is MainPageModel viewModel)
            {
                viewModel.ConnectCommand.Execute(null);
            }
        }
    }
}
