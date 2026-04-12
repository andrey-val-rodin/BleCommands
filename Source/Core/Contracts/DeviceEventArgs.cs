namespace Core.Contracts
{
    public class DeviceEventArgs : EventArgs
    {
        // TDOD: remove this class
        public DeviceEventArgs(IDevice device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public IDevice Device { get; }
    }
}
