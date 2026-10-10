using BleCommands.Core.Contracts;
using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using System.Timers;

namespace BleCommands.Core
{
    /// <summary>
    /// Provides Bluetooth communication capabilities with a connected device running
    /// firmware based on the Arduino BleCommands library.
    /// </summary>
    /// <typeparam name="TDevice">A specific device implementation.</typeparam>
    /// <typeparam name="TService">A specific service implementation.</typeparam>
    /// <typeparam name="TCharacteristic">A specific characteristic implementation.</typeparam>
    /// <remarks>
    /// This class implements the standard BleCommands.Arduino layout with three
    /// distinct characteristics. Derived classes may override <see cref="StartAsync"/>,
    /// <see cref="StartListening"/>, and <see cref="StopListening"/> to support devices
    /// with a different characteristic layout.
    /// </remarks>
    public abstract class BleTransport<TDevice, TService, TCharacteristic>
        : IBleTransport<TDevice, TService, TCharacteristic>
        where TDevice : class, IDevice
        where TService : class, IService
        where TCharacteristic : class, ICharacteristic
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly object _timerLock = new();
        private readonly System.Timers.Timer _listeningTimer = new(3000) { AutoReset = false };
        private TimeSpan _responseTimeout = TimeSpan.FromMilliseconds(1000);
        private bool _disposed;

        /// <summary>
        /// Occurs when the device connection is lost.
        /// </summary>
        /// <remarks>
        /// This event is forwarded from <see cref="IDevice.Disconnected"/>.
        /// After the connection is lost, the transport should be disposed and
        /// recreated together with the underlying device.
        /// </remarks>
        public event EventHandler? Disconnected
        {
            add => Device.Disconnected += value;
            remove => Device.Disconnected -= value;
        }

        /// <summary>
        /// Occurs when the listening timeout is exceeded
        /// (no token received within the specified interval).
        /// </summary>
        public event ElapsedEventHandler? ListeningTimeoutElapsed
        {
            add => _listeningTimer.Elapsed += value;
            remove => _listeningTimer.Elapsed -= value;
        }


        /// <summary>
        /// Occurs when a listening token is received from the Bluetooth device.
        /// </summary>
        /// <remarks>
        /// The underlying subscription is established by <see cref="StartAsync"/>.
        /// Therefore, this event may fire immediately after the transport starts,
        /// regardless of whether a listening session is active.
        /// </remarks>
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
        /// Gets the <see cref="TokenAggregator"/> instance used by <see cref="ResponseCharacteristic"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="TokenAggregator"/> is attached to <see cref="ResponseCharacteristic"/>.
        /// </exception>
        /// <remarks>
        /// The aggregator is attached by <see cref="StartAsync"/>.
        /// Derived classes that override <see cref="StartAsync"/> must attach an aggregator themselves.
        /// </remarks>
        protected TokenAggregator ResponseAggregator =>
            ResponseCharacteristic.TokenAggregator ??
            throw new InvalidOperationException("TokenAggregator not attached to ResponseCharacteristic");

        /// <summary>
        /// Gets the <see cref="TokenAggregator"/> instance used by <see cref="ListeningCharacteristic"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="TokenAggregator"/> is attached to <see cref="ResponseCharacteristic"/>.
        /// </exception>
        /// <remarks>
        /// The aggregator is attached by <see cref="StartAsync"/>.
        /// Derived classes that override <see cref="StartAsync"/> must attach an aggregator themselves.
        /// </remarks>
        protected TokenAggregator ListeningAggregator =>
            ListeningCharacteristic.TokenAggregator ??
            throw new InvalidOperationException("TokenAggregator not attached to ListeningCharacteristic");


        /// <summary>
        /// Gets a value indicating whether this object has been initialized
        /// (i.e., whether the <see cref="StartAsync"/> method was called.)
        /// </summary>
        public bool IsStarted { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether listening is currently in progress.
        /// </summary>
        public bool IsListening { get; protected set; }

        /// <inheritdoc />
        public char TokenDelimiter { get; protected set; }

        /// <inheritdoc />
        public TimeSpan ResponseTimeout
        {
            get => _responseTimeout;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(
                        nameof(value), "Response timeout must be greater than zero.");

                _responseTimeout = value;
            }
        }

        /// <summary>
        /// Validates the constructor parameters of a derived transport.
        /// </summary>
        /// <remarks>
        /// Derived classes should call this method from their constructor to ensure
        /// that all characteristics have the required properties and no aggregator
        /// is attached prematurely.
        /// </remarks>
        /// <param name="device">A Bluetooth LE device.</param>
        /// <param name="service"> A service.</param>
        /// <param name="commandCharacteristic">
        /// Characteristic for sending commands to the device (Write or WriteWithoutResponse).
        /// </param>
        /// <param name="responseCharacteristic">
        /// Characteristic for receiving command responses from the device (Notify or Indicate).
        /// </param>
        /// <param name="listeningCharacteristic">
        /// Characteristic for receiving token stream during listening (Notify or Indicate).
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any characteristic has invalid properties.</exception>
        protected void VerifyParameters(
            TDevice device,
            TService service,
            TCharacteristic commandCharacteristic,
            TCharacteristic responseCharacteristic,
            TCharacteristic listeningCharacteristic)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (service == null) throw new ArgumentNullException(nameof(service));

            if (commandCharacteristic == null) throw new ArgumentNullException(nameof(commandCharacteristic));
            if (responseCharacteristic == null) throw new ArgumentNullException(nameof(responseCharacteristic));
            if (listeningCharacteristic == null) throw new ArgumentNullException(nameof(listeningCharacteristic));

