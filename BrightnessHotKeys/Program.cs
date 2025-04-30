namespace BrightnessHotKeys;

/// <summary>
/// Application entry point
/// </summary>
internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Check if an instance is already running
        using var mutex = new Mutex(true, "BrightnessHotKeysAppMutex", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(@"An instance of Brightness Hotkeys is already running.",
                @"Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Set application icon
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "BrightnessHotKeys.ico");
            if (File.Exists(iconPath))
            {
                AppDomain.CurrentDomain.SetData("APP_ICON", new Icon(iconPath));
            }
        }
        catch (Exception)
        {
            // Continue without custom icon if it can't be loaded
        }

        // Run the application with our tray app context
        try
        {
            Application.Run(new TrayAppContext());
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"An error occurred: {ex.Message}", @"Brightness Control Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

//These improvements provide:
//1.	Visual brightness indicator
//2.	Multiple preset brightness levels in the context menu
//3.	Option to run at Windows startup
//4.	Current brightness detection
//5.	Configurable brightness step size
//6.	Single-instance application
//7.	Better error handling for hotkey registration
//8.	Proper resource cleanup with IDisposable
//9.	More user-friendly context menu
//10.	Automatic brightness level display