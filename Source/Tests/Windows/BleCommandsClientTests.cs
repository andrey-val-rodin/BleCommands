using BleCommands.Core.Enums;
using BleCommands.Core.Events;
using BleCommands.Windows;

namespace BleCommands.Tests.Windows
{
    public class BleCommandsClientTests
    {
        /*
        [Fact]
        public async Task StartAsync()
        {
            using var cts = new CancellationTokenSource();
            using var holder = await BleCommandsClient.CreateTransportAsync("BLECommands Device", cts.Token);
            var transport = holder?.Transport;
            Assert.NotNull(transport);
            var response = await transport.SendCommandAsync("", cts.Token);
            response = await transport.SendCommandAsync("STATE", cts.Token);
            response = await transport.SendCommandAsync("ON", cts.Token);
            response = await transport.SendCommandAsync("STATE", cts.Token);
            response = await transport.SendCommandAsync("OFF", cts.Token);
            response = await transport.SendCommandAsync("STATE", cts.Token);
            response = await transport.SendCommandAsync("TOGGLE", cts.Token);
            response = await transport.SendCommandAsync("STATE", cts.Token);

            response = await transport.SendCommandAsync("PING", cts.Token);
            response = await transport.SendCommandAsync("HELLO", cts.Token);
            response = await transport.SendCommandAsync("ECHO", cts.Token);
            response = await transport.SendCommandAsync("ECHO WITH ARGUMENTS", cts.Token);
            response = await transport.SendCommandAsync("STATUS", cts.Token);
            response = await transport.SendCommandAsync("GET  ", cts.Token);
            response = await transport.SendCommandAsync("GET MAC", cts.Token);
            response = await transport.SendCommandAsync("GET NAME", cts.Token);
            response = await transport.SendCommandAsync("GET_LONG", cts.Token);
        }
        */
    }
}
