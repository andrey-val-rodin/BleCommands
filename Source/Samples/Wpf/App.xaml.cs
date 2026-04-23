using BleCommands.Windows;
using Microsoft.Extensions.DependencyInjection;
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

        public static ServiceProvider? ServiceProvider { get; private set; }

#pragma warning disable IDE0079
#pragma warning disable VSTHRD104
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var joinableTaskContext = new JoinableTaskContext();
            JoinableTaskFactory = joinableTaskContext.Factory;

            JoinableTaskFactory.Run(async () =>
            {
                const string deviceName = "BLECommands Messaging Example";
                var transportHolder = await BleCommandsClient.CreateTransportAsync(deviceName);
                if (transportHolder == null)
                {
                    MessageBox.Show($"Failed to connect with device '{deviceName}'");
                    Shutdown();
                    return;
                }

                await transportHolder.Transport.BeginAsync();
                var services = new ServiceCollection();
                services.AddSingleton(transportHolder);
                ServiceProvider = services.BuildServiceProvider();

                return;
            });
        }
#pragma warning restore VSTHRD102
#pragma warning restore IDE0079

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            ServiceProvider?.Dispose();
        }
    }
}
