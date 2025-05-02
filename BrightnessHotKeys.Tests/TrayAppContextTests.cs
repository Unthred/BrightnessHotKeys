using System.Windows.Forms;
using Xunit;

namespace BrightnessHotKeys.Tests
{
    public class TrayAppContextTests
    {
        [Fact]
        public void SetBrightness_ShouldClampBrightnessWithinRange()
        {
            // Arrange
            var context = new TrayAppContext();
            context.GetCurrentBrightness();

            // Act
            context.SetBrightness(150); // Exceeds max
            var maxBrightness = context.GetCurrentBrightness();

            context.SetBrightness(-10); // Below min
            var minBrightness = context.GetCurrentBrightness();

            // Assert
            Assert.Equal(100, maxBrightness);
            Assert.Equal(0, minBrightness);
        }

        [Fact]
        public void RefreshMonitorList_ShouldUpdateMonitorMenu()
        {
            // Arrange
            var context = new TrayAppContext();
            var monitorsMenu = new ToolStripMenuItem();

            // Act
            context.RefreshMonitorList(monitorsMenu);

            // Assert
            Assert.NotEmpty(monitorsMenu.DropDownItems);
        }

        [Fact]
        public void ShowDetailedMonitorInfo_ShouldNotThrowException()
        {
            // Arrange
            var context = new TrayAppContext();

            // Act & Assert
            var exception = Record.Exception(() => context.ShowDetailedMonitorInfo(null, null));
            Assert.Null(exception);
        }
    }
}