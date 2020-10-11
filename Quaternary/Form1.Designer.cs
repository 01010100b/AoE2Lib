namespace Quaternary
{
    partial class Form1
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
            this.ButtonTest = new System.Windows.Forms.Button();
            this.ButtonCopy = new System.Windows.Forms.Button();
            this.TextBoxState = new System.Windows.Forms.TextBox();
            this.TextBoxLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ButtonTest
            // 
            this.ButtonTest.Location = new System.Drawing.Point(51, 41);
            this.ButtonTest.Name = "ButtonTest";
            this.ButtonTest.Size = new System.Drawing.Size(297, 84);
            this.ButtonTest.TabIndex = 0;
            this.ButtonTest.Text = "Test";
            this.ButtonTest.UseVisualStyleBackColor = true;
            this.ButtonTest.Click += new System.EventHandler(this.ButtonTest_Click);
            // 
            // ButtonCopy
            // 
            this.ButtonCopy.Location = new System.Drawing.Point(51, 131);
            this.ButtonCopy.Name = "ButtonCopy";
            this.ButtonCopy.Size = new System.Drawing.Size(297, 84);
            this.ButtonCopy.TabIndex = 1;
            this.ButtonCopy.Text = "Copy";
            this.ButtonCopy.UseVisualStyleBackColor = true;
            this.ButtonCopy.Click += new System.EventHandler(this.ButtonCopy_Click);
            // 
            // TextBoxState
            // 
            this.TextBoxState.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxState.Location = new System.Drawing.Point(385, 41);
            this.TextBoxState.Multiline = true;
            this.TextBoxState.Name = "TextBoxState";
            this.TextBoxState.ReadOnly = true;
            this.TextBoxState.Size = new System.Drawing.Size(510, 464);
            this.TextBoxState.TabIndex = 2;
            // 
            // TextBoxLog
            // 
            this.TextBoxLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxLog.Location = new System.Drawing.Point(51, 233);
            this.TextBoxLog.Multiline = true;
            this.TextBoxLog.Name = "TextBoxLog";
            this.TextBoxLog.ReadOnly = true;
            this.TextBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxLog.Size = new System.Drawing.Size(297, 272);
            this.TextBoxLog.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 535);
            this.Controls.Add(this.TextBoxLog);
            this.Controls.Add(this.TextBoxState);
            this.Controls.Add(this.ButtonCopy);
            this.Controls.Add(this.ButtonTest);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonTest;
        private System.Windows.Forms.Button ButtonCopy;
        private System.Windows.Forms.TextBox TextBoxState;
        private System.Windows.Forms.TextBox TextBoxLog;
    }
}

