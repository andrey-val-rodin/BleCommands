using BleCommands.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Wpf
{
    public partial class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private string _command = string.Empty;
        private string _response = string.Empty;
        private Brush _responseBrush = Brushes.Black;
        private Brush _listeningTokensBrush = Brushes.White;
        private bool _isMessagingInProgress;

        public MainWindowViewModel()
        {
            if (BleTransport != null)
            {
                BleTransport.ListeningTimeoutElapsed += BleTransport_ListeningTimeoutElapsed;
                BleTransport.ListeningTokenReceived += BleTransport_ListeningTokenReceived;
                BleTransport.StartListening(TimeSpan.FromSeconds(1));
            }

            Command = AvailableCommands[0];
        }

        public BleTransport? BleTransport { get; } = App.ServiceProvider?.GetRequiredService<BleTransportHolder>()?.Transport;

        public ObservableCollection<string> AvailableCommands { get; set; } = ["START", "STOP"];

        public ObservableCollection<string> ListeningTokensList { get; } = [];

        public string Command
        {
            get => _command;
            set
            {
                if (SetProperty(ref _command, value))
                    Response = string.Empty;
            }
        }

        public string Response
        {
            get => _response;
            set => SetProperty(ref _response, value);
        }

        public Brush ResponseBrush
        {
            get => _responseBrush;
            set => SetProperty(ref _responseBrush, value);
        }

        public Brush ListeningTokensBrush
        {
            get => _listeningTokensBrush;
            set => SetProperty(ref _listeningTokensBrush, value);
        }

        public bool IsMessagingInProgress
        {
            get => _isMessagingInProgress;
            set
            {
                if (SetProperty(ref _isMessagingInProgress, value))
                {
                    if (value)
                    {
                        ListeningTokensBrush = Brushes.White;
                    }
                    else
                    {
                        ListeningTokensBrush = Brushes.Gray;
                        Response = string.Empty;
                    }
                }
            }
        }

        [RelayCommand]
        public async Task SendAsync()
        {
            if (BleTransport == null)
                return;

            var response = await BleTransport.SendCommandAsync(Command);
            if (response == "OK")
            {
                if (Command == "START")
                    IsMessagingInProgress = true;
                Response = response;
                ResponseBrush = Brushes.Black;
            }
            else 
            {
                Response = response ?? string.Empty;
                ResponseBrush = Brushes.Red;
            }
        }
        
        [RelayCommand]
        public void Clear()
        {
            ListeningTokensList.Clear();
        }

        #region Handlers
#pragma warning disable IDE0079
#pragma warning disable VSTHRD102
        private void BleTransport_ListeningTimeoutElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            App.JoinableTaskFactory.Run(async delegate
            {
                await App.JoinableTaskFactory.SwitchToMainThreadAsync();
                MessageBox.Show("Listening timeout");
            });
        }

        private void BleTransport_ListeningTokenReceived(object? sender, BleCommands.Core.Events.TextEventArgs e)
        {
            App.JoinableTaskFactory.Run(async delegate
            {
                await App.JoinableTaskFactory.SwitchToMainThreadAsync();
                ListeningTokensList.Add(e.Text);
                if (e.Text == "END")
                    IsMessagingInProgress = false;
            });
        }
#pragma warning restore VSTHRD102
#pragma warning restore IDE0079
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action? onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            System.Diagnostics.Debug.WriteLine($"RaisePropertyChanged: {propertyName}, new value: {value}");
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void Dispose()
        {
            if (BleTransport != null)
            {
                BleTransport.ListeningTimeoutElapsed -= BleTransport_ListeningTimeoutElapsed;
                BleTransport.ListeningTokenReceived -= BleTransport_ListeningTokenReceived;
                BleTransport.StopListening();
            }
            GC.SuppressFinalize(this);
        }
    }
}
