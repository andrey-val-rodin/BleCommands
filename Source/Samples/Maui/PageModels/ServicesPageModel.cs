using CommunityToolkit.Mvvm.ComponentModel;
using MauiSample.Models;

namespace MauiSample.PageModels
{
    public partial class ServicesPageModel(DeviceHolder device) : ObservableObject
    {
        public DeviceHolder DeviceHolder { get; set; } = device;

        [ObservableProperty]
        bool _isBusy;
    }
}
