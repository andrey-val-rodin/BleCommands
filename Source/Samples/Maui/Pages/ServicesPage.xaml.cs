using MauiSample.PageModels;

namespace MauiSample.Pages
{
    public partial class ServicesPage : ContentPage
    {
        public ServicesPage(ServicesPageModel model)
        {
            BindingContext = model;
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (BindingContext is ServicesPageModel model)
                    await model.GetServicesCommand.ExecuteAsync(null);
            });
        }
    }
}
