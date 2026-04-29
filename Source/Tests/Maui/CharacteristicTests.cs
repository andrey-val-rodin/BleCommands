using BleCommands.Core;
using BleCommands.Core.Enums;
using BleCommands.Maui;

namespace BleCommands.Tests.Maui
{
    public class CharacteristicTests
    {
        [Fact]
        public void Constructor_NullCharacteristic_ArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Characteristic(null!);
            });
            Assert.Equal("characteristic", exception.ParamName);
        }

        [Fact]
        public async Task ReadAsync_CannotRead_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await characteristic.ReadAsync(TestContext.Current.CancellationToken);
            });

            Assert.Equal("The characteristic is not Read.", exception.Message);
        }

        [Fact]
        public async Task WriteAsync_CannotWrite_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read | CharacteristicPropertyFlags.Notify);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await characteristic.WriteAsync("test", TestContext.Current.CancellationToken);
            });

            Assert.Equal("The characteristic is neither Write nor WriteWithoutResponse.", exception.Message);
        }

        [Fact]
        public async Task WriteAsync_TextIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Write);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await characteristic.WriteAsync(null!, TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public void AttachTokenAggregator_CannotUpdate_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read | CharacteristicPropertyFlags.Write);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                characteristic.AttachTokenAggregator(new TokenAggregator());
            });

            Assert.Equal("The characteristic is neither Notify nor Indicate.", exception.Message);
        }

        [Fact]
        public void AttachTokenAggregator_TokenAggregatorIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                characteristic.AttachTokenAggregator(null!);
            });
        }

        [Fact]
        public void AttachTokenAggregator_WhenAlreadyAttached_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);
            var aggregator1 = new TokenAggregator();
            var aggregator2 = new TokenAggregator();

            characteristic.AttachTokenAggregator(aggregator1);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                characteristic.AttachTokenAggregator(aggregator2);
            });

            Assert.Equal("TokenAggregator is already attached. Call DetachTokenAggregator first.", exception.Message);
        }

        [Fact]
        public void DetachTokenAggregator_WhenAttached_SuccessfullyDetaches()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);
            var aggregator = new TokenAggregator();
            characteristic.AttachTokenAggregator(aggregator);
            Assert.NotNull(characteristic.TokenAggregator);

            // Act
            characteristic.DetachTokenAggregator();

            // Assert
            Assert.Null(characteristic.TokenAggregator);
        }

        [Fact]
        public void DetachTokenAggregator_WhenNotAttached_DoesNothing()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);
            Assert.Null(characteristic.TokenAggregator);

            // Act & Assert (No exceptions)
            characteristic.DetachTokenAggregator();
            Assert.Null(characteristic.TokenAggregator);
        }

        [Fact]
        public async Task StartReceivingAsync_CannotUpdate_ThrowsInvalidOperationException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await characteristic.StartReceivingAsync(TestContext.Current.CancellationToken);
            });

            Assert.Equal("The characteristic is neither Notify nor Indicate.", exception.Message);
        }

        [Fact]
        public async Task StartReceivingAsync_WhenNativeCharacteristicIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);

            // Act & Assert - при попытке работать с null NativeCharacteristic
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await characteristic.StartReceivingAsync(TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public void Properties_WithTestConstructor_ReturnsCorrectFlags()
        {
            // Arrange
            var expectedFlags = CharacteristicPropertyFlags.Read | CharacteristicPropertyFlags.Write | CharacteristicPropertyFlags.Notify;
            var characteristic = new Characteristic(expectedFlags);

            // Act & Assert
            Assert.Equal(expectedFlags, characteristic.Properties);
        }

        [Fact]
        public void CanRead_WhenPropertiesIncludeRead_ReturnsTrue()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);
            Assert.True(characteristic.CanRead);
        }

        [Fact]
        public void CanRead_WhenPropertiesDoNotIncludeRead_ReturnsFalse()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Write);
            Assert.False(characteristic.CanRead);
        }

        [Fact]
        public void CanWrite_WhenPropertiesIncludeWrite_ReturnsTrue()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Write);
            Assert.True(characteristic.CanWrite);
        }

        [Fact]
        public void CanWrite_WhenPropertiesIncludeWriteWithoutResponse_ReturnsTrue()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.WriteWithoutResponse);
            Assert.True(characteristic.CanWrite);
        }

        [Fact]
        public void CanWrite_WhenPropertiesDoNotIncludeWrite_ReturnsFalse()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);
            Assert.False(characteristic.CanWrite);
        }

        [Fact]
        public void CanUpdate_WhenPropertiesIncludeNotify_ReturnsTrue()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);
            Assert.True(characteristic.CanUpdate);
        }

        [Fact]
        public void CanUpdate_WhenPropertiesIncludeIndicate_ReturnsTrue()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Indicate);
            Assert.True(characteristic.CanUpdate);
        }

        [Fact]
        public void CanUpdate_WhenPropertiesDoNotIncludeNotifyOrIndicate_ReturnsFalse()
        {
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);
            Assert.False(characteristic.CanUpdate);
        }

        [Fact]
        public async Task ReadAsync_WhenDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);
            characteristic.Dispose();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await characteristic.ReadAsync(TestContext.Current.CancellationToken);
            });
        }

        [Fact]
        public async Task WriteAsync_WhenDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Write);
            characteristic.Dispose();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await characteristic.WriteAsync("test", TestContext.Current.CancellationToken);
            });

            Assert.Equal(typeof(Characteristic).FullName, exception.ObjectName);
        }

        [Fact]
        public void AttachTokenAggregator_WhenDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);
            characteristic.Dispose();

            // Act & Assert
            var exception = Assert.Throws<ObjectDisposedException>(() =>
            {
                characteristic.AttachTokenAggregator(new TokenAggregator());
            });

            Assert.Equal(typeof(Characteristic).FullName, exception.ObjectName);
        }

        [Fact]
        public async Task StartReceivingAsync_WhenDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);
            characteristic.Dispose();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await characteristic.StartReceivingAsync(TestContext.Current.CancellationToken);
            });

            Assert.Equal(typeof(Characteristic).FullName, exception.ObjectName);
        }

        [Fact]
        public void Dispose_WhenCalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);

            // Act
            characteristic.Dispose();

            // Assert (repeated Dispose must not throw an exception)
            var exception = Record.Exception(characteristic.Dispose);
            Assert.Null(exception);
        }

        [Fact]
        public void Properties_AfterDispose_StillAccessible()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read | CharacteristicPropertyFlags.Write);
            characteristic.Dispose();

            // Act & Assert - properties must be accessible even after Dispose
            Assert.Equal(CharacteristicPropertyFlags.Read | CharacteristicPropertyFlags.Write, characteristic.Properties);
            Assert.True(characteristic.CanRead);
            Assert.True(characteristic.CanWrite);
        }

        [Fact]
        public void Id_AfterDispose_StillAccessible()
        {
            // Arrange
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Read);
            characteristic.Dispose();

            // Act & Assert - Id must be accessible even after Dispose
            var exception = Record.Exception(() => _ = characteristic.Id);
            Assert.Null(exception);
        }

        [Fact]
        public void TokenAggregator_Initially_IsNull()
        {
            // Arrange & Act
            var characteristic = new Characteristic(CharacteristicPropertyFlags.Notify);

            // Assert
            Assert.Null(characteristic.TokenAggregator);
        }
    }
}
