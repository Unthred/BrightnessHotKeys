using Xunit;

namespace BrightnessHotKeys.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void Application_ShouldApplyLastBrightnessOnStartup()
        {
            // Arrange
            var settings = SettingsManager.Load();
            settings.LastBrightness = 75;
            settings.Save();

            // Act
            var context = new TrayAppContext();
            var currentBrightness = context.GetCurrentBrightness();

            // Assert
            Assert.Equal(75, currentBrightness);
        }

        [Fact]
        public void Application_ShouldSaveBrightnessWhenAdjusted()
        {
            // Arrange
            var context = new TrayAppContext();

            // Act
            context.SetBrightness(60);
            var settings = SettingsManager.Load();

            // Assert
            Assert.Equal(60, settings.LastBrightness);
        }
    }
}