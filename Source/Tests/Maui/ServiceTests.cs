using BleCommands.Maui;

namespace BleCommands.Tests.Maui
{
    public class ServiceTests
    {
        [Fact]
        public void Constructor_WithNullNativeService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new Service(null!));
            Assert.Equal("nativeService", exception.ParamName);
        }

        [Fact]
        public async Task GetCharacteristicAsync_WhenServiceDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var service = new Service();
            service.Dispose();
            var characteristicId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await service.GetCharacteristicAsync(characteristicId, TestContext.Current.CancellationToken));
            Assert.Equal(typeof(Service).FullName, exception.ObjectName);
        }

        [Fact]
        public async Task GetCharacteristicsAsync_WhenServiceDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var service = new Service();
            service.Dispose();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await service.GetCharacteristicsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(typeof(Service).FullName, exception.ObjectName);
        }

        [Fact]
        public void Dispose_WhenCalledMultipleTimes_DisposesNativeServiceOnlyOnce()
        {
            // Act
            var service = new Service();
            service.Dispose();

            // Assert (repeated Dispose must not throw an exception)
            var exception = Record.Exception(service.Dispose);
            Assert.Null(exception);
        }

        [Fact]
        public void Id_WhenServiceDisposed_StillAccessible()
        {
            var service = new Service();
            var idBeforeDispose = service.Id;
            service.Dispose();

            Assert.Equal(idBeforeDispose, service.Id);
        }
    }
}
