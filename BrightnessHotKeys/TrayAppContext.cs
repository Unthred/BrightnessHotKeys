using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace BrightnessHotKeys;

/// <summary>
/// Main application context that manages the tray icon, hotkeys, and brightness control
/// </summary>
public class TrayAppContext : ApplicationContext
{
    // Private fields
    private readonly NotifyIcon trayIcon;
    private readonly SettingsManager settings;
    private readonly HiddenForm hiddenForm;
    private readonly BrightnessIndicator brightnessIndicator;
    private int currentBrightness;
    private bool disposed;

    // Constants for hotkey registration
    private const int HotkeyUpId = 1;
    private const int HotkeyDownId = 2;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;

    // Native methods for hotkey registration
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>
    /// Initializes the application context and sets up the tray icon
    /// </summary>
    public TrayAppContext()
    {
        // Load settings
        settings = SettingsManager.Load();

        // Initialize brightness indicator form
        brightnessIndicator = new BrightnessIndicator();

        // Try to get current brightness level, or default to 50
        currentBrightness = BrightnessHelper.GetCurrentBrightness() ?? 50;
        BrightnessHelper.SetAllMonitorsBrightness(currentBrightness);

        // Initialize hidden form for hotkey handling
        hiddenForm = new HiddenForm();
        hiddenForm.HotKeyPressed += OnHotKeyPressed;

        // Get the application icon
        var appIcon = GetApplicationIcon();

        // Set up the tray icon with an enhanced context menu
        trayIcon = new NotifyIcon
        {
            Icon = appIcon,
            ContextMenuStrip = CreateContextMenu(),
            Visible = true,
            Text = @"Brightness Control"
        };

        // Apply startup settings
        if (settings.RunAtStartup)
        {
            SetStartupWithWindows(true);
        }

        // Register hotkeys
        RegisterHotKeys();
    }

    /// <summary>
    /// Gets the current brightness level.
    /// </summary>
    public int GetCurrentBrightness()
    {
        return currentBrightness;
    }

    /// <summary>
    /// Creates the context menu for the tray icon with all options
    /// </summary>
    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        // Current brightness display (non-clickable)
        menu.Items.Add($"Current Brightness: {currentBrightness}%", null, null).Enabled = false;
        menu.Items.Add(new ToolStripSeparator());

        // Preset brightness levels
        menu.Items.Add("Set Brightness to 25%", null, (_, _) => SetBrightness(25));
        menu.Items.Add("Set Brightness to 50%", null, (_, _) => SetBrightness(50));
        menu.Items.Add("Set Brightness to 75%", null, (_, _) => SetBrightness(75));
        menu.Items.Add("Set Brightness to 100%", null, (_, _) => SetBrightness(100));
        menu.Items.Add(new ToolStripSeparator());
    
        // Add monitor selection submenu
        var monitorsMenu = new ToolStripMenuItem("Monitor Selection");
        RefreshMonitorList(monitorsMenu);
        menu.Items.Add(monitorsMenu);
    
        // Add refresh monitors option
        menu.Items.Add("Refresh Monitor List", null, (_, _) => RefreshMonitorList((ToolStripMenuItem)menu.Items[7]));
        menu.Items.Add("Identify Monitors", null, (_, _) => BrightnessHelper.IdentifyMonitors());
        menu.Items.Add(new ToolStripSeparator());

        // Configuration options
        menu.Items.Add("Set Hotkeys", null, OpenHotkeyDialog);

        // Run at startup option (checkbox)
        var startupItem = new ToolStripMenuItem("Run at Windows Startup")
        {
            Checked = settings.RunAtStartup,
            CheckOnClick = true
        };
        startupItem.Click += (_, _) =>
        {
            settings.RunAtStartup = startupItem.Checked;
            SetStartupWithWindows(settings.RunAtStartup);
            settings.Save();
        };
        menu.Items.Add(startupItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, Exit);

