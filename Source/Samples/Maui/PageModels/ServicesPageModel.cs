using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;
using System.Collections.ObjectModel;

namespace MauiSample.PageModels
{
    public partial class ServicesPageModel : ObservableObject
    {
        public ServicesPageModel(DeviceHolder device)
        {
            DeviceHolder = device;
            Services = new ObservableCollection<MyService>(DeviceHolder.Services);
        }

        DeviceHolder DeviceHolder { get; set; }

        [ObservableProperty]
        bool _isBusy;
        
        public ObservableCollection<MyService> Services { get; }

        [ObservableProperty]
        MyService _item;

        [ObservableProperty]
        int _index;

        [RelayCommand]
        async Task ExploreServiceAsync()
        {
            IsBusy = true;
            try
            {
                var characteristic = await Item.GetCharacteristicsAsync();
            }
            catch (Exception ex)
            {
                // TODO: go to main page and show error
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
