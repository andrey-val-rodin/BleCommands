using BleCommands.Windows;
using System.Text;

namespace BleCommands.Tests.Core
{
    // These tests work with the following sketch:
    /*
#include <BLECommands.h>

BLECommandsServer server;

String generateTestString() {
  const int SIZE = 50 * 1024;
  char* buffer = (char*)malloc(SIZE + 1);

  if (!buffer) return String();

  int pos = 0;
  int i = 0;
  int cnt = 0;

  const unsigned char cyrillicBase = 0x90;

  const char* chineseChars[] = {
    "\xE4\xB8\xAD", // 中
    "\xE6\x96\x87", // 文
    "\xE5\x9B\xBD", // 国
    "\xE4\xBA\xBA", // 人
    "\xE6\xB0\x91", // 民
    "\xE5\xA4\xA7", // 大
    "\xE5\xAD\xA6", // 学
    "\xE7\x94\x9F"  // 生
  };
  const size_t CHINESE_COUNT = 8;

  while (pos < SIZE) {
    int remainder = i % 4;
    int count = cnt % 3;
    cnt++;
    switch (remainder) {
      case 0: { // ASCII - 1 byte
        for (int j = 0; j <= count; j++)
        {
          if (pos + 1 <= SIZE) {
            buffer[pos++] = static_cast<char>(32 + (i++ % 95));
          }
        }
        break;
      }
      case 1: { // Russian - 2 bytes
        for (int j = 0; j <= count; j++)
        {
          if (pos + 2 <= SIZE) {
            unsigned char cyrillic = cyrillicBase + (i++ % 32);
            buffer[pos++] = static_cast<char>(0xD0);
            buffer[pos++] = static_cast<char>(cyrillic);
          }
        }
        break;
      }

      case 2: { // Chinese character - 3 bytes
        for (int j = 0; j <= count; j++)
        {
          if (pos + 3 <= SIZE) {
            const char* ch = chineseChars[i++ % CHINESE_COUNT];
            buffer[pos++] = ch[0];
            buffer[pos++] = ch[1];
            buffer[pos++] = ch[2];
          }
        }
        break;
      }

      case 3: { // Emoji - 4 bytes
        for (int j = 0; j <= count; j++)
        {
          if (pos + 4 <= SIZE) {
            const char* fire = "\xF0\x9F\x94\xA5";
            buffer[pos++] = fire[0];
            buffer[pos++] = fire[1];
            buffer[pos++] = fire[2];
            buffer[pos++] = fire[3];
            i++;
          }
        }
        break;
      }
    }
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
            BleTransport.StartListening(TimeSpan.FromSeconds(5));

            try
            {
                await BleTransport.SendCommandAsync("LONG_MESSAGE", TestContext.Current.CancellationToken);
                await Task.Delay(5000, cts.Token);
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
            int pos = 0;
            int i = 0;
            int cnt = 0;

            char[] cyrillicChars = new char[32]; // А-Я
            for (int j = 0; j < 32; j++)
                cyrillicChars[j] = (char)(0x0410 + j);

            char[] chineseChars = ['中', '文', '国', '人', '民', '大', '学', '生'];

            while (pos < SIZE)
            {
                int remainder = i % 4;
                int count = cnt % 3;
                cnt++;
                switch (remainder)
                {
                    case 0: // ASCII - 1 byte
                        for (int j = 0; j <= count; j++)
                        {
                            if (pos + 1 <= SIZE)
                            {
                                char c = (char)(32 + (i++ % 95));
                                sb.Append(c);
                                pos += 1;
                            }
                        }
                        break;

                    case 1: // Russian - 2 bytes
                        for (int j = 0; j <= count; j++)
                        {
                            if (pos + 2 <= SIZE)
                            {
                                char c = cyrillicChars[i++ % cyrillicChars.Length];
                                sb.Append(c);
                                pos += 2;
                            }
                        }
                        break;

                    case 2: // Chinese character - 3 bytes
                        for (int j = 0; j <= count; j++)
                        {
                            if (pos + 3 <= SIZE)
                            {
                                char c = chineseChars[i++ % chineseChars.Length];
                                sb.Append(c);
                                pos += 3;
                            }
                        }
                        break;

                    case 3: // Emoji - 4 bytes
                        for (int j = 0; j <= count; j++)
                        {
                            if (pos + 4 <= SIZE)
                            {
                                sb.Append("🔥");
                                pos += 4;
                                i++;
                            }
                        }
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
