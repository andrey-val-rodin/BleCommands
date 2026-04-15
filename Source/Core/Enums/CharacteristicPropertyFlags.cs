namespace BleCommands.Core.Enums
{
    public enum CharacteristicPropertyFlags
    {
        Broadcast = 1,
        Read = 2,
        WriteWithoutResponse = 4,
        Write = 8,
        Notify = 0x10,
        Indicate = 0x20,
        AuthenticatedSignedWrites = 0x40,
        ExtendedProperties = 0x80,
        NotifyEncryptionRequired = 0x100,
        IndicateEncryptionRequired = 0x200
    }
}
