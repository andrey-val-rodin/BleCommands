using System.Windows;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            
            Closing += (s, e) =>
            {
                if (DataContext is IDisposable disposable)
                    disposable.Dispose();
            };
        }
    }
}