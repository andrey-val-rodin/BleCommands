using BleCommands.Core.Contracts;
using BleCommands.Core.Events;
using System.Timers;

namespace BleCommands.Core
{
    /// <inheritdoc />
    public abstract class BleTransport<TDevice, TService, TCharacteristic>
        : IBleTransport<TDevice, TService, TCharacteristic>
        where TDevice : class, IDevice
        where TService : class, IService
        where TCharacteristic : class, ICharacteristic
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly object _timerLock = new();
        private readonly System.Timers.Timer _listeningTimer = new(3000) { AutoReset = false };
        private bool _disposed;

        /// <inheritdoc />
        public event EventHandler? Disconnected
        {
            add => Device.Disconnected += value;
            remove => Device.Disconnected -= value;
        }

        /// <inheritdoc />
        public event ElapsedEventHandler? ListeningTimeoutElapsed
        {
            add => _listeningTimer.Elapsed += value;
            remove => _listeningTimer.Elapsed -= value;
        }

        /// <inheritdoc />
        public event EventHandler<TextEventArgs>? ListeningTokenReceived
        {
            add => ListeningAggregator.TokenReceived += value;
            remove => ListeningAggregator.TokenReceived -= value;
        }

        /// <inheritdoc />
        public abstract TDevice Device { get; }

        /// <inheritdoc />
        public abstract TService Service { get; }

        /// <inheritdoc />
        public abstract TCharacteristic CommandCharacteristic { get; }

        /// <inheritdoc />
        public abstract TCharacteristic ResponseCharacteristic { get; }

        /// <inheritdoc />
        public abstract TCharacteristic ListeningCharacteristic { get; }

        /// <summary>
        /// The <see cref="TokenAggregator"/> instance used by <see cref="ResponseCharacteristic"/>.
        /// </summary>
        protected TokenAggregator ResponseAggregator =>
            ResponseCharacteristic.TokenAggregator ??
            throw new InvalidOperationException("TokenAggregator not attached to ResponseCharacteristic");

        /// <summary>
        /// The <see cref="TokenAggregator"/> instance used by <see cref="ListeningCharacteristic"/>.
        /// </summary>
        protected TokenAggregator ListeningAggregator =>
            ListeningCharacteristic.TokenAggregator ??
            throw new InvalidOperationException("TokenAggregator not attached to ListeningCharacteristic");

        /// <inheritdoc />
        public bool IsListening { get; protected set; }

        /// <inheritdoc />
        public char TokenDelimiter { get; set; }

        /// <inheritdoc />
        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);

        /// <summary>
        /// Returns <c>true</c> if the <see cref="StartAsync(CancellationToken)"/> method was called.
        /// </summary>
        public bool IsStarted { get; protected set; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (IsStarted)
                return;

            await ResponseCharacteristic.StartReceivingAsync(token).ConfigureAwait(false);
            if (ResponseCharacteristic != ListeningCharacteristic)
                await ListeningCharacteristic.StartReceivingAsync(token).ConfigureAwait(false);

            IsStarted = true;
        }

        /// <inheritdoc />
        public async Task<string?> SendCommandAsync(
            string command, CancellationToken token = default)
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

                // Send command
                await CommandCharacteristic.WriteAsync(command, token).ConfigureAwait(false);

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
        public void StartListening(TimeSpan timeout)
        {
            ThrowIfDisposed();

            if (!IsStarted)
                throw new InvalidOperationException("BleTransport has not been started.");

            lock (_timerLock)
            {
                if (IsListening)
                    throw new InvalidOperationException("Listening is already in progress.");

                _listeningTimer.Interval = timeout.TotalMilliseconds;
                ListeningTokenReceived += ListeningHandler;
                IsListening = true;
                _listeningTimer.Start();
            }
        }

        private void ListeningHandler(object? sender, TextEventArgs args)
        {
            if (_disposed)
                return;

            lock (_timerLock)
            {
                if (!IsListening)
                    return;

                // Reset timer
                _listeningTimer.Stop();
                _listeningTimer.Start();
            }
        }

        /// <inheritdoc />
        public void StopListening()
        {
            lock (_timerLock)
            {
                _listeningTimer.Stop();
                ListeningTokenReceived -= ListeningHandler;
                IsListening = false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    typeof(BleTransport<TDevice, TService, TCharacteristic>).FullName);
        }

        /// <summary>
        /// Releases managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _semaphore.Dispose();

                    ListeningTokenReceived -= ListeningHandler;

                    // The device will dispose all its services and characteristics.
                    // However, BleTransport owns all objects passed to the constructor,
                    // so we need to dispose them all.
                    CommandCharacteristic?.Dispose();
                    ResponseCharacteristic?.Dispose();
                    ListeningCharacteristic?.Dispose();
                    Service?.Dispose();

                    Device?.Dispose();

                    _listeningTimer?.Stop();
                    _listeningTimer?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
