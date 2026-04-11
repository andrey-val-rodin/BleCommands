namespace Core.Contracts
{
    public class ValueUpdatedEventArgs
    {
        public ValueUpdatedEventArgs(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }
    }
}
