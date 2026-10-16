using BleCommands.Core.Enums;
using BleCommands.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using NativeCharacteristic = Plugin.BLE.Abstractions.Contracts.ICharacteristic;

namespace MauiSample.Models
{
    public partial class MyCharacteristic(NativeCharacteristic nativeCharacteristic)
        : ObservableObject
    {
        private bool _isNotifying = false;

        public Characteristic Characteristic { get; } = new Characteristic(nativeCharacteristic);
        private NativeCharacteristic NativeCharacteristic { get; } = nativeCharacteristic;

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

        public Guid Id => Characteristic.Id;

        public bool CanUpdate => Characteristic.CanUpdate;

        public bool CanRead => Characteristic.CanRead;

        public bool CanWrite => Characteristic.CanWrite;

        public string PropertiesString => GetPropertiesString(Characteristic.Properties);

        [ObservableProperty]
        public partial string NotifyValue { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ReadValue { get; set; } = string.Empty;

        public bool IsNotifying
        {
            get { return _isNotifying; }
            set
            {
                if (SetProperty(ref _isNotifying, value))
                {
                    if (_isNotifying)
                        Characteristic.ValueReceived += MyCharacteristic_ValueReceived;
                    else
                        Characteristic.ValueReceived -= MyCharacteristic_ValueReceived;
                }
            }
        }

        private void MyCharacteristic_ValueReceived(object? sender, BleCommands.Core.Events.ByteArrayEventArgs e)
        {
            NotifyValue = Characteristic.ConvertToString(e.Value);
        }

        public async Task InitializeAsync()
        {
            if (Characteristic.CanUpdate)
            {
                await Characteristic.StartReceivingAsync();
                IsNotifying = true;
            }
        }
        public async Task<string> ReadAsync(CancellationToken token = default)
        {
            return await Characteristic.ReadAsync(token);
        }

        public async Task WriteAsync(string text, CancellationToken token = default)
        {
            await Characteristic.WriteAsync(text, token);
        }

        private static string GetPropertiesString(CharacteristicPropertyFlags properties)
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
