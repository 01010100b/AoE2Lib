
namespace ExampleBot
{
    partial class Form1
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
            this.TextPlayer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TextProcess
            // 
            this.TextProcess.Location = new System.Drawing.Point(35, 28);
            this.TextProcess.Name = "TextProcess";
            this.TextProcess.Size = new System.Drawing.Size(145, 23);
            this.TextProcess.TabIndex = 0;
            this.TextProcess.Text = "WK";
            // 
            // ButtonConnect
            // 
            this.ButtonConnect.Location = new System.Drawing.Point(194, 28);
            this.ButtonConnect.Name = "ButtonConnect";
            this.ButtonConnect.Size = new System.Drawing.Size(111, 22);
            this.ButtonConnect.TabIndex = 1;
            this.ButtonConnect.Text = "Connect";
            this.ButtonConnect.UseVisualStyleBackColor = true;
            this.ButtonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(194, 80);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(111, 22);
            this.ButtonStart.TabIndex = 3;
            this.ButtonStart.Text = "Start for player";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // TextPlayer
            // 
            this.TextPlayer.Location = new System.Drawing.Point(35, 80);
            this.TextPlayer.Name = "TextPlayer";
            this.TextPlayer.Size = new System.Drawing.Size(145, 23);
            this.TextPlayer.TabIndex = 2;
            this.TextPlayer.Text = "1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.TextPlayer);
            this.Controls.Add(this.ButtonConnect);
            this.Controls.Add(this.TextProcess);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextProcess;
        private System.Windows.Forms.Button ButtonConnect;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.TextBox TextPlayer;
    }
}

