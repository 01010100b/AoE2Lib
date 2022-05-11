namespace Unary.UI
{
    partial class FormDev
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TextMessages = new System.Windows.Forms.TextBox();
            this.ButtonScenarios = new System.Windows.Forms.Button();
            this.ButtonLadder = new System.Windows.Forms.Button();
            this.ButtonStopLadder = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextMessages
            // 
            this.TextMessages.Location = new System.Drawing.Point(12, 161);
            this.TextMessages.Multiline = true;
            this.TextMessages.Name = "TextMessages";
            this.TextMessages.ReadOnly = true;
            this.TextMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextMessages.Size = new System.Drawing.Size(776, 381);
            this.TextMessages.TabIndex = 0;
            // 
            // ButtonScenarios
            // 
            this.ButtonScenarios.Location = new System.Drawing.Point(12, 17);
            this.ButtonScenarios.Name = "ButtonScenarios";
            this.ButtonScenarios.Size = new System.Drawing.Size(160, 41);
            this.ButtonScenarios.TabIndex = 1;
            this.ButtonScenarios.Text = "Run Scenarios";
            this.ButtonScenarios.UseVisualStyleBackColor = true;
            this.ButtonScenarios.Click += new System.EventHandler(this.ButtonScenarios_Click);
            // 
            // ButtonLadder
            // 
            this.ButtonLadder.Location = new System.Drawing.Point(12, 64);
            this.ButtonLadder.Name = "ButtonLadder";
            this.ButtonLadder.Size = new System.Drawing.Size(160, 41);
            this.ButtonLadder.TabIndex = 2;
            this.ButtonLadder.Text = "Run Ladder";
            this.ButtonLadder.UseVisualStyleBackColor = true;
            this.ButtonLadder.Click += new System.EventHandler(this.ButtonLadder_Click);
            // 
            // ButtonStopLadder
            // 
            this.ButtonStopLadder.Enabled = false;
            this.ButtonStopLadder.Location = new System.Drawing.Point(178, 64);
            this.ButtonStopLadder.Name = "ButtonStopLadder";
            this.ButtonStopLadder.Size = new System.Drawing.Size(160, 41);
            this.ButtonStopLadder.TabIndex = 3;
            this.ButtonStopLadder.Text = "Stop Ladder";
            this.ButtonStopLadder.UseVisualStyleBackColor = true;
            this.ButtonStopLadder.Click += new System.EventHandler(this.ButtonStopLadder_Click);
            // 
            // FormDev
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 554);
            this.Controls.Add(this.ButtonStopLadder);
            this.Controls.Add(this.ButtonLadder);
            this.Controls.Add(this.ButtonScenarios);
            this.Controls.Add(this.TextMessages);
            this.Name = "FormDev";
            this.Text = "FormDev";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextMessages;
        private System.Windows.Forms.Button ButtonScenarios;
        private System.Windows.Forms.Button ButtonLadder;
        private System.Windows.Forms.Button ButtonStopLadder;
    }
}