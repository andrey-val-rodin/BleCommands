using Windows;

namespace Tests.Windows
{
    public class DeviceFinderTests
    {
        [Fact]
        public async Task FindDeviceAsync_Found()
        {
            var finder = new DeviceFinder();
            var result = await finder.FindDeviceAsync("Rotating", TimeSpan.FromSeconds(5));
        }
    }
}
