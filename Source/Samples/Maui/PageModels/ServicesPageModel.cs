using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;
using System.Collections.ObjectModel;

namespace MauiSample.PageModels
{
    public partial class ServicesPageModel(DeviceHolder deviceHolder) : ObservableObject
    {
        DeviceHolder DeviceHolder { get; set; } = deviceHolder;

        [ObservableProperty]
        bool _isBusy;

        public ObservableCollection<MyService> Services { get; private set; } = [];

        [ObservableProperty]
        string _error = string.Empty;

        [ObservableProperty]
        MyService? _item;

        [RelayCommand]
        async Task GetServicesAsync()
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
        async Task ExploreServiceAsync(MyService? service)
        {
            if (service == null)
                return;

            Item = service;
            DeviceHolder.SelectedService = service;
            await Shell.Current.GoToAsync("characteristics", true);
        }
    }
}
