using BleCommands.Core;
using BleCommands.Core.Events;
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using BleTransport = BleCommands.Maui.BleTransport;

namespace IntegrationTests.Uwp
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
        public static readonly Guid DeviceUuid                  = new("00000000-0000-0000-8000-f8b3b722093e");
        public static readonly Guid ServiceUuid                 = new("0000ffe0-0000-1000-8000-00805f9b34fb");
        public static readonly Guid UpdatesCharacteristicUuid   = new("0000ffe1-0000-1000-8000-00805f9b34fb");
        public static readonly Guid WriteCharacteristicUuid     = new("0000ffe2-0000-1000-8000-00805f9b34fb");

        public static BleScanner BleScanner { get; } = new BleScanner();

        public static Device Device { get; private set; }

        public static Service Service { get; private set; }

        public static BleTransport BleTransport { get; private set; }

        public static Characteristic CommandCharacteristic { get; private set; }

        public static Characteristic ResponseCharacteristic { get; private set; }

        public static Characteristic ListeningCharacteristic { get; private set; }

        public static Characteristic CharacteristicWithAttachedAggregator { get; private set; }

        [AssemblyInitialize]
        public static async Task InitializeAsync(TestContext context)
        {
            var scanner = new BleScanner();
            Device = await scanner.FindDeviceAsync("Rotating Table");
            Assert.IsNotNull(Device, "Turn on Rotating Table!");
            await Device.ConnectAsync(context.CancellationToken);
            Assert.IsTrue(Device.IsConnected);
            /*
             * TODO: Instead of checking the connection status immediately, you should use the following code:
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.UtcNow;

            while (!device.IsConnected && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }

            if (!device.IsConnected)
                throw new TimeoutException("Device did not connect within timeout");
            */

            Service = await Device.GetServiceAsync(ServiceUuid, context.CancellationToken);
            Assert.IsNotNull(Service);
            CommandCharacteristic = await Service.GetCharacteristicAsync
                (WriteCharacteristicUuid, context.CancellationToken);
            Assert.IsNotNull(CommandCharacteristic);
            ListeningCharacteristic = ResponseCharacteristic =
                await Service.GetCharacteristicAsync(UpdatesCharacteristicUuid, context.CancellationToken);
            Assert.IsNotNull(ResponseCharacteristic);
            Assert.IsNotNull(ListeningCharacteristic);
            CharacteristicWithAttachedAggregator =
                await Service.GetCharacteristicAsync(UpdatesCharacteristicUuid, context.CancellationToken);
            CharacteristicWithAttachedAggregator.AttachTokenAggregator(new TokenAggregator());
            BleTransport = new BleTransport(
                Device,
                Service,
                CommandCharacteristic,
                ResponseCharacteristic,
                ListeningCharacteristic,
                '\n');

            await BleTransport.StartAsync(context.CancellationToken);
            await StopTableAsync();
        }

        public static async Task StopTableAsync()
        {
            var status = await BleTransport.SendCommandAsync("STATUS");
            if (status != "BUSY")
                return;

            var tcs = new TaskCompletionSource<bool>();
            BleTransport.ListeningTokenReceived += Handler;
            void Handler(object sender, TextEventArgs args)
            {
                if (args.Text == "END")
                    tcs.TrySetResult(true);
            }

            var response = await BleTransport.SendCommandAsync("STOP");
            if (response != "OK")
                tcs.TrySetResult(true);

            Assert.IsTrue(await tcs.Task);
            BleTransport.ListeningTokenReceived -= Handler;
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            BleScanner.Dispose();
            CommandCharacteristic?.Dispose();
            ResponseCharacteristic?.Dispose();
            ListeningCharacteristic?.Dispose();
            CharacteristicWithAttachedAggregator?.Dispose();
            BleTransport?.Dispose();
        }
    }
}
