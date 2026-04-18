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
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private System.Timers.Timer? _listeningTimer;
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
        public event ElapsedEventHandler? ListeningTimeoutElapsed
        {
            add
            {
                if (_listeningTimer != null)
                    _listeningTimer.Elapsed += value;
            }
            remove
            {
                if (_listeningTimer != null)
                    _listeningTimer.Elapsed -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<TextEventArgs>? ListeningTokenReceived
        {
            add
            {
                ListeningAggregator.TokenReceived += value;
            }
            remove
            {
                ListeningAggregator.TokenReceived -= value;
            }
        }

        /// <inheritdoc />
        public char TokenDelimiter { get; }

        /// <inheritdoc />
        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromMilliseconds(500);


        /// <inheritdoc />
        public ICharacteristic<GattCharacteristic> CommandCharacteristic { get; }

        /// <inheritdoc />
        public ICharacteristic<GattCharacteristic> ResponseCharacteristic { get; }

        protected TokenAggregator ResponseAggregator => ResponseCharacteristic.TokenAggregator!;

        /// <inheritdoc />
        public ICharacteristic<GattCharacteristic> ListeningCharacteristic { get; }

        protected TokenAggregator ListeningAggregator => ListeningCharacteristic.TokenAggregator!;

        /// <inheritdoc />
        public bool IsListening { get; protected set; }

        public bool IsStarted { get; protected set; }

        // TODO: describe exceptions
        /// <inheritdoc />
        public async Task StartAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (IsStarted)
                return;

            await ResponseCharacteristic.StartUpdatesAsync(token).ConfigureAwait(false);
            if (ResponseCharacteristic != ListeningCharacteristic)
                await ListeningCharacteristic.StartUpdatesAsync(token).ConfigureAwait(false);

            IsStarted = true;
        }

        /// <inheritdoc />
        public async Task<string?> SendCommandAsync(string command, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!IsStarted)
                throw new InvalidOperationException("BleTransport has not been started.");

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Command: {command}");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif

            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            var tcs = new TaskCompletionSource<string>();

            void Handler(object? sender, TextEventArgs args)
            {
                tcs.TrySetResult(args.Text);
            }

            try
            {
                ResponseAggregator.TokenReceived += Handler;

                // Append terminator and send command
                await CommandCharacteristic.WriteAsync(command + TokenDelimiter, token).ConfigureAwait(false);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(ResponseTimeout);

                using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    var result = await tcs.Task.ConfigureAwait(false);
#if DEBUG
                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine(
                        $"[{command}] Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
#endif
                    return result;
                }
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            finally
            {
                ResponseAggregator.TokenReceived -= Handler;
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void StartListening(TimeSpan? timeout = null)
        {
            ThrowIfDisposed();
            if (!IsStarted)
                throw new InvalidOperationException("BleTransport has not been started.");
            if (IsListening)
                throw new InvalidOperationException("Listening is already in progress.");

            if (timeout != null)
            {
                _listeningTimer = new System.Timers.Timer() { Interval = timeout.Value.TotalMilliseconds };
                _listeningTimer.Start();
            }

            ListeningTokenReceived += ListeningHandler;
            IsListening = true;
        }

        private void ListeningHandler(object? sender, TextEventArgs args)
        {
            // Reset timer
            _listeningTimer?.Stop();
            _listeningTimer?.Start();
        }

        /// <inheritdoc />
        public void StopListening()
        {
            _listeningTimer?.Stop();
            _listeningTimer?.Dispose();
            _listeningTimer = null;
            ListeningTokenReceived -= ListeningHandler;
        }

        protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ListeningTokenReceived -= ListeningHandler;

                    CommandCharacteristic?.Dispose();
                    ResponseCharacteristic?.Dispose();
                    ListeningCharacteristic?.Dispose();

                    _listeningTimer?.Stop();
                    _listeningTimer?.Dispose();
                    _listeningTimer = null;
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
