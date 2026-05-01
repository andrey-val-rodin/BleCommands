using BleCommands.Core.Events;
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntegrationTests.Uwp
{
    [TestClass]
    public class TransportTests
    {
        public TestContext TestContext { get; set; }

        private BleTransport BleTransport => Fixture.BleTransport;

        [TestMethod]
        public async Task SendCommandAsync_Status_ValidResponse()
        {
            Assert.AreEqual("READY", await BleTransport.SendCommandAsync("STATUS", TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task SendCommandAsync_InsufficientTimeout_Null()
        {
            var oldTimeout = BleTransport.ResponseTimeout;
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object sender, TextEventArgs args)
            {
                tcs.SetResult(true);
            }

            try
            {
                BleTransport.ResponseTimeout = TimeSpan.FromMilliseconds(1);
                BleTransport.ListeningTokenReceived += Handler;
                Assert.IsNull(await BleTransport.SendCommandAsync("STATUS", TestContext.CancellationToken));

                // Wait until we receive an actual response
                await tcs.Task;
            }
            finally
            {
                BleTransport.ListeningTokenReceived -= Handler;
                BleTransport.ResponseTimeout = oldTimeout;
            }
        }

        [TestMethod]
        public async Task ListeningIsInProgress_RotateTable_ReceiveTokens()
        {
            Assert.AreEqual("OK", await BleTransport.SendCommandAsync("RUN FM", TestContext.CancellationToken));
            var tokens = new List<string>();
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object sender, TextEventArgs args)
            {
                tokens.Add(args.Text);
                if (args.Text == "MOVERR")
                    tcs.SetResult(false); // Is Rotating Table turned on?
                else if (args.Text == "ENDSTEP")
                    tcs.TrySetResult(true);
            }

            void TimeoutHandler(object sender, System.Timers.ElapsedEventArgs e)
            {
                tcs.TrySetResult(false); // Listening timeout
            }

            try
            {
                BleTransport.ListeningTokenReceived += Handler;
                BleTransport.ListeningTimeoutElapsed += TimeoutHandler;
                BleTransport.StartListening(TimeSpan.FromSeconds(3));
                Assert.AreEqual("OK", await BleTransport.SendCommandAsync("FM 1", TestContext.CancellationToken));
                Assert.IsTrue(await tcs.Task);
                await Fixture.StopTableAsync();
                Assert.Contains(s => s.StartsWith("POS"), tokens);
                Assert.AreEqual("READY", await BleTransport.SendCommandAsync("STATUS", TestContext.CancellationToken));
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
