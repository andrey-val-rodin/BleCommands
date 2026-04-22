using BleCommands.Core.Events;
using BleCommands.Windows;

namespace BleCommands.Tests.Windows
{
    public class BleCommandsClientTests
    {
        [Fact]
        public async Task StartAsync()
        {
            using var cts = new CancellationTokenSource();
            using var client = new BleCommandsClient();
            Assert.True(await client.BeginAsync("BLECommands Device", cts.Token));
            Assert.NotNull(client.Transport);
            var response = await client.Transport.SendCommandAsync("", cts.Token);
            response = await client.Transport.SendCommandAsync("STATE", cts.Token);
            response = await client.Transport.SendCommandAsync("ON", cts.Token);
            response = await client.Transport.SendCommandAsync("STATE", cts.Token);
            response = await client.Transport.SendCommandAsync("OFF", cts.Token);
            response = await client.Transport.SendCommandAsync("STATE", cts.Token);
            response = await client.Transport.SendCommandAsync("TOGGLE", cts.Token);
            response = await client.Transport.SendCommandAsync("STATE", cts.Token);

            response = await client.Transport.SendCommandAsync("PING", cts.Token);
            response = await client.Transport.SendCommandAsync("HELLO", cts.Token);
            response = await client.Transport.SendCommandAsync("ECHO", cts.Token);
            response = await client.Transport.SendCommandAsync("ECHO WITH ARGUMENTS", cts.Token);
            response = await client.Transport.SendCommandAsync("STATUS", cts.Token);
            response = await client.Transport.SendCommandAsync("GET  ", cts.Token);
            response = await client.Transport.SendCommandAsync("GET MAC", cts.Token);
            response = await client.Transport.SendCommandAsync("GET NAME", cts.Token);
        }

        [Fact]
        public async Task LongText()
        {
            using var cts = new CancellationTokenSource();
            using var client = new BleCommandsClient();
            Assert.True(await client.BeginAsync("BLECommands Device", cts.Token));
            Assert.NotNull(client.Transport);
            var response = await client.Transport.SendCommandAsync("GET_LONG", cts.Token);
        }
    }
}
