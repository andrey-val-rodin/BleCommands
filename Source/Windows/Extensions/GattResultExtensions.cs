// Copyright (c) The dotnet-bluetooth-le Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.


// This file has been copied from dotnet-bluetooth-le
// https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le/blob/main/Source/Plugin.BLE/Windows/Extensions/GattResultExtensions.cs
// Date: 2024-04-14 with these changes applied:
// 1. These comments are added
// 2. namespace Plugin.BLE.Extensions -> Windows.Extensions
// 3. Nullable types
// 4. Minor changes in GetErrorMessage()
// 5. new byte[0] => Array.Empty<byte>()

using BleCommands.Core.Exceptions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCommands.Windows.Extensions
{
    internal static class GattResultExtensions
    {
        public static void ThrowIfError(this GattWriteResult result, [CallerMemberName] string? tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static void ThrowIfError(this GattCharacteristicsResult result, [CallerMemberName] string? tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static void ThrowIfError(this GattDescriptorsResult result, [CallerMemberName] string? tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static void ThrowIfError(this GattDeviceServicesResult result, [CallerMemberName] string? tag = null)
            => result.Status.ThrowIfError(tag, result.ProtocolError);

        public static byte[] GetValueOrThrowIfError(this GattReadResult result, [CallerMemberName] string? tag = null)
        {
            var errorMessage = result.Status.GetErrorMessage(tag, result.ProtocolError);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new CharacteristicException(errorMessage);
            }

            return result.Value?.ToArray() ?? Array.Empty<byte>();
        }

        public static void ThrowIfError(this GattCommunicationStatus status, [CallerMemberName] string? tag = null, byte? protocolError = null)
        {
            var errorMessage = status.GetErrorMessage(tag, protocolError);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception(errorMessage);
            }
        }

        private static string? GetErrorMessage(this GattCommunicationStatus status, string? tag, byte? protocolError)
        {
#pragma warning disable IDE0066
            switch (status)
            {
                //output trace message with status of update
                case GattCommunicationStatus.Success:
                    return null;
                case GattCommunicationStatus.ProtocolError when protocolError != null:
                    return $"[{tag}] failed with status: {status} and protocol error {protocolError.GetErrorString()}";
                case GattCommunicationStatus.AccessDenied:
                case GattCommunicationStatus.ProtocolError:
                case GattCommunicationStatus.Unreachable:
                    return $"[{tag}] failed with status: {status}";
                default:
                    return null;
            }
#pragma warning restore IDE0066
        }
    }
}