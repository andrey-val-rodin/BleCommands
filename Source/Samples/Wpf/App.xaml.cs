using Microsoft.VisualStudio.Threading;
using System.Windows;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static JoinableTaskFactory JoinableTaskFactory { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var joinableTaskContext = new JoinableTaskContext();
            JoinableTaskFactory = joinableTaskContext.Factory;
        }
    }
}
