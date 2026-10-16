using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;
using System.Collections.ObjectModel;

namespace MauiSample.PageModels
{
    public partial class ServicesPageModel(DeviceHolder deviceHolder) : ObservableObject
    {
        public DeviceHolder DeviceHolder { get; set; } = deviceHolder;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        public ObservableCollection<MyService> Services { get; private set; } = [];

        [ObservableProperty]
        public partial string Error { get; set; } = string.Empty;

        [ObservableProperty]
        public partial MyService? Item { get; set; }

        [RelayCommand]
        private async Task GetServicesAsync()
        {
            IsBusy = true;
            try
            {
                var device = DeviceHolder.Device;
                if (device == null)
                    return;

                var services = await device.GetServicesAsync();
                DeviceHolder.SetServices(services);
                Services.Clear();
                foreach (var service in DeviceHolder.Services)
                {
                    Services.Add(service);
                }
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

        [RelayCommand]
        private async Task ExploreServiceAsync(MyService? service)
        {
            if (service == null)
                return;

            Item = service;
            DeviceHolder.SelectedService = service;
            await Shell.Current.GoToAsync("characteristics", true);
        }
    }
}
