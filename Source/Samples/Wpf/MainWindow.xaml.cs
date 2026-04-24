using System.Windows;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DeviceName = "BLECommands Messaging Example";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            Loaded += MainWindow_Loaded;
            Closing += (s, e) =>
            {
                if (DataContext is IDisposable disposable)
                    disposable.Dispose();
            };
        }

#pragma warning disable IDE0079
#pragma warning disable VSTHRD102
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel model)
                return;

            App.JoinableTaskFactory.Run(async () =>
            {
                if (!await model.BeginAsync(DeviceName))
                {
                    model.Error = $"Failed to connect to device '{DeviceName}'";
                }
            });
        }
#pragma warning restore VSTHRD102
#pragma warning restore IDE0079
    }
}