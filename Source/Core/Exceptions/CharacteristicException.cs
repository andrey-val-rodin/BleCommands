namespace BleCommands.Core.Exceptions
{
    public class CharacteristicException : Exception
    {
        public CharacteristicException() : base()
        {
        }

        public CharacteristicException(string message) : base(message)
        {
        }
    }
}
