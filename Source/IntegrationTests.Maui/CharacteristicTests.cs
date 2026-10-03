using BleCommands.Core;
using BleCommands.Core.Events;
using BleCommands.Maui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace IntegrationTests.Maui
{
    /// <summary>
    /// These tests use real device called Rotating Table:
    /// <see href="https://table-360.ru/">https://table-360.ru/</see>
    /// </summary>
    [TestClass]
    public class CharacteristicTests
    {
        public TestContext TestContext { get; set; }

        #region GetCharacteristicAsync
        [TestMethod]
        public async Task GetCharacteristicAsync_ForExistingUuid_ReturnsCharacteristic()
        {
            // Act
            using var characteristic = await Fixture.Service!.GetCharacteristicAsync(
                Fixture.UpdatesCharacteristicUuid,
                TestContext.CancellationToken);

            // Assert
            Assert.IsNotNull(characteristic);
        }
        #endregion

        #region DetachTokenAggregator
        [TestMethod]
        public async Task DetachTokenAggregator_TokenAggregatorPropertyIsNull()
        {
            var characteristic = await Fixture.Service!.GetCharacteristicAsync(
                Fixture.UpdatesCharacteristicUuid, TestContext.CancellationToken);
            Assert.IsNotNull(characteristic);
            var aggregator = new TokenAggregator();
            characteristic.AttachTokenAggregator(aggregator);
            Assert.AreEqual(aggregator, characteristic.TokenAggregator);

            characteristic.DetachTokenAggregator();
            Assert.IsNull(characteristic.TokenAggregator);
        }
        #endregion

        #region StartReceivingAsync
        [TestMethod]
        public async Task StartReceivingAsync_WhenCharacteristicSupportsNotify_StartsReceiving()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();

            // Act
            await characteristic.StartReceivingAsync(TestContext.CancellationToken);

            // Assert
            Assert.IsTrue(characteristic.IsReceiving);
        }

        [TestMethod]
        public async Task StartReceivingAsync_WhenAlreadyReceiving_ThrowsInvalidOperationException()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();
            await characteristic.StartReceivingAsync(TestContext.CancellationToken);

            // Act
            async Task act() => await characteristic.StartReceivingAsync(
                TestContext.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.AreEqual("Receiving is in progress already.", exception.Message);
        }

        [TestMethod]
        public async Task StartReceivingAsync_WhenCharacteristicDoesNotSupportNotify_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = Fixture.CommandCharacteristic!;

            // Act
            async Task act() => await characteristic.StartReceivingAsync(
                TestContext.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.AreEqual("The characteristic is neither Notify nor Indicate.", exception.Message);
        }
        #endregion

        #region StopReceivingAsync
        [TestMethod]
        public async Task StopReceivingAsync_WhenCharacteristicSupportsNotify_StopsReceiving()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();
            await characteristic.StartReceivingAsync(TestContext.CancellationToken);

            try
            {
                // Act
                await characteristic.StopReceivingAsync(TestContext.CancellationToken);

                // Assert
                Assert.IsFalse(characteristic.IsReceiving);
            }
            finally
            {
                // Restore characteristic to an operational state
                await characteristic.StartReceivingAsync(TestContext.CancellationToken);
            }
        }

        [TestMethod]
        public async Task StopReceivingAsync_WhenNotReceiving_ThrowsInvalidOperationException()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();

            // Act
            async Task act() => await characteristic.StopReceivingAsync(
                TestContext.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.AreEqual("Receiving is not in progress.", exception.Message);
        }
        #endregion

        #region ReadAsync
        [TestMethod]
        public async Task ReadAsync_WhenCharacteristicIsNotReadable_ThrowsInvalidOperationException()
        {
            // Arrange
            using var characteristic = await GetUpdatesCharacteristicAsync();

            // Act
            async Task act() => await characteristic.ReadAsync(TestContext.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.AreEqual("The characteristic is not Read.", exception.Message);
        }

        [TestMethod]
        public async Task ReadAsync_WhenCharacteristicIsReadable_ReturnsDeviceName()
        {
            // Arrange
            var characteristic = await GetDeviceNameCharacteristicAsync();

            // Act
            var result = await characteristic.ReadAsync(TestContext.CancellationToken);

            // Assert
            Assert.AreEqual("Rotating Table", result);
        }
        #endregion

        #region WriteAsync
        [TestMethod]
        public async Task WriteAsync_WhenCharacteristicIsNotWritable_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = Fixture.ResponseCharacteristic;

            // Act
            async Task act() => await characteristic!.WriteAsync(
                "STATUS", TestContext.CancellationToken);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.AreEqual("The characteristic is neither Write nor WriteWithoutResponse.", exception.Message);
        }
        #endregion

        #region ValueReceived
        [TestMethod]
        public async Task ValueReceived_WhenDeviceSendsResponse_RaisesEventWithReceivedBytes()
        {
            // Arrange
            var characteristic = Fixture.ListeningCharacteristic!;
            var eventArgsSource = new TaskCompletionSource<ByteArrayEventArgs>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            void Handler(object? sender, ByteArrayEventArgs args) => eventArgsSource.TrySetResult(args);
            characteristic.ValueReceived += Handler;

            try
            {
                // Act
                var response = await Fixture.BleTransport!.SendCommandAsync(
                    "STATUS", TestContext.CancellationToken);
                var eventArgs = await eventArgsSource.Task;

                // Assert
                Assert.AreEqual("READY", response);
                Assert.AreEqual("READY\n", Characteristic.ConvertToString(eventArgs.Value));
            }
            finally
            {
                characteristic.ValueReceived -= Handler;
            }
        }
        #endregion

        #region Helpers
        private async Task<Characteristic> GetUpdatesCharacteristicAsync()
        {
            var characteristic = await Fixture.Service!.GetCharacteristicAsync(
                Fixture.UpdatesCharacteristicUuid,
                TestContext.CancellationToken);

            return characteristic ?? throw new InvalidOperationException(
                "Characteristic not found in test environment");
        }

        private async Task<Characteristic> GetDeviceNameCharacteristicAsync()
        {
            var service = await Fixture.Device!.GetServiceAsync(
                new Guid("00001800-0000-1000-8000-00805f9b34fb"),
                TestContext.CancellationToken) ??
                throw new InvalidOperationException("Device Information Service not found");

            var characteristic = await service.GetCharacteristicAsync(
                new Guid("00002a00-0000-1000-8000-00805f9b34fb"),
                TestContext.CancellationToken);

            return characteristic ?? throw new InvalidOperationException(
                "Device Name characteristic not found");
        }

        #endregion
    }
}
