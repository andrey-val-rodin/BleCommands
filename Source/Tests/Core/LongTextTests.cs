using BleCommands.Windows;
using System.Text;

namespace BleCommands.Tests.Core
{
    // These tests work with this sketch:
    /*
#include <BLECommands.h>

BLECommandsServer server;

String generateTestString() {
  const size_t SIZE = 50 * 1024;
  char* buffer = (char*)malloc(SIZE + 1);
  
  if (!buffer) return String();
  
  for (size_t i = 0; i < SIZE; i++) {
    buffer[i] = 32 + (i % 95);
  }
  
  buffer[SIZE] = '\0';
  String result = String(buffer);
  free(buffer);
  
  return result;
}
String longText = generateTestString();

void setup() {
  Serial.begin(115200);
  while (!Serial);

  if (!server.begin("BLECommands device")) {
    Serial.println("Starting BLECommands server failed");
    while (true);
  }

  server
    .onCommand("ECHO", [](const String& command, const String& args) -> String {
      String text = args.isEmpty() ? command : command + " " + args;
      Serial.println(text);
      return text;})
    .onCommand("LONG_RESPONSE", [](const String& command, const String& args) -> String {
      return longText;})
    .onCommand("LONG_MESSAGE", [](const String& command, const String& args) -> String {
      server.send(longText);
      return "OK";});
}

void loop() {
  server.poll();
}
    */
    public sealed class Fixture : IAsyncLifetime
    {
        public BleTransport BleTransport { get; set; } = null!;

        public ValueTask DisposeAsync()
        {
            BleTransport?.Dispose();
            return ValueTask.CompletedTask;
        }

        public async ValueTask InitializeAsync()
        {
            var transport = await ArduinoClient.CreateTransportAsync(
                "BLECommands device", TestContext.Current.CancellationToken);
            Assert.NotNull(transport);
            await transport.StartAsync(TestContext.Current.CancellationToken);

            BleTransport = transport;
        }
    }

    public class LongTextTests : IClassFixture<Fixture>
    {
        public BleTransport BleTransport { get; set; }
        public string TestString { get; set; }
        
        public LongTextTests(Fixture fixture)
        {
            BleTransport = fixture.BleTransport;
            Assert.NotNull(BleTransport);
            TestString = GenerateTestString();
        }

        [Fact]
        public async Task SendLongText()
        {
            var stringOf512Chars = "ECHO __10|_______20|_______30|_______40|_______50|_______60|_______70|_______80|_______90|______100|______110|______120|______130|______140|______150|______160|______170|______180|______190|______200|______210|______220|______230|______240|______250|______260|______270|______280|______290|______300|______310|______320|______330|______340|______350|______360|______370|______380|______390|______400|______410|______420|______430|______440|______450|______460|______470|______480|______490|______500|______510|__";
            var response = await BleTransport.SendCommandAsync(stringOf512Chars, TestContext.Current.CancellationToken);

            Assert.Equal(stringOf512Chars, response);
        }

        [Fact]
        public async Task ReceiveLargeResponse()
        {
            var response = await BleTransport.SendCommandAsync("LONG_RESPONSE", TestContext.Current.CancellationToken);

            Assert.Equal(TestString, response);
        }

        [Fact]
        public async Task ReceiveLargeMessage()
        {
            string? failureReason = null;
            var cts = new CancellationTokenSource();

            void ListeningTokenReceived(object? sender, BleCommands.Core.Events.TextEventArgs e)
            {
                if (e.Text != TestString)
                    failureReason = $"TokenReceived: Expected '{TestString}', but got '{e.Text}'";

                cts.Cancel();
            }

            void ListeningTimeoutElapsed(object? sender, System.Timers.ElapsedEventArgs e)
            {
                failureReason = "TimeoutElapsed: The operation timed out.";
                cts.Cancel();
            }

            BleTransport.ListeningTimeoutElapsed += ListeningTimeoutElapsed;
            BleTransport.ListeningTokenReceived += ListeningTokenReceived;
            BleTransport.StartListening(TimeSpan.FromSeconds(3));

            try
            {
                await BleTransport.SendCommandAsync("LONG_MESSAGE", TestContext.Current.CancellationToken);
                await Task.Delay(3000, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                BleTransport.StopListening();
                BleTransport.ListeningTimeoutElapsed -= ListeningTimeoutElapsed;
                BleTransport.ListeningTokenReceived -= ListeningTokenReceived;
            }

            Assert.Null(failureReason);
        }

        private static string GenerateTestString()
        {
            const int SIZE = 50 * 1024;
            StringBuilder sb = new();
            for (var i = 0; i < SIZE; i++)
            {
                char c = (char)(32 + i % 95);
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
