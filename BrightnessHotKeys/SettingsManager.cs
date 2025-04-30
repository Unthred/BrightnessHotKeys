using System.Text.Json;

namespace BrightnessHotKeys;

/// <summary>
/// Manages application settings and persistence
/// </summary>
public class SettingsManager
{
    // Default hotkey settings
    public Keys BrightnessUpKey { get; set; } = Keys.Up;
    public bool BrightnessUpCtrl { get; set; } = true;
    public bool BrightnessUpAlt { get; set; }
    public bool BrightnessUpShift { get; set; }

    public Keys BrightnessDownKey { get; set; } = Keys.Down;
    public bool BrightnessDownCtrl { get; set; } = true;
    public bool BrightnessDownAlt { get; set; }
    public bool BrightnessDownShift { get; set; }

    /// <summary>
    /// Step size for brightness adjustments
    /// </summary>
    public int BrightnessStepSize { get; set; } = 10;

    /// <summary>
    /// Whether to run the application at Windows startup
    /// </summary>
    public bool RunAtStartup { get; set; }

    /// <summary>
    /// Updates this instance with values from another settings instance
    /// </summary>
    public void UpdateFrom(SettingsManager other)
    {
        BrightnessUpKey = other.BrightnessUpKey;
        BrightnessUpCtrl = other.BrightnessUpCtrl;
        BrightnessUpAlt = other.BrightnessUpAlt;
        BrightnessUpShift = other.BrightnessUpShift;
        BrightnessDownKey = other.BrightnessDownKey;
        BrightnessDownCtrl = other.BrightnessDownCtrl;
        BrightnessDownAlt = other.BrightnessDownAlt;
        BrightnessDownShift = other.BrightnessDownShift;
    }


    /// <summary>
    /// Path to the settings file in the application directory
    /// </summary>
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BrightnessHotkeys", "settings.json");

    /// <summary>
    /// Saves current settings to the settings file
    /// </summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            // Log error or display a message if needed
            Console.WriteLine($@"Error saving settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads settings from the settings file or creates default settings if the file doesn't exist
    /// </summary>
    public static SettingsManager Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var settings = JsonSerializer.Deserialize<SettingsManager>(json);
                return settings ?? new SettingsManager();
            }
        }
        catch (Exception ex)
        {
            // Log error or display a message if needed
            Console.WriteLine($@"Error loading settings: {ex.Message}");
        }

        return new SettingsManager();
    }
}
