namespace BrightnessHotKeys;

sealed partial class HotkeyDialog
{
    private System.Windows.Forms.TextBox txtUpKey;
    private System.Windows.Forms.TextBox txtDownKey;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label lblInstructions;
    private System.Windows.Forms.Label lblBrightnessUp;
    private System.Windows.Forms.Label lblBrightnessDown;
    private System.Windows.Forms.Label lblNote;

    private void InitializeComponent()
    {
        this.txtUpKey = new System.Windows.Forms.TextBox();
        this.txtDownKey = new System.Windows.Forms.TextBox();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.lblInstructions = new System.Windows.Forms.Label();
        this.lblBrightnessUp = new System.Windows.Forms.Label();
        this.lblBrightnessDown = new System.Windows.Forms.Label();
        this.lblNote = new System.Windows.Forms.Label();
        this.SuspendLayout();

        // lblInstructions
        this.lblInstructions.AutoSize = true;
        this.lblInstructions.Location = new System.Drawing.Point(12, 12);
        this.lblInstructions.Name = "lblInstructions";
        this.lblInstructions.Size = new System.Drawing.Size(300, 15);
        this.lblInstructions.TabIndex = 0;
        this.lblInstructions.Text = "Click inside a textbox and press the key combination.";

        // lblBrightnessUp
        this.lblBrightnessUp.AutoSize = true;
        this.lblBrightnessUp.Location = new System.Drawing.Point(12, 40);
        this.lblBrightnessUp.Name = "lblBrightnessUp";
        this.lblBrightnessUp.Size = new System.Drawing.Size(85, 15);
        this.lblBrightnessUp.TabIndex = 1;
        this.lblBrightnessUp.Text = "Brightness Up:";

        // txtUpKey
        this.txtUpKey.Location = new System.Drawing.Point(120, 37);
        this.txtUpKey.Name = "txtUpKey";
        this.txtUpKey.Size = new System.Drawing.Size(200, 23);
        this.txtUpKey.TabIndex = 2;
        this.txtUpKey.ReadOnly = true;
        this.txtUpKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtUpKey_KeyDown);

        // lblBrightnessDown
        this.lblBrightnessDown.AutoSize = true;
        this.lblBrightnessDown.Location = new System.Drawing.Point(12, 70);
        this.lblBrightnessDown.Name = "lblBrightnessDown";
        this.lblBrightnessDown.Size = new System.Drawing.Size(100, 15);
        this.lblBrightnessDown.TabIndex = 3;
        this.lblBrightnessDown.Text = "Brightness Down:";

        // txtDownKey
        this.txtDownKey.Location = new System.Drawing.Point(120, 67);
        this.txtDownKey.Name = "txtDownKey";
        this.txtDownKey.Size = new System.Drawing.Size(200, 23);
        this.txtDownKey.TabIndex = 4;
        this.txtDownKey.ReadOnly = true;
        this.txtDownKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDownKey_KeyDown);

        // lblNote
        this.lblNote.AutoSize = true;
        this.lblNote.Location = new System.Drawing.Point(12, 100);
        this.lblNote.Name = "lblNote";
        this.lblNote.Size = new System.Drawing.Size(300, 15);
        this.lblNote.TabIndex = 5;
        this.lblNote.Text = "Note: These hotkeys will work system-wide.";

        // btnOK
        this.btnOK.Location = new System.Drawing.Point(160, 130);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(75, 23);
        this.btnOK.TabIndex = 6;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(245, 130);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 7;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

        // HotkeyDialog
        this.ClientSize = new System.Drawing.Size(340, 170);
        this.Controls.Add(this.lblInstructions);
        this.Controls.Add(this.lblBrightnessUp);
        this.Controls.Add(this.txtUpKey);
        this.Controls.Add(this.lblBrightnessDown);
        this.Controls.Add(this.txtDownKey);
        this.Controls.Add(this.lblNote);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.btnCancel);
        this.Name = "HotkeyDialog";
        this.Text = "Configure Brightness Hotkeys";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
