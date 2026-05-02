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
        /// Occurs when the device connection is lost
        /// </summary>
        event EventHandler? Disconnected;
        /// <summary>
        /// Occurs when the listening timeout is exceeded
        /// (no token received within the specified interval).
        /// </summary>
        event ElapsedEventHandler? ListeningTimeoutElapsed;

        /// <summary>
        /// Occurs when a token is received from the Bluetooth device during listening.
        /// Subscribe to this event before calling <see cref="StartListening"/>.
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
        /// Gets the characteristic used for receiving token streams during listening mode.
        /// </summary>
        TCharacteristic ListeningCharacteristic { get; }

        /// <summary>
        /// Gets a value indicating whether listening is currently in progress.
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
        Task StartAsync(CancellationToken token = default);

        /// <summary>
        /// Sends a command to the Bluetooth device and waits for the response.
        /// </summary>
        /// <param name="command">The command string to send.</param>
        /// <returns>
        /// The response string received from the Bluetooth device,
        /// or null if the device does not respond within <see cref="ResponseTimeout">.
        /// </returns>
        /// <param name="token">A token to cancel the operation.</param>
        /// <exception cref="CharacteristicException">
        /// Thrown when Bluetooth errors occur
        /// </exception>
        /// <exception cref="TimeoutException">
        /// Thrown when the device doesn't respond within the expected timeframe.
        /// </exception>
        Task<string?> SendCommandAsync(string command, CancellationToken token = default);

        /// <summary>
        /// Starts listening for a token stream from the Bluetooth device.
        /// Each received token will raise the <see cref="ListeningTokenReceived"/> event.
        /// </summary>
        /// <param name="timeout">
        /// A timeout that specifies the maximum allowed interval between consecutive tokens.
        /// If the interval exceeds this value,
        /// the <see cref="ListeningTimeoutElapsed"/> event is raised.
        /// </param>
        /// <remarks>
        /// Subscribe to <see cref="ListeningTokenReceived"/> before calling this method.
        /// Listening continues until <see cref="StopListening"/> is called.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when characteristics are not set.
        /// </exception>
        void StartListening(TimeSpan timeout);

        /// <summary>
        /// Stops the ongoing listening process.
        /// No further <see cref="ListeningTokenReceived"/> events will be raised after this call.
        /// </summary>
        /// <remarks>
        /// Has no effect if listening is not currently active (check <see cref="IsListening"/>).
        /// </remarks>
        void StopListening();
    }
}
