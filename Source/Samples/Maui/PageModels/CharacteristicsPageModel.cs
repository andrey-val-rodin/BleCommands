using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSample.Models;
using System.Collections.ObjectModel;

namespace MauiSample.PageModels
{
    public partial class CharacteristicsPageModel(DeviceHolder deviceHolder) : ObservableObject
    {
        public event Func<MyCharacteristic, Task<string>>? RequestUserInput;

        DeviceHolder DeviceHolder { get; set; } = deviceHolder;

        [ObservableProperty]
        bool _isBusy;

        public ObservableCollection<MyCharacteristic> Characteristics { get; private set; } = [];

        [ObservableProperty]
        string _error = string.Empty;

        [ObservableProperty]
        MyCharacteristic? _item;

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
                DeviceHolder.SetCharacteristics(characteristics);
                foreach (var characteristic in DeviceHolder.Characteristics)
                {
                    await characteristic.InitializeAsync();
                    Characteristics.Add(characteristic);
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
        async Task WriteAsync(MyCharacteristic characteristic)
        {
            if (characteristic == null)
                return;

            Item = characteristic;

            var handler = RequestUserInput;
            if (handler != null)
            {
                var input = await handler.Invoke(characteristic);
                if (input != null)
                {
                    try
                    {
                        await characteristic.WriteAsync(input + '\n');
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;
                    }
                }
            }
        }

        [RelayCommand]
        async Task ReadAsync(MyCharacteristic characteristic)
        {
            if (characteristic == null)
                return;

            Item = characteristic;

            try
            {
                characteristic.ReadValue = await characteristic.ReadAsync();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        [RelayCommand]
        void ToggleNotifying(MyCharacteristic characteristic)
        {
            if (characteristic == null)
                return;

            Item = characteristic;
            characteristic.IsNotifying = !characteristic.IsNotifying;
        }
    }
}
