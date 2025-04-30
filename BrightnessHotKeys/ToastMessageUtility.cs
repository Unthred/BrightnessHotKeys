namespace BrightnessHotKeys;

/// <summary>
/// Utility class for displaying toast messages using NotifyIcon
/// </summary>
public static class ToastMessageUtility
{
    /// <summary>
    /// Displays a toast message with the specified title, message, and icon type.
    /// </summary>
    /// <param name="title">The title of the toast message.</param>
    /// <param name="message">The message content of the toast.</param>
    /// <param name="icon">The icon to display (e.g., Info, Warning, Error).</param>
    /// <param name="duration">The duration in milliseconds to display the toast (default is 3000ms).</param>
    public static void ShowToast(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int duration = 3000)
    {
        using var notifyIcon = new NotifyIcon();
        notifyIcon.Icon = SystemIcons.Information; // Default icon
        notifyIcon.Visible = true;

        // Set the icon based on the type
        switch (icon)
        {
            case ToolTipIcon.Warning:
                notifyIcon.Icon = SystemIcons.Warning;
                break;
            case ToolTipIcon.Error:
                notifyIcon.Icon = SystemIcons.Error;
                break;
            case ToolTipIcon.Info:
            default:
                notifyIcon.Icon = SystemIcons.Information;
                break;
        }

        // Configure the balloon tip
        notifyIcon.BalloonTipTitle = title;
        notifyIcon.BalloonTipText = message;
        notifyIcon.BalloonTipIcon = icon;

        // Show the balloon tip
        notifyIcon.ShowBalloonTip(duration);

        // Dispose the NotifyIcon after the toast is shown
        Task.Delay(duration).ContinueWith(_ => notifyIcon.Dispose());
    }
}