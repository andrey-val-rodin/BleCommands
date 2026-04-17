namespace BleCommands.Core.Events
{
    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Text { get; }
    }
}
