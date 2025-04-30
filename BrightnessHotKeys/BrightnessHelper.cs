using System.Management;
using System.Runtime.InteropServices;

namespace BrightnessHotKeys;

/// <summary>
/// Provides functionality to adjust brightness on both internal and external displays
/// </summary>
public static class BrightnessHelper
{
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
            var monitors = GetPhysicalMonitors();
            var success = false;

            foreach (var monitor in monitors)
            {
                if (SetMonitorBrightness(monitor, brightness))
                {
                    success = true;
                }
                // Always clean up handles
                DestroyPhysicalMonitor(monitor);
            }

            return success;
        }
        catch (Exception)
        {
            return false; // DDC/CI failed or not supported
        }
    }

    /// <summary>
    /// Attempts to adjust brightness on all connected monitors
    /// </summary>
    /// <param name="brightness">Brightness level (0-100)</param>
    /// <returns>True if brightness was adjusted on at least one display</returns>
    public static bool SetAllMonitorsBrightness(int brightness)
    {
        brightness = Math.Clamp(brightness, 0, 100);
        var success = false;

        // Try internal display first
        try
        {
            if (SetInternalDisplayBrightness(brightness))
            {
                success = true;
            }
        }
        catch (Exception)
        {
            // Ignore internal display failures
        }

        // Try external monitors next
        try
        {
            if (SetExternalMonitorBrightness(brightness))
            {
                success = true;
            }
        }
        catch (Exception)
        {
            // Ignore external display failures
        }

        return success;
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
            var monitors = GetPhysicalMonitors();
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

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetMonitorBrightness(
        IntPtr hMonitor,
        ref uint pdwMinimumBrightness,
        ref uint pdwCurrentBrightness,
        ref uint pdwMaximumBrightness);

    ///// <summary>
    ///// Checks if an internal laptop display is available
    ///// </summary>
    //private static bool IsInternalDisplayConnected()
    //{
    //    try
    //    {
    //        using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorBrightness");
    //        return searcher.Get().Count > 0;
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}

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
