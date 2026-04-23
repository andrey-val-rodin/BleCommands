using BleCommands.Windows;

try
{
    WriteLine("This sample uses the device with uploaded LED sketch from BleCommands Examples");

    const string deviceName = "BLECommands LED Example";
    string[] commands = ["ON", "OFF", "TOGGLE", "STATE"];
    string[] validResponses = ["OK", "ON", "OFF"];
    WriteLine("Connecting...");

    using var holder = await BleCommandsClient.CreateTransportAsync(deviceName);
    var transport = holder?.Transport;
    if (transport == null)
    {
        // Failed to create transport
        WriteLine($"Failed to connect with device '{deviceName}'", ConsoleColor.Yellow);
        return;
    }

    await transport.BeginAsync();
    PrintAvailableCommands(commands);

    while (true)
    {
        var command = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(command))
            return;

        var response = await transport.SendCommandAsync(command);
        if (response == null)
        {
            WriteLine("Failed to get response", ConsoleColor.Red);
            return;
        }

        var color = ConsoleColor.Yellow;
        if (!validResponses.Contains(response))
            color = ConsoleColor.Red;
        WriteLine(response, color);
    }
}
catch (Exception ex)
{
    WriteLine("Oops! Something goes wrong...");
    WriteLine(ex.ToString(), ConsoleColor.Red);
}

static void WriteLine(string response, ConsoleColor color = ConsoleColor.White, params object[] arg)
{
    Console.ForegroundColor = color;
    Console.WriteLine(response, arg);
    Console.ForegroundColor = ConsoleColor.White;
}

static void PrintAvailableCommands(IEnumerable<string> commands)
{
    Console.WriteLine("Available commands:");
    foreach (var command in commands)
    {
        WriteLine($"   {command}");
    }
}
