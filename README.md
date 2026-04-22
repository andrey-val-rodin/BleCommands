# BleCommands

**BleCommands** is a simple and reliable library for exchanging text commands between devices via Bluetooth Low Energy (BLE). It provides a unified interface for .NET applications (MAUI, WPF, WinForms) and, together with the [companion library for Arduino](https://github.com/andrey-val-rodin/BleCommands.Arduino), makes it easy to control your BLE devices.

## Features

- **Text-based protocol**: Commands are human-readable, simplifying debugging.
- **Ease of use**: No need to understand the intricacies of GATT, characteristics, descriptors or buffering.
- **Cross-platform**: A single API for **MAUI** (Android/iOS), **WPF**, and **WinForms**.
- **Reliability**: Automatic reassembly of fragmented packets, response waiting and timeouts.
- **Ready-to-use companion**: The dedicated [library for Arduino/ESP32](https://github.com/andrey-val-rodin/BleCommands.Arduino) implements the server side of the protocol out of the box.

## Quick Start

## Installation

Add one of the following NuGet packages to your project:

```bash
# For MAUI applications (Android, iOS)
dotnet add package BleCommands.Maui

# For Windows applications (WPF, WinForms)
dotnet add package BleCommands.Windows
```