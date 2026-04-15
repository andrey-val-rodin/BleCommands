namespace BleCommands.Core.Events
{
    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }
    }
}
