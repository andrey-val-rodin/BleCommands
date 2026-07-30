using BleCommands.Windows;

namespace BleCommands.IntegrationTests.Windows
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [Collection("IntegrationTests.Windows")]
    public class CharacteristicTests(Fixture fixture)
    {
        private Fixture Fixture { get; } = fixture;

        #region GetCharacteristicAsync
        [Fact]
        public async Task GetCharacteristicAsync_ForExistingUuid_ReturnsCharacteristic()
        {
            // Act
            using var characteristic = await Fixture.Service.GetCharacteristicAsync(
                Fixture.UpdatesCharacteristicUuid,
                TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(characteristic);
        }
        #endregion

        #region StartReceivingAsync
        [Fact]
        public async Task StartReceivingAsync_WhenCharacteristicSupportsNotify_StartsReceiving()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();

            // Act
            await characteristic.StartReceivingAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(characteristic.IsReceiving);
        }

        [Fact]
        public async Task StartReceivingAsync_WhenAlreadyReceiving_ThrowsInvalidOperationException()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();
            await characteristic.StartReceivingAsync(TestContext.Current.CancellationToken);

            // Act
            async Task act() => await characteristic.StartReceivingAsync(
                TestContext.Current.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("Receiving is in progress already.", exception.Message);
        }

        [Fact]
        public async Task StartReceivingAsync_WhenCharacteristicDoesNotSupportNotify_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = Fixture.CommandCharacteristic;

            // Act
            async Task act() => await characteristic.StartReceivingAsync(
                TestContext.Current.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("The characteristic is neither Notify nor Indicate.", exception.Message);
        }
        #endregion

        #region StopReceivingAsync
        [Fact]
        public async Task StopReceivingAsync_WhenCharacteristicSupportsNotify_StopsReceiving()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();
            await characteristic.StartReceivingAsync(TestContext.Current.CancellationToken);

            // Act
            await characteristic.StopReceivingAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.False(characteristic.IsReceiving);
        }

        [Fact]
        public async Task StopReceivingAsync_WhenNotReceiving_ThrowsInvalidOperationException()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();

            // Act
            async Task act() => await characteristic.StopReceivingAsync(
                TestContext.Current.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("Receiving is not in progress.", exception.Message);
        }
        #endregion

        #region ReadAsync
        [Fact]
        public async Task ReadAsync_WhenCharacteristicIsNotReadable_ThrowsInvalidOperationException()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();

            // Act
            async Task act() => await characteristic.ReadAsync(TestContext.Current.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("The characteristic is not Read.", exception.Message);
        }

        [Fact]
        public async Task ReadAsync_WhenCharacteristicIsReadable_ReturnsDeviceName()
        {
            // Arrange
            var characteristic = await GetDeviceNameCharacteristicAsync();

            // Act
            var result = await characteristic.ReadAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("Rotating Table", result);
        }
        #endregion

        #region WriteAsync
        [Fact]
        public async Task WriteAsync_WhenCharacteristicIsNotWritable_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = Fixture.ResponseCharacteristic;

            // Act
            async Task act() => await characteristic.WriteAsync(
                "STATUS", TestContext.Current.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("The characteristic is neither Write nor WriteWithoutResponse.", exception.Message);
        }
        #endregion

        #region Helpers
        private async Task<Characteristic> GetUpdatesCharacteristicAsync()
        {
            var characteristic = await Fixture.Service.GetCharacteristicAsync(
                Fixture.UpdatesCharacteristicUuid,
                TestContext.Current.CancellationToken);

            return characteristic ?? throw new InvalidOperationException(
                "Characteristic not found in test environment");
        }

        private async Task<Characteristic> GetDeviceNameCharacteristicAsync()
        {
            var service = await Fixture.Device.GetServiceAsync(
                new Guid("00001800-0000-1000-8000-00805f9b34fb"),
                TestContext.Current.CancellationToken) ??
                throw new InvalidOperationException("Device Information Service not found");

            var characteristic = await service.GetCharacteristicAsync(
                new Guid("00002a00-0000-1000-8000-00805f9b34fb"),
                TestContext.Current.CancellationToken);

            return characteristic ?? throw new InvalidOperationException(
                "Device Name characteristic not found");
        }

        #endregion
    }
}
