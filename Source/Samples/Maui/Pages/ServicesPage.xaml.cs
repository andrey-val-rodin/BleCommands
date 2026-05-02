using MauiSample.PageModels;

namespace MauiSample.Pages
{
    public partial class ServicesPage : ContentPage
    {
        public ServicesPage(ServicesPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}