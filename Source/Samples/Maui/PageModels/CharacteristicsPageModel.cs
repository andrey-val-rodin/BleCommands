using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;
using System.Collections.ObjectModel;

namespace MauiSample.PageModels
{
    public partial class CharacteristicsPageModel(DeviceHolder deviceHolder) : ObservableObject
    {
        DeviceHolder DeviceHolder { get; set; } = deviceHolder;

        [ObservableProperty]
        bool _isBusy;

        public ObservableCollection<MyCharacteristic> Characteristics { get; private set; } = [];

        [ObservableProperty]
        string _error = string.Empty;

        [ObservableProperty]
        MyService? _item;

        [RelayCommand]
        async Task GetCharacteristicsAsync()
        {
            IsBusy = true;
            try
            {
                var service = DeviceHolder.SelectedService;
                if (service == null)
                    return;

                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    Characteristics.Add(new MyCharacteristic(characteristic.NativeCharacteristic));
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
        void ExploreService()
        {
            if (Item == null)
                return;
        }
    }
}
