namespace BleCommands.Core.Enums
{
    /// <summary>
    /// Provides a collection of flags representing the GATT Characteristic Properties.
    /// </summary>
    public enum CharacteristicPropertyFlags
    {
        /// <summary>
        /// The characteristic supports broadcasting.
        /// </summary>
        Broadcast = 1,
        /// <summary>
        /// The characteristic is readable.
        /// </summary>
        Read = 2,
        /// <summary>
        /// The characteristic supports Write Without Response.
        /// </summary>
        WriteWithoutResponse = 4,
        /// <summary>
        /// The characteristic is writable.
        /// </summary>
        Write = 8,
        /// <summary>
        /// The characteristic is notifiable.
        /// </summary>
        Notify = 0x10,
        /// <summary>
        /// The characteristic is indicatable.
        /// </summary>
        Indicate = 0x20,
        /// <summary>
        /// The characteristic supports signed writes.
        /// </summary>
        AuthenticatedSignedWrites = 0x40,
        /// <summary>
        /// The ExtendedProperties Descriptor is present.
        /// </summary>
        ExtendedProperties = 0x80,
        /// <summary>
        /// <para><b>Windows:</b> The characteristic supports reliable writes.</para>
        /// <para><b>iOS:</b> Only trusted devices can enable notifications of the characteristic's value.</para>
        /// </summary>
        NotifyEncryptionRequired = 0x100,
        /// <summary>
        /// <para><b>Windows:</b> The characteristic has writable auxiliaries.</para>
        /// <para><b>iOS:</b> Only trusted devices can enable indications of the characteristic's value.</para>
        /// </summary>
        IndicateEncryptionRequired = 0x200
    }
}
