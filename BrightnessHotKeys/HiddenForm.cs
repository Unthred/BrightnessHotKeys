namespace BrightnessHotKeys;

/// <summary>
/// A hidden form used to receive Windows messages for global hotkey events
/// </summary>
public class HiddenForm : Form
{
    // Windows message constants for hotkey handling
    private const int WmHotkey = 0x0312;

    /// <summary>
    /// Event that fires when a registered hotkey is pressed
    /// </summary>
    public event Action<int>? HotKeyPressed;

    /// <summary>
    /// Creates a new hidden form
    /// </summary>
    public HiddenForm()
    {
        // Set the form to be invisible
        Visible = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        Size = new Size(0, 0);
    }

    /// <summary>
    /// Processes Windows messages, watching for hotkey messages
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        // Check if message is a hotkey notification
        if (m.Msg == WmHotkey)
        {
            // Extract the hotkey ID
            var id = m.WParam.ToInt32();

            // Raise the event
            HotKeyPressed?.Invoke(id);
        }

        base.WndProc(ref m);
    }
}