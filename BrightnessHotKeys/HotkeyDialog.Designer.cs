namespace BrightnessHotKeys;
// 
partial class HotkeyDialog
{
    private System.Windows.Forms.TextBox txtUpKey;
    private System.Windows.Forms.TextBox txtDownKey;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;

    private void InitializeComponent()
    {
        this.txtUpKey = new System.Windows.Forms.TextBox();
        this.txtDownKey = new System.Windows.Forms.TextBox();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();

        // txtUpKey
        this.txtUpKey.Location = new System.Drawing.Point(12, 12);
        this.txtUpKey.Name = "txtUpKey";
        this.txtUpKey.Size = new System.Drawing.Size(260, 23);
        this.txtUpKey.TabIndex = 0;
        this.txtUpKey.ReadOnly = true;
        this.txtUpKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtUpKey_KeyDown);

        // txtDownKey
        this.txtDownKey.Location = new System.Drawing.Point(12, 41);
        this.txtDownKey.Name = "txtDownKey";
        this.txtDownKey.Size = new System.Drawing.Size(260, 23);
        this.txtDownKey.TabIndex = 1;
        this.txtDownKey.ReadOnly = true;
        this.txtDownKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDownKey_KeyDown);

        // btnOK
        this.btnOK.Location = new System.Drawing.Point(116, 70);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(75, 23);
        this.btnOK.TabIndex = 2;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(197, 70);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 3;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

        // HotkeyDialog
        this.ClientSize = new System.Drawing.Size(284, 105);
        this.Controls.Add(this.txtUpKey);
        this.Controls.Add(this.txtDownKey);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.btnCancel);
        this.Name = "HotkeyDialog";
        this.Text = "Set Brightness Hotkeys";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
