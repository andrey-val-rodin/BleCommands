using BleCommands.Windows;

namespace BleCommands.Tests.Maui
{
    public class BleScannerTests
    {
        [Fact]
        public async Task FindDeviceAsync_DeviceNameIsNull_ArgumentNullException()
        {
            using var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string? deviceName = null!;
                await FindDeviceWithTimeoutAsync(scanner, deviceName);
            });
            Assert.Equal("deviceName", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsync_DeviceNameIsEmpty_ArgumentNullException()
        {
            using var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string deviceName = string.Empty;
                await FindDeviceWithTimeoutAsync(scanner, deviceName);
            });
            Assert.Equal("deviceName", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsyncWithCancellationToken_DeviceNameIsNull_ArgumentNullException()
        {
            using var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string? deviceName = null!;
                await scanner.FindDeviceAsync(deviceName, TestContext.Current.CancellationToken);
            });
            Assert.Equal("deviceName", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsyncWithCancellationToken_DeviceNameIsEmpty_ArgumentNullException()
        {
            using var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string deviceName = string.Empty;
                await scanner.FindDeviceAsync(deviceName, TestContext.Current.CancellationToken);
            });
            Assert.Equal("deviceName", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsync_TimeSpanIsZero_ArgumentOutOfRangeException()
        {
            using var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await scanner.FindDeviceAsync("Some device", TimeSpan.Zero);
            });
            Assert.Equal("timeout", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsync_TimeSpanIsNegative_ArgumentOutOfRangeException()
        {
            using var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await scanner.FindDeviceAsync("Some device", TimeSpan.Zero - TimeSpan.FromSeconds(1));
            });
            Assert.Equal("timeout", exception.ParamName);
        }

        // Just to avoid xUnit1051 "Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken"
        private static async Task FindDeviceWithTimeoutAsync(BleScanner scanner, string deviceName)
        {
            await scanner.FindDeviceAsync(deviceName);
        }
    }
}
