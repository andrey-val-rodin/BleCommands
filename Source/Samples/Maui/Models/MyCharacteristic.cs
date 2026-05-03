using BleCommands.Core.Enums;
using BleCommands.Maui;
using NativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;

namespace MauiSample.Models
{
    public partial class MyCharacteristic(NativeCharacteristic nativeCharacteristic) : Characteristic(nativeCharacteristic)
    {
        public string Name
        {
            get
            {
                var name = NativeCharacteristic.Name;
                if (name == "Unknown characteristic")
                    name = "Custom Characteristic";
                return name;
            }
        }

        public string PropertiesString => GetPropertiesString(Properties);

        public static string GetPropertiesString(CharacteristicPropertyFlags properties)
        {
            if (properties == 0)
                return "No properties";

            var displayParts = new List<string>();

            if (properties.HasFlag(CharacteristicPropertyFlags.Read)) displayParts.Add("Read");
            if (properties.HasFlag(CharacteristicPropertyFlags.Write)) displayParts.Add("Write");
            if (properties.HasFlag(CharacteristicPropertyFlags.WriteWithoutResponse)) displayParts.Add("Write without response");
            if (properties.HasFlag(CharacteristicPropertyFlags.Notify)) displayParts.Add("Notify");
            if (properties.HasFlag(CharacteristicPropertyFlags.Indicate)) displayParts.Add("Indicate");
            if (properties.HasFlag(CharacteristicPropertyFlags.Broadcast)) displayParts.Add("Broadcast");
            if (properties.HasFlag(CharacteristicPropertyFlags.AuthenticatedSignedWrites)) displayParts.Add("Signed writes");
            if (properties.HasFlag(CharacteristicPropertyFlags.ExtendedProperties)) displayParts.Add("Extended");
            if (properties.HasFlag(CharacteristicPropertyFlags.NotifyEncryptionRequired)) displayParts.Add("Notify with encryption");
            if (properties.HasFlag(CharacteristicPropertyFlags.IndicateEncryptionRequired)) displayParts.Add("Indicate wth encryption");

            return string.Join(", ", displayParts);
        }
    }
}
