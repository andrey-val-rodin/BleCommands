using BleCommands.Maui;

namespace BleCommands.Tests.Maui
{
    public class BleScannerTests
    {
        [Fact]
        public async Task FindDeviceAsync_DeviceNameIsNull_ArgumentNullException()
        {
            var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string? deviceName = null!;
                await scanner.FindDeviceAsync(deviceName);
            });
            Assert.Equal("deviceName", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsync_DeviceNameIsEmpty_ArgumentNullException()
        {
            var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string deviceName = string.Empty;
                await scanner.FindDeviceAsync(deviceName);
            });
            Assert.Equal("deviceName", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsync_TimeSpanIsZero_ArgumentOutOfRangeException()
        {
            var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await scanner.FindDeviceAsync("Some device", TimeSpan.Zero);
            });
            Assert.Equal("timeout", exception.ParamName);
        }

        [Fact]
        public async Task FindDeviceAsync_TimeSpanIsNegative_ArgumentOutOfRangeException()
        {
            var scanner = new BleScanner();
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await scanner.FindDeviceAsync("Some device", TimeSpan.Zero - TimeSpan.FromSeconds(1));
            });
            Assert.Equal("timeout", exception.ParamName);
        }
    }
}
