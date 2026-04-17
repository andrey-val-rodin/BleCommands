using BleCommands.Core;
using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using System.Timers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows
{
    /// <inheritdoc />
    public class BleTransport : IBleTransport<GattCharacteristic>
    {
        private TaskCompletionSource<string>? _pendingRequest;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed;

        /// <summary>
        /// A constructor.
        /// </summary>
        /// <param name="commandCharacteristic">Characteristic for sending commands to the device (Write or WriteWithoutResponse).</param>
        /// <param name="responseCharacteristic">Characteristic for receiving command responses from the device (Notify or Indicate).</param>
        /// <param name="listeningCharacteristic">Characteristic for receiving token stream during listening (Notify or Indicate).</param>
        /// <param name="tokenDelimiter">Token separator. Typically, character '\n' is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if any characteristic is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any characteristic has invalid properties.</exception>
        public BleTransport(
            ICharacteristic<GattCharacteristic> commandCharacteristic,
            ICharacteristic<GattCharacteristic> responseCharacteristic,
            ICharacteristic<GattCharacteristic> listeningCharacteristic,
            char tokenDelimiter = TokenAggregator.DefaultTokenDelimiter)
        {
            ArgumentNullException.ThrowIfNull(commandCharacteristic);
            ArgumentNullException.ThrowIfNull(responseCharacteristic);
            ArgumentNullException.ThrowIfNull(listeningCharacteristic);

            if (!commandCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Write) &&
                !commandCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse))
                throw new ArgumentException(
                    "{nameof(commandCharacteristic)} is neither Write nor Write without response.",
                    nameof(commandCharacteristic));
            if (!responseCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Notify) &&
                !responseCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Indicate))
                throw new ArgumentException(
                    $"{nameof(responseCharacteristic)} is neither Update nor Indicate.",
                    nameof(responseCharacteristic));
            if (!listeningCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Notify) &&
                !listeningCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Indicate))
                throw new ArgumentException(
                    $"{nameof(listeningCharacteristic)} is neither Update nor Indicate.",
                    nameof(listeningCharacteristic));

            if (responseCharacteristic.TokenAggregator != null)
                throw new ArgumentException(
                    $"{nameof(responseCharacteristic)} has attached TokenAggregator already.",
                    nameof(responseCharacteristic));
            if (listeningCharacteristic.TokenAggregator != null)
                throw new ArgumentException(
                    $"{nameof(listeningCharacteristic)} has attached TokenAggregator already.",
                    nameof(listeningCharacteristic));

            CommandCharacteristic = commandCharacteristic;
            ResponseCharacteristic = responseCharacteristic;
            ListeningCharacteristic = listeningCharacteristic;
            TokenDelimiter = tokenDelimiter;

            if (ResponseCharacteristic == ListeningCharacteristic)
            {
                ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator());
            }
            else
            {
                ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator());
                ListeningCharacteristic.AttachTokenAggregator(new TokenAggregator());
            }
        }

        /// <inheritdoc />
        public event ElapsedEventHandler? ListeningTimeoutElapsed;

        /// <inheritdoc />
        public event EventHandler<TextEventArgs>? ListeningTokenReceived;

        /// <inheritdoc />
        public char TokenDelimiter { get; }

        /// <inheritdoc />
        public ICharacteristic<GattCharacteristic> CommandCharacteristic { get; }

        /// <inheritdoc />
        public ICharacteristic<GattCharacteristic> ResponseCharacteristic { get; }

        protected TokenAggregator? ResponseAggregator => ResponseCharacteristic.TokenAggregator;

        /// <inheritdoc />
        public ICharacteristic<GattCharacteristic> ListeningCharacteristic { get; }

        protected TokenAggregator? ListeningAggregator => ListeningCharacteristic.TokenAggregator;

        /// <inheritdoc />
        public bool IsListening { get; protected set; }

        public bool IsStarted { get; protected set; }

        // TODO: describe exceptions
        /// <inheritdoc />
        public async Task StartAsync(CancellationToken token = default)
        {
            if (IsStarted)
                return;

            await ResponseCharacteristic.StartUpdatesAsync(token).ConfigureAwait(false);
            if (ResponseCharacteristic != ListeningCharacteristic)
                await ListeningCharacteristic.StartUpdatesAsync(token).ConfigureAwait(false);

            IsStarted = true;
        }

        /// <inheritdoc />
        public async Task<string> SendCommandAsync(string command, CancellationToken token = default)
        {
            System.Diagnostics.Debug.WriteLine($"Command: {command}");

            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            if (ResponseAggregator != null)
                ResponseAggregator.TokenReceived += BleTransport_TokenReceived;
            var tcs = new TaskCompletionSource<string>();
            Interlocked.Exchange(ref _pendingRequest, tcs);

            try
            {
                // Append terminator
                command += TokenDelimiter;
                await CommandCharacteristic.WriteAsync(command, token).ConfigureAwait(false);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                using (cts.Token.Register(() => _pendingRequest?.TrySetCanceled()))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                _pendingRequest = null;
                if (ResponseAggregator != null)
                    ResponseAggregator.TokenReceived -= BleTransport_TokenReceived;
                _semaphore.Release();
            }
        }

        private void BleTransport_TokenReceived(object? sender, TextEventArgs e)
        {
            var tcs = Interlocked.Exchange(ref _pendingRequest, null);
            tcs?.TrySetResult(e.Text);
        }

        /// <inheritdoc />
        public void StartListening(TimeSpan? timeout = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void StopListening()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CommandCharacteristic?.Dispose();
                    ResponseCharacteristic?.Dispose();
                    ListeningCharacteristic?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
