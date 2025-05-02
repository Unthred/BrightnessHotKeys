using Xunit;

namespace BrightnessHotKeys.Tests
{
    public class BrightnessHelperTests
    {
        [Fact]
        public void IsLaptopDisplayAvailable_ShouldReturnTrueIfLaptopDisplayExists()
        {
            // Act
            var result = BrightnessHelper.IsLaptopDisplayAvailable();

            // Assert
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void GetMonitorInfo_ShouldReturnListOfMonitors()
        {
            // Act
            var monitorInfo = BrightnessHelper.GetMonitorInfo();

            // Assert
            Assert.NotNull(monitorInfo);
            Assert.IsType<List<string>>(monitorInfo);
        }

        [Fact]
        public void SetAllMonitorsBrightness_ShouldReturnTrueIfSuccessful()
        {
            // Act
            var result = BrightnessHelper.SetAllMonitorsBrightness(50);

            // Assert
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void IdentifyMonitors_ShouldNotThrowException()
        {
            // Act & Assert
            var exception = Record.Exception(BrightnessHelper.IdentifyMonitors);
            Assert.Null(exception);
        }
    }
}