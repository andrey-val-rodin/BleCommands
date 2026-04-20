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
            Assert.True(await client.BeginAsync("BLECommands device", cts.Token));
            Assert.NotNull(client.Transport);
            var response = await client.Transport.SendCommandAsync("HELLO", cts.Token);
        }
    }
}
