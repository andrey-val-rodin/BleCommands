using BleCommands.Windows;

namespace BleCommands.Tests.Windows
{
    public class DeviceTests
    {
        [Fact]
        public async Task ConnectAsync_Disposed_ObjectDisposedException()
        {
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                var device = new Device(0);
                device.Dispose();

                await device.ConnectAsync(TestContext.Current.CancellationToken);
            });
            Assert.Equal(typeof(Device).FullName, exception.ObjectName);
        }

        [Fact]
        public async Task ConnectAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var device = new DeviceStub();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await device.ConnectAsync(cts.Token);
            });
        }

        [Fact]
        public async Task GetServicesAsync_Disposed_ObjectDisposedException()
        {
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                var device = new Device(0);
                device.Dispose();

                await device.GetServicesAsync(TestContext.Current.CancellationToken);
            });
            Assert.Equal(typeof(Device).FullName, exception.ObjectName);
        }

        [Fact]
        public async Task GetServicesAsync_NotConnected_InvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var device = new Device(0);

                await device.GetServicesAsync(TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public async Task GetServicesAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var device = new DeviceStub();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await device.GetServicesAsync(cts.Token);
            });
        }

        [Fact]
        public async Task GetServiceAsync_Disposed_ObjectDisposedException()
        {
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                var device = new Device(0);
                device.Dispose();

                await device.GetServiceAsync(Guid.Empty, TestContext.Current.CancellationToken);
            });
            Assert.Equal(typeof(Device).FullName, exception.ObjectName);
        }

        [Fact]
        public async Task GetServiceAsync_NotConnected_InvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var device = new Device(0);

                await device.GetServiceAsync(Guid.Empty, TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public async Task GetServiceAsync_ExternalCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var device = new DeviceStub();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await device.GetServiceAsync(Guid.NewGuid(), cts.Token);
            });
        }
    }
}
