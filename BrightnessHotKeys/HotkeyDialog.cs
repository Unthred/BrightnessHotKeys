namespace BrightnessHotKeys;

/// <summary>
/// Dialog for configuring keyboard hotkeys
/// </summary>
public partial class HotkeyDialog : Form
{
    // Brightness up hotkey settings
    private Keys upKey;
    private bool upCtrl;
    private bool upAlt;
    private bool upShift;

    // Brightness down hotkey settings
    private Keys downKey;
    private bool downCtrl;
    private bool downAlt;
    private bool downShift;

    /// <summary>
    /// Initializes the hotkey dialog with the current settings
    /// </summary>
    public HotkeyDialog(SettingsManager current)
    {
        InitializeComponent();

        // Load existing settings
        upKey = current.BrightnessUpKey;
        upCtrl = current.BrightnessUpCtrl;
        upAlt = current.BrightnessUpAlt;
        upShift = current.BrightnessUpShift;

        downKey = current.BrightnessDownKey;
        downCtrl = current.BrightnessDownCtrl;
        downAlt = current.BrightnessDownAlt;
        downShift = current.BrightnessDownShift;

        UpdateTextBoxes();
    }

    public HotkeyDialog(Keys downKey)
    {
        this.downKey = downKey;
    }

    /// <summary>
    /// Updates the text boxes to show the currently configured hotkeys
    /// </summary>
    private void UpdateTextBoxes()
    {
        txtUpKey.Text = GetKeyString(upCtrl, upAlt, upShift, upKey);
        txtDownKey.Text = GetKeyString(downCtrl, downAlt, downShift, downKey);
    }

    /// <summary>
    /// Formats a key combination as a readable string
    /// </summary>
    private static string GetKeyString(bool ctrl, bool alt, bool shift, Keys key)
    {
        var modifiers = new List<string>();

        if (ctrl) modifiers.Add("Ctrl");
        if (alt) modifiers.Add("Alt");
        if (shift) modifiers.Add("Shift");

        string keyText = key.ToString();

        return modifiers.Count > 0
            ? string.Join(" + ", modifiers) + " + " + keyText
            : keyText;
    }

    /// <summary>
    /// Handles key down events for the brightness up hotkey
    /// </summary>
    private void txtUpKey_KeyDown(object sender, KeyEventArgs e)
    {
        upCtrl = e.Control;
        upAlt = e.Alt;
        upShift = e.Shift;
        upKey = e.KeyCode;
        UpdateTextBoxes();
        e.SuppressKeyPress = true;
    }

    /// <summary>
    /// Handles key down events for the brightness down hotkey
    /// </summary>
    private void txtDownKey_KeyDown(object sender, KeyEventArgs e)
    {
        downCtrl = e.Control;
        downAlt = e.Alt;
        downShift = e.Shift;
        downKey = e.KeyCode;
        UpdateTextBoxes();
        e.SuppressKeyPress = true;
    }

    /// <summary>
    /// Creates an updated settings object with the new hotkey configurations
    /// </summary>
    public SettingsManager GetUpdatedSettings() => new SettingsManager
    {
        BrightnessUpKey = upKey,
        BrightnessUpCtrl = upCtrl,
        BrightnessUpAlt = upAlt,
        BrightnessUpShift = upShift,
        BrightnessDownKey = downKey,
        BrightnessDownCtrl = downCtrl,
        BrightnessDownAlt = downAlt,
        BrightnessDownShift = downShift
    };

    /// <summary>
    /// Handles the OK button click
    /// </summary>
    private void btnOK_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;

    /// <summary>
    /// Handles the Cancel button click
    /// </summary>
    private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
}