            if (!commandCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.Write) &&
                !commandCharacteristic.Properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse))
                throw new ArgumentException(
                    $"{nameof(commandCharacteristic)} is neither Write nor Write without response.",
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
        }

        /// <summary>
        /// Starts the transport: attaches token aggregators to the response and listening
        /// characteristics and subscribes to both.
        /// </summary>
        /// <param name="token">A token to cancel the operation.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the transport has already been started.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the transport has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was canceled via <paramref name="token"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method must be called exactly once after the transport is created.
        /// If it is called again, an <see cref="InvalidOperationException"/> is thrown.
        /// </para>
        /// <para>
        /// If initialization fails, the transport should be disposed and recreated
        /// instead of attempting to start it again.
        /// </para>
        /// <para>
        /// In the base implementation, both <see cref="ResponseCharacteristic"/> and
        /// <see cref="ListeningCharacteristic"/> are subscribed. Derived classes may override
        /// this method to change the subscription strategy, for example if the device uses
        /// a single characteristic for both responses and listening tokens.
        /// </para>
        /// </remarks>
        public virtual async Task StartAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (IsStarted)
                throw new InvalidOperationException("BleTransport has already been started.");

            ResponseCharacteristic.AttachTokenAggregator(new TokenAggregator(TokenDelimiter));
            ListeningCharacteristic.AttachTokenAggregator(new TokenAggregator(TokenDelimiter));

            await ResponseCharacteristic.StartReceivingAsync(token).ConfigureAwait(false);
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif

            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            var tcs = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void Handler(object? sender, TextEventArgs args)
            {
                tcs.TrySetResult(args.Text);
            }

            try
            {
                ResponseAggregator.TokenReceived += Handler;

                // Send command
                await CommandCharacteristic.WriteAsync(command, token).ConfigureAwait(false);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeoutCts.CancelAfter(ResponseTimeout);

                await Task.WhenAny(
                    tcs.Task,
                    Task.Delay(Timeout.InfiniteTimeSpan, timeoutCts.Token)).ConfigureAwait(false);

                if (tcs.Task.IsCompleted)
                {
#if DEBUG
                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine(
                        $"[{command}] Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
#endif
                    return await tcs.Task.ConfigureAwait(false);
                }

                token.ThrowIfCancellationRequested();
                return null;
            }
            finally
            {
                ResponseAggregator.TokenReceived -= Handler;
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Starts listening session and begins tracking the interval between messages.
        /// </summary>
        /// <param name="timeout">
        /// A timeout that specifies the maximum allowed interval between consecutive tokens.
        /// If the interval exceeds this value,
        /// the <see cref="ListeningTimeoutElapsed"/> event is raised.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the transport has not been started or listening is already in progress.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeout"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the transport has been disposed.
        /// </exception>
        /// <remarks>
        /// <para>
        /// A listening session is a logical concept: it controls the timeout timer and
        /// the <see cref="IsListening"/> flag. The underlying subscription is established
        /// by <see cref="StartAsync"/> and is not affected by this method.
        /// </para>
        /// <para>
        /// Subscribe to <see cref="ListeningTokenReceived"/> and
        /// <see cref="ListeningTimeoutElapsed"/> before calling this method.
        /// </para>
        /// <para>
        /// Call this method when you are about to start receiving
        /// periodic messages from the Bluetooth device.
        /// The session continues until <see cref="StopListening"/> is called.
        /// </para>
        /// </remarks>
        public void StartListening(TimeSpan timeout)
        {
            ThrowIfDisposed();

            if (!IsStarted)
                throw new InvalidOperationException("BleTransport has not been started.");

            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(timeout), "Listening timeout must be greater than zero.");

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

        /// <summary>
        /// Stops the ongoing listening session.
        /// </summary>
        /// <remarks>
        /// <para>
        /// No new <see cref="ListeningTimeoutElapsed"/> callbacks will be raised
        /// after this call.
        /// A timeout callback that has already started may still complete.
        /// The <see cref="ListeningTokenReceived"/> event will still be raised after this method is called.
        /// </para>
        /// <para>
        /// Has no effect if listening is not currently active (check <see cref="IsListening"/>).
        /// </para>
        /// </remarks>
        public void StopListening()
        {
            if (!IsListening)
                return;

            lock (_timerLock)
            {
                _listeningTimer.Stop();
                ListeningTokenReceived -= ListeningHandler;
                IsListening = false;
            }
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this transport has been disposed.
        /// </summary>
        /// <remarks>
        /// Derived classes overriding <see cref="StartAsync"/> or other public methods
        /// should call this method at the beginning of their implementation.
        /// </remarks>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Releases managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method disposes all owned objects including <see cref="Device"/>,
        /// <see cref="Service"/>, and all characteristics. While the device's 
        /// <see cref="IDisposable.Dispose">Dispose</see> implementation should properly 
        /// dispose its child objects, explicit disposal is performed to ensure cleanup 
        /// even if objects were created independently.
        /// </para>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    lock (_timerLock)
                    {
                        var listeningAggregator = ListeningCharacteristic?.TokenAggregator;
                        if (listeningAggregator != null)
                            listeningAggregator.TokenReceived -= ListeningHandler;
                        _listeningTimer.Dispose();
                        IsListening = false;
                    }

                    _semaphore.Dispose();

                    CommandCharacteristic?.Dispose();
                    ResponseCharacteristic?.Dispose();
                    ListeningCharacteristic?.Dispose();
                    Service?.Dispose();
                    Device?.Dispose();
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
