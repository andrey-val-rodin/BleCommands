using BleCommands.Maui;

namespace MauiSample
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!await RequestBluetoothPermissionsAsync())
                    return;

                var scanner = new BleScanner();
                using var device = await scanner.FindDeviceAsync("Rotating Table", TimeSpan.FromSeconds(1));
                if (device == null)
                    return;

                await device.ConnectAsync();
                var ccc = device.IsConnected;
            });
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
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
    }
}
