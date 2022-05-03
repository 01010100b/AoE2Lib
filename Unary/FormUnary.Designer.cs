
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
            this.TextProcess = new System.Windows.Forms.TextBox();
            this.ButtonConnect = new System.Windows.Forms.Button();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.TextMessages = new System.Windows.Forms.TextBox();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.NumericPlayer = new System.Windows.Forms.NumericUpDown();
            this.ButtonBrowseExe = new System.Windows.Forms.Button();
            this.LabelExePath = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NumericPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // TextProcess
            // 
            this.TextProcess.Location = new System.Drawing.Point(235, 114);
            this.TextProcess.Name = "TextProcess";
            this.TextProcess.Size = new System.Drawing.Size(87, 23);
            this.TextProcess.TabIndex = 0;
            this.TextProcess.Text = "WK";
            // 
            // ButtonConnect
            // 
            this.ButtonConnect.Location = new System.Drawing.Point(12, 55);
            this.ButtonConnect.Name = "ButtonConnect";
            this.ButtonConnect.Size = new System.Drawing.Size(133, 26);
            this.ButtonConnect.TabIndex = 1;
            this.ButtonConnect.Text = "Connect to process";
            this.ButtonConnect.UseVisualStyleBackColor = true;
            this.ButtonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Enabled = false;
            this.ButtonStart.Location = new System.Drawing.Point(12, 177);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(133, 26);
            this.ButtonStart.TabIndex = 3;
            this.ButtonStart.Text = "Start for player";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // TextMessages
            // 
            this.TextMessages.Enabled = false;
            this.TextMessages.Location = new System.Drawing.Point(12, 287);
            this.TextMessages.Multiline = true;
            this.TextMessages.Name = "TextMessages";
            this.TextMessages.ReadOnly = true;
            this.TextMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextMessages.Size = new System.Drawing.Size(776, 309);
            this.TextMessages.TabIndex = 4;
            // 
            // ButtonStop
            // 
            this.ButtonStop.Enabled = false;
            this.ButtonStop.Location = new System.Drawing.Point(12, 209);
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
            this.Label1.Location = new System.Drawing.Point(151, 215);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(141, 15);
            this.Label1.TabIndex = 6;
            this.Label1.Text = "may take up to 5 seconds";
            // 
            // NumericPlayer
            // 
            this.NumericPlayer.Location = new System.Drawing.Point(151, 181);
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
            this.LabelExePath.Size = new System.Drawing.Size(141, 15);
            this.LabelExePath.TabIndex = 10;
            this.LabelExePath.Text = "may take up to 5 seconds";
            // 
            // FormUnary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 608);
            this.Controls.Add(this.LabelExePath);
            this.Controls.Add(this.ButtonBrowseExe);
            this.Controls.Add(this.NumericPlayer);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.ButtonStop);
            this.Controls.Add(this.TextMessages);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.ButtonConnect);
            this.Controls.Add(this.TextProcess);
            this.Name = "FormUnary";
            this.Text = "Unary";
            ((System.ComponentModel.ISupportInitialize)(this.NumericPlayer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextProcess;
        private System.Windows.Forms.Button ButtonConnect;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.TextBox TextMessages;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.NumericUpDown NumericPlayer;
        private System.Windows.Forms.Button ButtonBrowseExe;
        private System.Windows.Forms.Label LabelExePath;
    }
}

