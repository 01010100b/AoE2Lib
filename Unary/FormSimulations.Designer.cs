
namespace Unary
{
    partial class FormSimulations
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
            this.components = new System.ComponentModel.Container();
            this.ButtonTick = new System.Windows.Forms.Button();
            this.TimerTick = new System.Windows.Forms.Timer(this.components);
            this.LabelGameTime = new System.Windows.Forms.Label();
            this.ButtonTest = new System.Windows.Forms.Button();
            this.TextOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ButtonTick
            // 
            this.ButtonTick.Location = new System.Drawing.Point(574, 30);
            this.ButtonTick.Name = "ButtonTick";
            this.ButtonTick.Size = new System.Drawing.Size(180, 46);
            this.ButtonTick.TabIndex = 0;
            this.ButtonTick.Text = "Start/Stop";
            this.ButtonTick.UseVisualStyleBackColor = true;
            this.ButtonTick.Click += new System.EventHandler(this.ButtonTick_Click);
            // 
            // TimerTick
            // 
            this.TimerTick.Tick += new System.EventHandler(this.TimerTick_Tick);
            // 
            // LabelGameTime
            // 
            this.LabelGameTime.AutoSize = true;
            this.LabelGameTime.Location = new System.Drawing.Point(579, 98);
            this.LabelGameTime.Name = "LabelGameTime";
            this.LabelGameTime.Size = new System.Drawing.Size(38, 15);
            this.LabelGameTime.TabIndex = 1;
            this.LabelGameTime.Text = "label1";
            // 
            // ButtonTest
            // 
            this.ButtonTest.Location = new System.Drawing.Point(574, 134);
            this.ButtonTest.Name = "ButtonTest";
            this.ButtonTest.Size = new System.Drawing.Size(180, 46);
            this.ButtonTest.TabIndex = 2;
            this.ButtonTest.Text = "Test 10 seconds";
            this.ButtonTest.UseVisualStyleBackColor = true;
            this.ButtonTest.Click += new System.EventHandler(this.ButtonTest_Click);
            // 
            // TextOutput
            // 
            this.TextOutput.Location = new System.Drawing.Point(575, 210);
            this.TextOutput.Multiline = true;
            this.TextOutput.Name = "TextOutput";
            this.TextOutput.ReadOnly = true;
            this.TextOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextOutput.Size = new System.Drawing.Size(178, 228);
            this.TextOutput.TabIndex = 3;
            // 
            // FormSimulations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TextOutput);
            this.Controls.Add(this.ButtonTest);
            this.Controls.Add(this.LabelGameTime);
            this.Controls.Add(this.ButtonTick);
            this.DoubleBuffered = true;
            this.Name = "FormSimulations";
            this.Text = "FormSimulations";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormSimulations_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonTick;
        private System.Windows.Forms.Timer TimerTick;
        private System.Windows.Forms.Label LabelGameTime;
        private System.Windows.Forms.Button ButtonTest;
        private System.Windows.Forms.TextBox TextOutput;
    }
}