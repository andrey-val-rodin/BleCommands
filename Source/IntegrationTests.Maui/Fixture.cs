using BleCommands.Core;
using BleCommands.Core.Events;
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BleTransport = BleCommands.Maui.BleTransport;

namespace IntegrationTests.Maui
{

    /// <summary>
    /// The test fixture uses real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public static class Fixture
    {
        public const string Id = "BluetoothLE#BluetoothLE90:e8:68:ad:f0:54-f8:b3:b7:22:09:3e";
        public const ulong MacAddress = 0xf8b3b722093e;
        public static readonly Guid DeviceUuid                  = new("00000000-0000-0000-0000-f8b3b722093e");
        public static readonly Guid ServiceUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B901");
        public static readonly Guid CommandCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B902");
        public static readonly Guid ResponseCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B903");
        public static readonly Guid ListeningCharacteristicUuid = new("DB341FB3-8977-4C2D-AC6C-74540BD8B904");
        private static readonly List<IDisposable> _disposableObjects = [];

        public static BleScanner BleScanner { get; } = new BleScanner();

        public static Device? Device { get; private set; }

        public static Service? Service { get; private set; }

        public static BleTransport? BleTransport { get; private set; }

        public static Characteristic? CommandCharacteristic { get; private set; }

        public static Characteristic? ResponseCharacteristic { get; private set; }

        public static Characteristic? ListeningCharacteristic { get; private set; }

        public static Characteristic? CharacteristicWithAttachedAggregator { get; private set; }

        [AssemblyInitialize]
        public static async Task InitializeAsync(TestContext context)
        {
            var transport = await ArduinoClient.CreateTransportAsync(
                "Rotating Table", context.CancellationToken);
            Assert.IsNotNull(transport, "Turn on Rotating Table!");

            BleTransport = transport;
            Device = (Device)BleTransport.Device;
            RegisterDisposableObject(Device);
            Service = (Service)BleTransport.Service;
            RegisterDisposableObject(Service);
            CommandCharacteristic = (Characteristic)BleTransport.CommandCharacteristic;
            ResponseCharacteristic = (Characteristic)BleTransport.ResponseCharacteristic;
            ListeningCharacteristic = (Characteristic)BleTransport.ListeningCharacteristic;
            RegisterDisposableObject(CommandCharacteristic);
            RegisterDisposableObject(ResponseCharacteristic);
            RegisterDisposableObject(ListeningCharacteristic);
            CharacteristicWithAttachedAggregator = 
                (await Service.GetCharacteristicAsync(
                    ListeningCharacteristicUuid, context.CancellationToken))!;
            Assert.IsNotNull(CharacteristicWithAttachedAggregator);
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());
            RegisterDisposableObject(CharacteristicWithAttachedAggregator);

            await BleTransport.StartAsync(context.CancellationToken);
            await StopTableAsync();
        }

        public static async Task StopTableAsync()
        {
            Assert.IsNotNull(BleTransport);

            var status = await BleTransport.SendCommandAsync("STATUS");
            if (status != "BUSY")
                return;

            var tcs = new TaskCompletionSource<bool>();
            BleTransport.ListeningTokenReceived += Handler;
            void Handler(object? sender, TextEventArgs args)
            {
                if (args.Text == "END")
                    tcs.TrySetResult(true);
            }

            await BleTransport.SendCommandAsync("STOP");

            Assert.IsTrue(await tcs.Task);
            BleTransport.ListeningTokenReceived -= Handler;
        }

        private static async Task RetrieveObjectsToCheckCorrectDisposingAsync()
        {
            Assert.IsNotNull(Device);
            var services = await Device.GetServicesAsync();
            foreach (var service in services)
            {
                _disposableObjects.Add(service);
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    _disposableObjects.Add(characteristic);
                }
            }
        }

        private static void RegisterDisposableObject(IDisposable obj)
        {
            Assert.IsNotNull(obj);
            _disposableObjects.Add(obj);
        }

        private static void VerifyDisposeWasCalled(object obj)
        {
            var type = obj.GetType();

#pragma warning disable IDE0079
#pragma warning disable IL2075
            var disposedField = type.GetField("_disposed",
                BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore IL2075
#pragma warning restore IDE0079

            var value = disposedField?.GetValue(obj);
            if (value != null)
            {
                var isDisposed = (bool)value;
                Assert.IsTrue(isDisposed, $"The {type.Name} object was not disposed");
            }
            else
            {
                if (obj is IDisposable)
                {
                    Assert.Fail($"The {type.Name} object implements IDisposable, " +
                        "but the _disposed field was not found.");
                }
            }
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            BleTransport?.Dispose();

            // Check whether all objects were disposed
            foreach (var obj in _disposableObjects)
            {
                VerifyDisposeWasCalled(obj);
            }
        }
    }
}
