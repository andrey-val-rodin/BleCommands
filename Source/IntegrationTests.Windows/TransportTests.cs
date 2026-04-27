using BleCommands.Core.Events;
using BleCommands.Windows;

namespace BleCommands.IntegrationTests.Windows
{
    public sealed class TransportTests(Fixture fixture)
    {
        private Fixture Fixture { get; } = fixture;

        private BleTransport BleTransport => Fixture.BleTransport;

        [Fact]
        public async Task SendCommandAsync_Status_ValidResponse()
        {
            Assert.Equal("READY", await BleTransport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task SendCommandAsync_InsufficientTimeout_Null()
        {
            var oldTimeout = BleTransport.ResponseTimeout;
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object? sender, TextEventArgs args)
            {
                tcs.SetResult(true);
            }

            try
            {
                BleTransport.ResponseTimeout = TimeSpan.FromMilliseconds(1);
                BleTransport.ListeningTokenReceived += Handler;
                Assert.Null(await BleTransport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken));

                // Wait until we receive an actual response
                await tcs.Task;
            }
            finally
            {
                BleTransport.ListeningTokenReceived -= Handler;
                BleTransport.ResponseTimeout = oldTimeout;
            }
        }

        [Fact]
        public async Task ListeningIsInProgress_RotateTable_ReceiveTokens()
        {
            Assert.Equal("OK", await BleTransport.SendCommandAsync("RUN FM", TestContext.Current.CancellationToken));
            var tokens = new List<string>();
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object? sender, TextEventArgs args)
            {
                tokens.Add(args.Text);
                if (args.Text == "MOVERR")
                    tcs.SetResult(false); // Is Rotating Table turned on?
                else if (args.Text == "ENDSTEP")
                    tcs.TrySetResult(true);
            }

            void TimeoutHandler(object? sender, System.Timers.ElapsedEventArgs e)
            {
                tcs.TrySetResult(false); // Listening timeout
            }

            try
            {
                BleTransport.ListeningTokenReceived += Handler;
                BleTransport.ListeningTimeoutElapsed += TimeoutHandler;
                BleTransport.StartListening(TimeSpan.FromSeconds(3));
                Assert.Equal("OK", await BleTransport.SendCommandAsync("FM 1", TestContext.Current.CancellationToken));
                Assert.True(await tcs.Task);
                await Fixture.StopTableAsync();
                Assert.Contains(tokens, s => s.StartsWith("POS"));
                Assert.Equal("READY", await BleTransport.SendCommandAsync("STATUS", TestContext.Current.CancellationToken));
            }
            finally
            {
                BleTransport.StopListening();
                BleTransport.ListeningTokenReceived -= Handler;
                BleTransport.ListeningTimeoutElapsed -= TimeoutHandler;
                BleTransport.StopListening();
            }
        }
    }
}
