namespace BrightnessHotKeys;

/// <summary>
/// Dialog for configuring keyboard hotkeys
/// </summary>
public sealed partial class HotkeyDialog : Form
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

        // Set a meaningful title for the form
        Text = @"Configure Brightness Hotkeys";

        // Add instructions label at the top
        var lblInstructions = new Label
        {
            Text = @"Click inside a textbox and press the key combination you want to use.",
            AutoSize = true,
            Location = new Point(12, 12),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(lblInstructions);

        // Add labels for the textboxes
        var lblBrightnessUp = new Label
        {
            Text = @"Brightness Up:",
            AutoSize = true,
            Location = new Point(12, lblInstructions.Bottom + 20)
        };
        Controls.Add(lblBrightnessUp);

        // Position the txtUpKey below its label
        txtUpKey.Location = new Point(120, lblBrightnessUp.Location.Y - 3);
        txtUpKey.Width = 200;

        var lblBrightnessDown = new Label
        {
            Text = @"Brightness Down:",
            AutoSize = true,
            Location = new Point(12, lblBrightnessUp.Bottom + 20)
        };
        Controls.Add(lblBrightnessDown);

        // Position the txtDownKey below its label
        txtDownKey.Location = new Point(120, lblBrightnessDown.Location.Y - 3);
        txtDownKey.Width = 200;

        // Add a note about system-wide hotkeys
        var lblNote = new Label
        {
            Text = @"Note: These hotkeys will work system-wide, even when the app is minimized.",
            AutoSize = true,
            Location = new Point(12, lblBrightnessDown.Bottom + 20),
            Font = new Font(this.Font, FontStyle.Italic)
        };
        Controls.Add(lblNote);

        // Position the OK and Cancel buttons
        btnOK.Location = new Point(this.Width - 170, lblNote.Bottom + 20);
        btnCancel.Location = new Point(this.Width - 89, lblNote.Bottom + 20);

        // Adjust form height to accommodate all controls
        Height = btnOK.Bottom + 50;

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

    // Remove the parameterless constructor or fix it to properly initialize all UI elements
    // This constructor appears to be unused based on the available code
    public HotkeyDialog(Keys downKey)
    {
        InitializeComponent();
        this.downKey = downKey;
        // This constructor should include the UI initialization as well
        // For now, just warn of the issue
        MessageBox.Show(@"Note: This constructor doesn't fully initialize the dialog.",
            @"Initialization Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        var keyText = key.ToString();

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
