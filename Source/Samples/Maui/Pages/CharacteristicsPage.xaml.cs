using MauiSample.PageModels;

namespace MauiSample.Pages
{
    public partial class CharacteristicsPage : ContentPage
    {
        public CharacteristicsPage(CharacteristicsPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
            model.RequestUserInput += Model_RequestUserInputAsync;
        }

        private async Task<string> Model_RequestUserInputAsync(Models.MyCharacteristic arg)
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                return await DisplayPromptAsync("Write Value",
                    $"Enter value to write:",
                    "OK", "Cancel");
            });
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
