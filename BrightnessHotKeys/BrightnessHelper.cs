using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace BrightnessHotKeys;

/// <summary>
/// Provides functionality to adjust brightness on both internal and external displays
/// </summary>
public static class BrightnessHelper
{
    // Keep track of which monitors are enabled
    private static readonly List<bool> EnabledMonitors = new();

    /// <summary>
    /// Checks if an internal laptop display is available
    /// </summary>
    public static bool IsLaptopDisplayAvailable()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorBrightness");
            return searcher.Get().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enable or disable a specific monitor for brightness control
    /// </summary>
    public static void ToggleMonitor(int index, bool enabled)
    {
        if (index >= 0)
        {
            // Ensure the list is big enough
            while (EnabledMonitors.Count <= index)
            {
                EnabledMonitors.Add(true); // Default to enabled
            }
            
            EnabledMonitors[index] = enabled;
        }
    }
    
    /// <summary>
    /// Gets basic information about all detected monitors
    /// </summary>
    public static List<string> GetMonitorInfo()
    {
        var result = new List<string>();
        try
        {
            var monitors = GetPhysicalMonitors().ToArray();
            int index = 0;
            
            // Check for laptop display first
            if (IsLaptopDisplayAvailable())
            {
                result.Add($"Monitor {index++}: Internal Display (Laptop)");
            }
            
            foreach (var monitor in monitors)
            {
                try
                {
                    uint min = 0, current = 0, max = 0;
                    bool supportsControl = GetMonitorBrightness(monitor, ref min, ref current, ref max);
                    result.Add($"Monitor {index++}: External Display - Brightness Control: {(supportsControl ? "Available" : "Unavailable")}");
                }
                catch
                {
                    result.Add($"Monitor {index++}: External Display - Error Accessing");
                }
                finally
                {
                    DestroyPhysicalMonitor(monitor);
                }
            }
        }
        catch (Exception ex)
        {
            result.Add($"Error detecting monitors: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets detailed diagnostic information about monitors
    /// </summary>
    public static string GetDetailedMonitorInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("MONITOR DIAGNOSTIC INFORMATION");
        sb.AppendLine("==============================");
        
        try
        {
            // Report laptop display
            sb.AppendLine("INTERNAL DISPLAY:");
            sb.AppendLine($"- Available: {IsLaptopDisplayAvailable()}");
            if (IsLaptopDisplayAvailable())
            {
                try
                {
                    sb.AppendLine($"- Current brightness: {GetCurrentBrightness()}%");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"- Error reading brightness: {ex.Message}");
                }
            }
            sb.AppendLine();
            
            // Report external monitors
            sb.AppendLine("EXTERNAL MONITORS:");
            var monitors = GetPhysicalMonitors().ToArray();
            if (monitors.Length == 0)
            {
                sb.AppendLine("- No external monitors detected");
            }
            else
            {
                for (int i = 0; i < monitors.Length; i++)
                {
                    sb.AppendLine($"MONITOR #{i+1}:");
                    try
                    {
                        uint min = 0, current = 0, max = 0;
                        bool supportsControl = GetMonitorBrightness(monitors[i], ref min, ref current, ref max);
                        
                        sb.AppendLine($"- Supports DDC/CI brightness: {supportsControl}");
                        sb.AppendLine($"- Current brightness: {current}");
                        sb.AppendLine($"- Min brightness: {min}");
                        sb.AppendLine($"- Max brightness: {max}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"- Error: {ex.Message}");
                    }
                    finally
                    {
                        DestroyPhysicalMonitor(monitors[i]);
                    }
                    
                    sb.AppendLine();
                }
            }
            
            // Add system info that might be relevant
            sb.AppendLine("SYSTEM INFORMATION:");
            sb.AppendLine($"- OS: {Environment.OSVersion}");
            sb.AppendLine($"- 64-bit OS: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"- .NET Runtime: {Environment.Version}");
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error generating diagnostic info: {ex.Message}");
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Sets brightness on all enabled monitors
    /// </summary>
    public static bool SetAllMonitorsBrightness(int brightness)
    {
        bool anySuccess = false;
        
        // Try setting laptop display brightness if available
        if (IsLaptopDisplayAvailable() && (EnabledMonitors.Count == 0 || EnabledMonitors[0]))
        {
            if (SetInternalDisplayBrightness(brightness))
            {
                anySuccess = true;
            }
        }
        
        // Try setting external monitor brightness
        try
        {
            var monitors = GetPhysicalMonitors().ToArray();
            for (int i = 0; i < monitors.Length; i++)
            {
                // Skip disabled monitors
                int monitorIndex = IsLaptopDisplayAvailable() ? i + 1 : i;
                if (EnabledMonitors.Count > monitorIndex && !EnabledMonitors[monitorIndex])
                {
                    DestroyPhysicalMonitor(monitors[i]);
                    continue;
                }
                
                try
                {
                    uint min = 0, current = 0, max = 0;
                    if (GetMonitorBrightness(monitors[i], ref min, ref current, ref max))
                    {
                        if (SetMonitorBrightness(monitors[i], brightness))
                        {
                            anySuccess = true;
                        }
                    }
                }
                finally
                {
                    DestroyPhysicalMonitor(monitors[i]);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting brightness: {ex.Message}");
        }
        
        return anySuccess;
    }

    /// <summary>
    /// Attempts to adjust brightness on internal laptop display
    /// </summary>
    /// <param name="brightness">Brightness level (0-100)</param>
    /// <returns>True if operation was successful</returns>
    public static bool SetInternalDisplayBrightness(int brightness)
    {
        try
        {
            using var mclass = new ManagementClass("WmiMonitorBrightnessMethods");
            mclass.Scope = new ManagementScope(@"\\.\root\wmi");
            var instances = mclass.GetInstances();

            var foundInstance = false;
            foreach (var o in instances)
            {
                var instance = (ManagementObject) o;
                foundInstance = true;
                var args = new object[] { 1, brightness };
                instance.InvokeMethod("WmiSetBrightness", args);
            }

            return foundInstance;
        }
        catch (Exception)
        {
            return false; // WMI method not supported or failed
        }
    }

    /// <summary>
    /// Attempts to adjust brightness on external monitors using DDC/CI
    /// </summary>
    /// <param name="brightness">Brightness level (0-100)</param>
    /// <returns>True if operation was successful on at least one monitor</returns>
    public static bool SetExternalMonitorBrightness(int brightness)
    {
        try
        {
            var monitors = GetPhysicalMonitors().ToArray();
            var success = false;
        
            foreach (var monitor in monitors)
            {
                try
                {
                    // Check if this monitor supports brightness control
                    uint min = 0, current = 0, max = 0;
                    if (GetMonitorBrightness(monitor, ref min, ref current, ref max))
                    {
                        // Only attempt to set brightness if the monitor supports the capability
                        if (max > 0 && SetMonitorBrightness(monitor, brightness))
                        {
                            success = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting brightness on monitor: {ex.Message}");
                }
                finally
                {
                    // Always clean up handles
                    DestroyPhysicalMonitor(monitor);
                }
            }

            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SetExternalMonitorBrightness: {ex.Message}");
            return false; // DDC/CI failed or not supported
        }
    }

    /// <summary>
    /// Attempts to get the current brightness level
    /// </summary>
    /// <returns>Brightness level (0-100) or null if it couldn't be determined</returns>
    public static int? GetCurrentBrightness()
    {
        try
        {
            // Try internal display first
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorBrightness");
            foreach (var o in searcher.Get())
            {
                var instance = (ManagementObject) o;
                var curBrightness = (byte)instance["CurrentBrightness"];
                return curBrightness;
            }

            // Try DDC/CI for external monitors
            var monitors = GetPhysicalMonitors().ToArray();
            foreach (var monitor in monitors)
            {
                try
                {
                    uint minBrightness = 0, curBrightness = 0, maxBrightness = 0;
                    if (GetMonitorBrightness(monitor, ref minBrightness, ref curBrightness, ref maxBrightness))
                    {
                        var brightness = maxBrightness > 0
                            ? (int)(curBrightness * 100 / maxBrightness)
                            : (int)curBrightness;

                        DestroyPhysicalMonitor(monitor);
                        return brightness;
                    }
                    DestroyPhysicalMonitor(monitor);
                }
                catch
                {
                    DestroyPhysicalMonitor(monitor);
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Displays an overlay on each monitor to identify it.
    /// </summary>
    public static void IdentifyMonitors()
    {
        var monitors = GetPhysicalMonitors().ToArray();
        for (int i = 0; i < monitors.Length; i++)
        {
            // Create a new thread for each monitor to display the overlay
            int monitorIndex = i + 1; // 1-based index for display
            new Thread(() =>
            {
                using var form = new MonitorIdentifierForm(monitorIndex);
                form.ShowDialog();
            }).Start();
        }
    }

    /// <summary>
    /// A form to display the monitor identifier.
    /// </summary>
    private sealed class MonitorIdentifierForm : Form
    {
        public MonitorIdentifierForm(int monitorIndex)
        {
            // Set up the form
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Black;
            TransparencyKey = Color.Black; // Make the background transparent
            TopMost = true;

            // Set the size of the form
            Size = new Size(600, 600); // Larger size for better visibility

            // Center the form on the monitor
            var screen = Screen.AllScreens[monitorIndex - 1];
            Location = new Point(screen.Bounds.X + (screen.Bounds.Width - Width) / 2,
                screen.Bounds.Y + (screen.Bounds.Height - Height) / 2);

            // Add a label to display the monitor number
            var label = new Label
            {
                Text = $@"{monitorIndex}", // Only display the number
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 432, FontStyle.Bold), // 3x larger font size (144 * 3 = 432)
                ForeColor = Color.White // White text for visibility
            };
            Controls.Add(label);

            // Close the form after 3 seconds
            var timer = new System.Windows.Forms.Timer { Interval = 3000 };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();
        }
    }


    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetMonitorBrightness(
        IntPtr hMonitor,
        ref uint pdwMinimumBrightness,
        ref uint pdwCurrentBrightness,
        ref uint pdwMaximumBrightness);

    /// <summary>
    /// Gets handles to all physical monitors in the system
    /// </summary>
    private static IEnumerable<IntPtr> GetPhysicalMonitors()
    {
        var monitorHandles = new List<IntPtr>();
        var hdc = GetDC(IntPtr.Zero);

        try
        {
            EnumDisplayMonitors(hdc, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr _, ref Rect _, IntPtr _) =>
                {
                    uint count = 0;
                    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count))
                    {
                        return true;
                    }

                    var physicalMonitors = new PhysicalMonitor[count];
                    if (GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
                    {
                        monitorHandles.AddRange(physicalMonitors.Select(pm => pm.hPhysicalMonitor));
                    }
                    return true;
                }, IntPtr.Zero);
        }
        finally
        {
            ReleaseDC(IntPtr.Zero, hdc);
        }

        return monitorHandles;
    }

    /// <summary>
    /// Sets brightness on a specific physical monitor
    /// </summary>
    private static bool SetMonitorBrightness(IntPtr monitorHandle, int brightness)
    {
        try
        {
            brightness = Math.Clamp(brightness, 0, 100);
            return SetMonitorBrightness(monitorHandle, (uint)brightness);
        }
        catch
        {
            return false;
        }
    }

    #region P/Invoke Declarations

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(
        IntPtr hMonitor,
        ref uint pdwNumberOfPhysicalMonitors);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(
        IntPtr hMonitor,
        uint dwPhysicalMonitorArraySize,
        [Out] PhysicalMonitor[] pPhysicalMonitorArray);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool SetMonitorBrightness(
        IntPtr hMonitor,
        uint dwNewBrightness);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr lprcClip,
        MonitorEnumProc lpfnEnum,
        IntPtr dwData);

    private delegate bool MonitorEnumProc(
        IntPtr hMonitor,
        IntPtr hdcMonitor,
        ref Rect lprcMonitor,
        IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    private struct PhysicalMonitor
    {
        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    #endregion
}