        return menu;
    }

    /// <summary>
    /// Refreshes the monitor list in the context menu
    /// </summary>
    public void RefreshMonitorList(ToolStripMenuItem monitorsMenu)
    {
        // Clear existing items
        monitorsMenu.DropDownItems.Clear();
    
        // Get monitor information
        var monitorInfo = BrightnessHelper.GetMonitorInfo();
    
        if (monitorInfo.Count == 0)
        {
            var noMonitorsItem = monitorsMenu.DropDownItems.Add("No monitors detected");
            noMonitorsItem.Enabled = false;
            return;
        }
    
        // Add each monitor to the submenu
        for (var i = 0; i < monitorInfo.Count; i++)
        {
            var monitorItem = new ToolStripMenuItem(monitorInfo[i])
            {
                CheckOnClick = true,
                Checked = true // All monitors selected by default
            };
        
            var index = i; // Capture for lambda
            monitorItem.Click += (_, _) => 
            {
                BrightnessHelper.ToggleMonitor(index, monitorItem.Checked);
                // Immediately apply current brightness to see the effect
                BrightnessHelper.SetAllMonitorsBrightness(currentBrightness);
            };
        
            monitorsMenu.DropDownItems.Add(monitorItem);
        }
    
        // Add diagnostic info item
        monitorsMenu.DropDownItems.Add(new ToolStripSeparator());
        monitorsMenu.DropDownItems.Add("Show Detailed Monitor Info", null, ShowDetailedMonitorInfo);
    }

    /// <summary>
    /// Shows detailed monitor information in a message box
    /// </summary>
    public void ShowDetailedMonitorInfo(object? sender, EventArgs? e)
    {
        var detailedInfo = BrightnessHelper.GetDetailedMonitorInfo();
        MessageBox.Show(
            detailedInfo,
            @"Monitor Diagnostic Information",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    /// <summary>
    /// Gets the application icon or falls back to system icon if not available
    /// </summary>
    private static Icon GetApplicationIcon()
    {
        try
        {
            var assembly = typeof(TrayAppContext).Assembly;
            using var stream = assembly.GetManifestResourceStream("BrightnessHotKeys.BrightnessHotKeys.ico");
            if (stream != null)
            {
                return new Icon(stream);
            }
        }
        catch (Exception)
        {
            return SystemIcons.Error;
        }

        return SystemIcons.Exclamation;
    }

    /// <summary>
    /// Opens the dialog to configure hotkeys
    /// </summary>
    private void OpenHotkeyDialog(object? sender, EventArgs e)
    {
        // Temporarily disable hotkeys while dialog is open
        UnregisterHotKeys();

        using var dialog = new HotkeyDialog(settings);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            // Get updated settings
            var updatedSettings = dialog.GetUpdatedSettings();

            // Update settings
            settings.UpdateFrom(updatedSettings);
            settings.Save();

            RegisterHotKeys();
            ShowNotification("Hotkeys updated successfully");
        }
        else
        {
            RegisterHotKeys();
        }
    }

    /// <summary>
    /// Configure the application to run at Windows startup
    /// </summary>
    private static void SetStartupWithWindows(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (key == null)
            {
                return;
            }

            if (enable)
            {
                var appPath = Process.GetCurrentProcess().MainModule?.FileName ??
                              AppContext.BaseDirectory;
                key.SetValue("BrightnessHotkeys", $"\"{appPath}\"");
            }
            else
            {
                key.DeleteValue("BrightnessHotkeys", false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting startup: {ex.Message}");
        }
    }

    /// <summary>
    /// Clean up resources and exit the application
    /// </summary>
    private void Exit(object? sender, EventArgs e)
    {
        trayIcon.Visible = false;
        UnregisterHotKeys();
        Application.Exit();
    }

    /// <summary>
    /// Handles hotkey press events
    /// </summary>
    private void OnHotKeyPressed(int id)
    {
        switch (id)
        {
            case HotkeyUpId:
                SetBrightness(currentBrightness + settings.BrightnessStepSize);
                // Removed toast notification
                break;

            case HotkeyDownId:
                SetBrightness(currentBrightness - settings.BrightnessStepSize);
                // Removed toast notification
                break;
        }
    }

    /// <summary>
    /// Registers the configured hotkeys with the system
    /// </summary>
    private void RegisterHotKeys()
    {
        UnregisterHotKeys();

        // Configure brightness up hotkey
        uint modifiersUp = 0;
        if (settings.BrightnessUpCtrl) modifiersUp |= ModControl;
        if (settings.BrightnessUpAlt) modifiersUp |= ModAlt;
        if (settings.BrightnessUpShift) modifiersUp |= ModShift;

        // Configure brightness down hotkey
        uint modifiersDown = 0;
        if (settings.BrightnessDownCtrl) modifiersDown |= ModControl;
        if (settings.BrightnessDownAlt) modifiersDown |= ModAlt;
        if (settings.BrightnessDownShift) modifiersDown |= ModShift;

        // Register both hotkeys
        var upSuccess = RegisterHotKey(hiddenForm.Handle, HotkeyUpId, modifiersUp, (uint)settings.BrightnessUpKey);
        var downSuccess = RegisterHotKey(hiddenForm.Handle, HotkeyDownId, modifiersDown, (uint)settings.BrightnessDownKey);

        if (!upSuccess || !downSuccess)
        {
            ToastMessageUtility.ShowToast("Hotkey Registration Failed", "Some hotkeys could not be registered. They may be in use by another application.", ToolTipIcon.Warning);
        }
    }

    /// <summary>
    /// Unregisters previously registered hotkeys
    /// </summary>
    private void UnregisterHotKeys()
    {
        UnregisterHotKey(hiddenForm.Handle, HotkeyUpId);
        UnregisterHotKey(hiddenForm.Handle, HotkeyDownId);
    }

    /// <summary>
    /// Sets the brightness level and applies it to all monitors
    /// </summary>
    public void SetBrightness(int level)
    {
        currentBrightness = Math.Clamp(level, 0, 100);

        // Update the brightness display in the context menu
        if (trayIcon.ContextMenuStrip?.Items.Count > 0)
        {
            trayIcon.ContextMenuStrip.Items[0].Text = $@"Current Brightness: {currentBrightness}%";
        }

        // Show visual indicator
        brightnessIndicator.ShowBrightness(currentBrightness);

        var success = BrightnessHelper.SetAllMonitorsBrightness(currentBrightness);

        if (success)
        {
            // Save the current brightness level to settings
            settings.LastBrightness = currentBrightness;
            settings.Save();
        }
        else
        {
            ShowNotification("Could not adjust brightness. Make sure DDC/CI is enabled in your monitor settings.");
        }
    }

    /// <summary>
    /// Shows a notification in the system tray
    /// </summary>
    private void ShowNotification(string message)
    {
        trayIcon.BalloonTipTitle = @"Brightness Control";
        trayIcon.BalloonTipText = message;
        trayIcon.ShowBalloonTip(1000); // Reduced from 2000ms to 1000ms
    }

    /// <summary>
    /// Clean up resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Clean up managed resources
                UnregisterHotKeys();
                trayIcon.Dispose();
                hiddenForm.Dispose();
                brightnessIndicator.Dispose();
            }

            disposed = true;
        }

        base.Dispose(disposing);
    }
}


