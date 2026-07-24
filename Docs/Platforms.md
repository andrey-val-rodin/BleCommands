# Platforms

There are two libraries: `BleCommands.Maui` and `BleCommands.Windows`.

## Windows

Uses classes from `Windows.Devices.Bluetooth` namespace. Native objects are:

- `BluetoothLEDevice`
- `GattDeviceService`
- `GattCharacteristic`

## MAUI

Uses popular NuGet package `Plugin.BLE`. Native objects are:

- `Plugin.BLE.Abstractions.Contracts.IDevice`
- `Plugin.BLE.Abstractions.Contracts.IService`
- `Plugin.BLE.Abstractions.Contracts.ICharacteristic`

## Core

Abstraction part that contains interfaces and base implementations.
