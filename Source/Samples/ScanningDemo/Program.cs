using BleCommands.Core.Exceptions;
using BleCommands.Windows;
using BleCommands.Windows.Events;
using Plugin.BLE.Abstractions;

SemaphoreSlim semaphore = new(1, 1);
var scanner = new BleScanner();
scanner.DeviceDiscovered += Handler;
await scanner.ScanAsync();

async void Handler(object? sender, DeviceDiscoveredEventArgs e)
{
    await semaphore.WaitAsync();
    try
    {
        Console.Write(ToMacString(e.BluetoothAddress));
        Console.WriteLine("\tConnecting...");

        try
        {
            using var device = new Device(e.BluetoothAddress);
            await device.ConnectAsync();

            Console.WriteLine($"\"{device.Name}\"");

            var services = await device.GetServicesAsync();
            foreach (var service in services)
            {
                Console.Write(service.Id);
                Console.Write("\t");
                Console.WriteLine($"\"{KnownServices.Lookup(service.Id).Name}\"");
            }
        }
        catch (DeviceException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
    }
    finally
    {
        semaphore.Release();
    }
}

static string ToMacString(ulong value)
{
    string hex = value.ToString("x12");
    return string.Join(':',
        Enumerable.Range(0, 6)
        .Select(i => hex.Substring(i * 2, 2)));
}
