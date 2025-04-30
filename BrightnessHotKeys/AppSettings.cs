using System.Text.Json;

namespace BrightnessHotKeys;

public class AppSettings
{
    public Keys BrightnessUpKey { get; set; } = Keys.Up;
    public Keys BrightnessDownKey { get; set; } = Keys.Down;
    public int LastBrightness { get; set; } = 50;

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BrightnessHotkeys", "settings.json"
    );

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            ToastMessageUtility.ShowToast("Error Loading Settings", $"An error occurred: {ex.Message}", ToolTipIcon.Error);
        }

        return new AppSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}