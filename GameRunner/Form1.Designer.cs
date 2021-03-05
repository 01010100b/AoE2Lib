
namespace GameRunner
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
            this.ButtonDev = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonDev
            // 
            this.ButtonDev.Location = new System.Drawing.Point(43, 29);
            this.ButtonDev.Name = "ButtonDev";
            this.ButtonDev.Size = new System.Drawing.Size(251, 85);
            this.ButtonDev.TabIndex = 0;
            this.ButtonDev.Text = "button1";
            this.ButtonDev.UseVisualStyleBackColor = true;
            this.ButtonDev.Click += new System.EventHandler(this.ButtonDev_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ButtonDev);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonDev;
    }
}

