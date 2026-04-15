using System.Timers;

namespace BleCommands.Core.Contracts
{
    public interface IBleTransport<TDevice, TService, TCharacteristic>
    {
        event ElapsedEventHandler? ListeningTimeoutElapsed;

        bool IsListening { get; }

        BleStream? ListeningStream { get; }

        Task<string> SendCommandAsync(string command);
    }
}
