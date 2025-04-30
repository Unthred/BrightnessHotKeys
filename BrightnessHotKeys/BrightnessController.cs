using System.Management;

namespace BrightnessHotKeys;

public static class BrightnessController
{
    public static int GetCurrentBrightness()
    {
        using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorBrightness");
        foreach (var o in searcher.Get())
        {
            var obj = (ManagementObject)o;
            return Convert.ToInt32(obj["CurrentBrightness"]);
        }

        return 50; // fallback
    }

    public static void SetBrightness(int brightness)
    {
        brightness = Math.Clamp(brightness, 0, 100);

        using var mclass = new ManagementClass("WmiMonitorBrightnessMethods");
        mclass.Scope = new ManagementScope(@"\\.\root\wmi");

        foreach (var o in mclass.GetInstances())
        {
            var instance = (ManagementObject) o;
            var args = new object[] { 1, brightness };
            instance.InvokeMethod("WmiSetBrightness", args);
        }
    }

    public static int AdjustBrightness(int delta)
    {
        int current = GetCurrentBrightness();
        int newValue = Math.Clamp(current + delta, 0, 100);
        SetBrightness(newValue);
        return newValue;
    }
}