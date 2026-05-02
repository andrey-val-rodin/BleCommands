using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.BLE.Abstractions;

namespace MauiSample.PageModels
{
    public partial class ServicesPageModel : ObservableObject, IQueryAttributable
    {
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
        }
    }
}
