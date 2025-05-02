using System.Text.Json;
using System.Windows.Forms;
using Xunit;

namespace BrightnessHotKeys.Tests
{
    public class SettingsManagerTests
    {
        [Fact]
        public void Save_ShouldCreateSettingsFile()
        {
            // Arrange
            var settings = new SettingsManager
            {
                BrightnessUpKey = Keys.Up,
                BrightnessDownKey = Keys.Down,
                BrightnessStepSize = 10,
                RunAtStartup = true,
                LastBrightness = 75
            };

            // Act
            settings.Save();

            // Assert
            Assert.True(File.Exists(SettingsManager.FilePath));
            var savedSettings = JsonSerializer.Deserialize<SettingsManager>(File.ReadAllText(SettingsManager.FilePath));
            Assert.NotNull(savedSettings);
            Assert.Equal(75, savedSettings.LastBrightness);
        }

        [Fact]
        public void Load_ShouldReturnDefaultSettingsIfFileDoesNotExist()
        {
            // Arrange
            if (File.Exists(SettingsManager.FilePath))
                File.Delete(SettingsManager.FilePath);

            // Act
            var settings = SettingsManager.Load();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(Keys.Up, settings.BrightnessUpKey);
            Assert.Equal(50, settings.LastBrightness); // Default value
        }

        [Fact]
        public void UpdateFrom_ShouldUpdateAllProperties()
        {
            // Arrange
            var originalSettings = new SettingsManager
            {
                BrightnessUpKey = Keys.Up,
                BrightnessDownKey = Keys.Down,
                BrightnessStepSize = 10,
                RunAtStartup = false,
                LastBrightness = 50
            };

            var newSettings = new SettingsManager
            {
                BrightnessUpKey = Keys.W,
                BrightnessDownKey = Keys.S,
                BrightnessStepSize = 5,
                RunAtStartup = true,
                LastBrightness = 75
            };

            // Act
            originalSettings.UpdateFrom(newSettings);

            // Assert
            Assert.Equal(Keys.W, originalSettings.BrightnessUpKey);
            Assert.Equal(Keys.S, originalSettings.BrightnessDownKey);
            Assert.Equal(5, originalSettings.BrightnessStepSize);
            Assert.True(originalSettings.RunAtStartup);
            Assert.Equal(75, originalSettings.LastBrightness);
        }
    }
}
