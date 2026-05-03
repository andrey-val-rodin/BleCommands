using MauiSample.PageModels;

namespace MauiSample.Pages
{
    public partial class CharacteristicsPage : ContentPage
    {
        public CharacteristicsPage(CharacteristicsPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (BindingContext is CharacteristicsPageModel model)
                    await model.GetCharacteristicsCommand.ExecuteAsync(null);
            });
        }
    }
}
