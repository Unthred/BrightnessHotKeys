namespace BrightnessHotKeys;

/// <summary>
/// A simple form to display the current brightness level
/// </summary>
public sealed class BrightnessIndicator : Form
{
    private readonly ProgressBar progressBar;
    private readonly System.Windows.Forms.Timer timer; // Explicitly specify the Timer type

    public BrightnessIndicator()
    {
        // Calculate position to appear in the lower right corner (like toast notifications)
        if (Screen.PrimaryScreen != null)
        {
            var screenRight = Screen.PrimaryScreen.WorkingArea.Right;
            var screenBottom = Screen.PrimaryScreen.WorkingArea.Bottom;

            // Make the indicator smaller with reduced height
            var width = 250;
            var height = 20; // 30% smaller than original 40px height

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(screenRight - width - 10, screenBottom - height - 10);
            Size = new Size(width, height);
        }

        BackColor = Color.FromArgb(40, 40, 40); // Set the background color directly
        ShowInTaskbar = false;
        TopMost = true;

        // Add a subtle border
        Padding = new Padding(1);

        progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Dock = DockStyle.Fill,
            ForeColor = Color.DodgerBlue // Brighter blue color
        };
        Controls.Add(progressBar);

        // Shorter display time
        timer = new System.Windows.Forms.Timer { Interval = 500 };
        timer.Tick += (_, _) => { timer.Stop(); Hide(); };
    }

    public void ShowBrightness(int level)
    {
        progressBar.Value = level;
        Show();
        timer.Stop();
        timer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Add a border
        using var pen = new Pen(Color.FromArgb(70, 70, 70));
        e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Dispose();
        }
        base.Dispose(disposing);
    }
}
