# BleCommands

[![.NET Build & Test](https://github.com/andrey-val-rodin/BleCommands/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/andrey-val-rodin/BleCommands/actions/workflows/dotnet-build.yml)

**BleCommands** is a simple and reliable library for exchanging text commands between devices via Bluetooth Low Energy (BLE). It provides a unified interface for .NET applications (MAUI, WPF, WinForms) and, together with the [companion library for Arduino](https://github.com/andrey-val-rodin/BleCommands.Arduino), makes it easy to control your BLE devices.

## Features

- **Text-based protocol**: Commands are human-readable, simplifying debugging.
- **Ease of use**: No need to understand the intricacies of GATT, characteristics, descriptors or buffering.
- **Cross-platform**: A single API for **MAUI** (Android/iOS), **WPF**, and **WinForms**.
- **Reliability**: Automatic reassembly of fragmented packets, response waiting and timeouts.
- **Ready-to-use companion**: The dedicated [library for Arduino/ESP32](https://github.com/andrey-val-rodin/BleCommands.Arduino) implements the server side of the protocol out of the box.
- **Advanced scenarios**: Convenient work with a BLE device with access to native objects.

## Installation

Add one of the following NuGet packages to your project:

```powershell
# For MAUI applications (Android, iOS)
Install-Package BleCommands.Maui

# For Windows applications (WPF, WinForms, Console)
Install-Package BleCommands.Windows
```

## Quick Start
If your Arduino sketch uses the [Arduino companion library](https://github.com/andrey-val-rodin/BleCommands.Arduino), then the work will be quite simple:
```c#
using var transport = await ArduinoClient.CreateTransportAsync("My device");
await transport.StartAsync();
var response = await transport.SendCommandAsync("Status");
```
You can subscribe to regular messages from the device, as well as to timeout and connection loss events:
```c#
transport.ListeningTimeoutElapsed += (s, e) => { Console.WriteLine("Timeout"); };
transport.ListeningTokenReceived += (s, e) => { Console.WriteLine($"Token: {e.Text}"); };
transport.Disconnected += (s, e) => { Console.WriteLine("Device disconnected"); };
transport.StartListening(TimeSpan.FromSeconds(1));
```
You can work directly with Device, Service, and Characteristic objects. To release all system resources after work, simply dispose the Device object:
```c#
using var device = await new BleScanner().FindDeviceAsync("My device");
var services = await device.GetServicesAsync();
var characteristics = await services.FirstOrDefault()?.GetCharacteristicsAsync();
```
