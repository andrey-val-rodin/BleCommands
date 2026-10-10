using BleCommands.Core.Events;
using System.Timers;

namespace BleCommands.Core.Contracts
{
    /// <summary>
    /// Provides Bluetooth communication capabilities with a connected device
    /// and properly releases all system resources after use.
    /// </summary>
    /// <typeparam name="TDevice">A specific device implementation.</typeparam>
    /// <typeparam name="TService">A specific service implementation.</typeparam>
    /// <typeparam name="TCharacteristic">A specific characteristic implementation.</typeparam>
    public interface IBleTransport<TDevice, TService, TCharacteristic> : IDisposable
        where TDevice : IDevice
        where TService : IService
        where TCharacteristic : ICharacteristic
    {
        /// <summary>
        /// Occurs when the device connection is lost.
        /// </summary>
        event EventHandler? Disconnected;

        /// <summary>
        /// Occurs when no listening token is received within the configured interval.
        /// </summary>
        /// <remarks>
        /// The interval is specified when a listening session is started.
        /// </remarks>
        event ElapsedEventHandler? ListeningTimeoutElapsed;

        /// <summary>
        /// Occurs when a listening token is received from the Bluetooth device.
        /// </summary>
        event EventHandler<TextEventArgs>? ListeningTokenReceived;

        /// <summary>
        /// Gets the Bluetooth LE device.
        /// </summary>
        TDevice Device { get; }

        /// <summary>
        /// Gets the service.
        /// </summary>
        TService Service { get; }

        /// <summary>
        /// Gets the characteristic used for sending commands to the Bluetooth device.
        /// </summary>
        TCharacteristic CommandCharacteristic { get; }

        /// <summary>
        /// Gets the characteristic used for receiving responses from the Bluetooth device.
        /// </summary>
        TCharacteristic ResponseCharacteristic { get; }

        /// <summary>
        /// Gets the characteristic used for receiving tokens from the Bluetooth device.
        /// </summary>
        TCharacteristic ListeningCharacteristic { get; }

        /// <summary>
        /// Gets a value indicating whether the transport has been successfully started.
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Gets a value indicating whether a listening session is currently in progress.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets the token separator. The default is '\n'.
        /// </summary>
        char TokenDelimiter { get; }

        /// <summary>
        /// Specifies period of time to wait for a response to command.
        /// </summary>
        TimeSpan ResponseTimeout { get; set; }

        /// <summary>
        /// Starts process of communication between Bluetooth transport and device.
        /// </summary>
        /// <param name="token">A token to cancel the operation.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the transport has already been started.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was canceled via <paramref name="token"/>.
        /// </exception>
        /// <remarks>
        /// This method is intended to be called once after the transport is created.
        /// If initialization fails, the transport should be disposed and recreated
        /// instead of attempting to start it again.
        /// </remarks>
        Task StartAsync(CancellationToken token = default);

        /// <summary>
        /// Sends a command to the Bluetooth device and waits for the response.
        /// </summary>
        /// <param name="command">The command string to send.</param>
        /// <param name="token">A token to cancel the operation.</param>
        /// <returns>
        /// The response string received from the Bluetooth device,
        /// or <c>null</c> if the device does not respond within <see cref="ResponseTimeout"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the transport has not been started.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the transport has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was canceled via <paramref name="token"/>.
        /// </exception>
        /// <remarks>
        /// Responses are matched to commands only by arrival order. The transport does not
        /// provide correlation IDs. If a command times out or is cancelled, a delayed response
        /// from that command may be received by a subsequent call to <see cref="SendCommandAsync"/>
        /// and cannot be distinguished from its response.
        /// Configure <see cref="ResponseTimeout"/> with sufficient margin for the device and
        /// communication conditions. For reliable correlation, the application protocol must
        /// include a command identifier in the command and response payloads.
        /// </remarks>
        Task<string?> SendCommandAsync(string command, CancellationToken token = default);

        /// <summary>
        /// Starts a listening session for tokens received from the Bluetooth device.
        /// </summary>
        /// <param name="timeout">
        /// The maximum allowed interval between consecutive tokens.
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
        void StartListening(TimeSpan timeout);

        /// <summary>
        /// Stops the ongoing listening session.
        /// </summary>
        /// <remarks>
        /// Has no effect if no listening session is active.
        /// </remarks>
        void StopListening();
    }
}
