
namespace Unary
{
    partial class FormUnary
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ButtonStart = new System.Windows.Forms.Button();
            this.TextMessages = new System.Windows.Forms.TextBox();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.NumericPlayer = new System.Windows.Forms.NumericUpDown();
            this.ButtonBrowseExe = new System.Windows.Forms.Button();
            this.LabelExePath = new System.Windows.Forms.Label();
            this.ButtonDev = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.NumericPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonStart
            // 
            this.ButtonStart.Enabled = false;
            this.ButtonStart.Location = new System.Drawing.Point(12, 47);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(133, 26);
            this.ButtonStart.TabIndex = 3;
            this.ButtonStart.Text = "Start for player";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // TextMessages
            // 
            this.TextMessages.Location = new System.Drawing.Point(12, 111);
            this.TextMessages.Multiline = true;
            this.TextMessages.Name = "TextMessages";
            this.TextMessages.ReadOnly = true;
            this.TextMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextMessages.Size = new System.Drawing.Size(776, 355);
            this.TextMessages.TabIndex = 4;
            this.TextMessages.TabStop = false;
            // 
            // ButtonStop
            // 
            this.ButtonStop.Enabled = false;
            this.ButtonStop.Location = new System.Drawing.Point(12, 79);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(133, 26);
            this.ButtonStop.TabIndex = 5;
            this.ButtonStop.Text = "Stop all players";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(151, 85);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(141, 15);
            this.Label1.TabIndex = 6;
            this.Label1.Text = "may take up to 5 seconds";
            // 
            // NumericPlayer
            // 
            this.NumericPlayer.Location = new System.Drawing.Point(151, 51);
            this.NumericPlayer.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.NumericPlayer.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericPlayer.Name = "NumericPlayer";
            this.NumericPlayer.Size = new System.Drawing.Size(35, 23);
            this.NumericPlayer.TabIndex = 8;
            this.NumericPlayer.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ButtonBrowseExe
            // 
            this.ButtonBrowseExe.Location = new System.Drawing.Point(12, 15);
            this.ButtonBrowseExe.Name = "ButtonBrowseExe";
            this.ButtonBrowseExe.Size = new System.Drawing.Size(133, 26);
            this.ButtonBrowseExe.TabIndex = 9;
            this.ButtonBrowseExe.Text = "Browse Exe Path";
            this.ButtonBrowseExe.UseVisualStyleBackColor = true;
            this.ButtonBrowseExe.Click += new System.EventHandler(this.ButtonBrowseExe_Click);
            // 
            // LabelExePath
            // 
            this.LabelExePath.AutoSize = true;
            this.LabelExePath.Location = new System.Drawing.Point(151, 21);
            this.LabelExePath.Name = "LabelExePath";
            this.LabelExePath.Size = new System.Drawing.Size(70, 15);
            this.LabelExePath.TabIndex = 10;
            this.LabelExePath.Text = "path-to-exe";
            // 
            // ButtonDev
            // 
            this.ButtonDev.Enabled = false;
            this.ButtonDev.Location = new System.Drawing.Point(655, 74);
            this.ButtonDev.Name = "ButtonDev";
            this.ButtonDev.Size = new System.Drawing.Size(133, 26);
            this.ButtonDev.TabIndex = 11;
            this.ButtonDev.Text = "Dev";
            this.ButtonDev.UseVisualStyleBackColor = true;
            this.ButtonDev.Click += new System.EventHandler(this.ButtonDev_Click);
            // 
            // FormUnary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 472);
            this.Controls.Add(this.ButtonDev);
            this.Controls.Add(this.LabelExePath);
            this.Controls.Add(this.ButtonBrowseExe);
            this.Controls.Add(this.NumericPlayer);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.ButtonStop);
            this.Controls.Add(this.TextMessages);
            this.Controls.Add(this.ButtonStart);
            this.Name = "FormUnary";
            this.Text = "Unary";
            ((System.ComponentModel.ISupportInitialize)(this.NumericPlayer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.TextBox TextMessages;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.NumericUpDown NumericPlayer;
        private System.Windows.Forms.Button ButtonBrowseExe;
        private System.Windows.Forms.Label LabelExePath;
        private System.Windows.Forms.Button ButtonDev;
    }
}

