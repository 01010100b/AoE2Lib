namespace Quaternary
{
    partial class WallingForm
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
            this.ButtonGenerate = new System.Windows.Forms.Button();
            this.CheckShowGoals = new System.Windows.Forms.CheckBox();
            this.CheckShowInterior = new System.Windows.Forms.CheckBox();
            this.LabelWallCount = new System.Windows.Forms.Label();
            this.LabelInterior = new System.Windows.Forms.Label();
            this.CheckAllResources = new System.Windows.Forms.CheckBox();
            this.CheckOptimize = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ButtonGenerate
            // 
            this.ButtonGenerate.Location = new System.Drawing.Point(1022, 901);
            this.ButtonGenerate.Name = "ButtonGenerate";
            this.ButtonGenerate.Size = new System.Drawing.Size(204, 58);
            this.ButtonGenerate.TabIndex = 0;
            this.ButtonGenerate.Text = "Generate";
            this.ButtonGenerate.UseVisualStyleBackColor = true;
            this.ButtonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
            // 
            // CheckShowGoals
            // 
            this.CheckShowGoals.AutoSize = true;
            this.CheckShowGoals.Location = new System.Drawing.Point(1022, 878);
            this.CheckShowGoals.Name = "CheckShowGoals";
            this.CheckShowGoals.Size = new System.Drawing.Size(81, 17);
            this.CheckShowGoals.TabIndex = 1;
            this.CheckShowGoals.Text = "Show goals";
            this.CheckShowGoals.UseVisualStyleBackColor = true;
            // 
            // CheckShowInterior
            // 
            this.CheckShowInterior.AutoSize = true;
            this.CheckShowInterior.Location = new System.Drawing.Point(1022, 855);
            this.CheckShowInterior.Name = "CheckShowInterior";
            this.CheckShowInterior.Size = new System.Drawing.Size(87, 17);
            this.CheckShowInterior.TabIndex = 2;
            this.CheckShowInterior.Text = "Show interior";
            this.CheckShowInterior.UseVisualStyleBackColor = true;
            // 
            // LabelWallCount
            // 
            this.LabelWallCount.AutoSize = true;
            this.LabelWallCount.Location = new System.Drawing.Point(1019, 723);
            this.LabelWallCount.Name = "LabelWallCount";
            this.LabelWallCount.Size = new System.Drawing.Size(63, 13);
            this.LabelWallCount.TabIndex = 3;
            this.LabelWallCount.Text = "Wall length:";
            // 
            // LabelInterior
            // 
            this.LabelInterior.AutoSize = true;
            this.LabelInterior.Location = new System.Drawing.Point(1019, 736);
            this.LabelInterior.Name = "LabelInterior";
            this.LabelInterior.Size = new System.Drawing.Size(42, 13);
            this.LabelInterior.TabIndex = 4;
            this.LabelInterior.Text = "Interior:";
            // 
            // CheckAllResources
            // 
            this.CheckAllResources.AutoSize = true;
            this.CheckAllResources.Location = new System.Drawing.Point(1022, 832);
            this.CheckAllResources.Name = "CheckAllResources";
            this.CheckAllResources.Size = new System.Drawing.Size(113, 17);
            this.CheckAllResources.TabIndex = 5;
            this.CheckAllResources.Text = "Take all resources";
            this.CheckAllResources.UseVisualStyleBackColor = true;
            // 
            // CheckOptimize
            // 
            this.CheckOptimize.AutoSize = true;
            this.CheckOptimize.Location = new System.Drawing.Point(1022, 809);
            this.CheckOptimize.Name = "CheckOptimize";
            this.CheckOptimize.Size = new System.Drawing.Size(66, 17);
            this.CheckOptimize.TabIndex = 6;
            this.CheckOptimize.Text = "Optimize";
            this.CheckOptimize.UseVisualStyleBackColor = true;
            // 
            // WallingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1238, 971);
            this.Controls.Add(this.CheckOptimize);
            this.Controls.Add(this.CheckAllResources);
            this.Controls.Add(this.LabelInterior);
            this.Controls.Add(this.LabelWallCount);
            this.Controls.Add(this.CheckShowInterior);
            this.Controls.Add(this.CheckShowGoals);
            this.Controls.Add(this.ButtonGenerate);
            this.Name = "WallingForm";
            this.Text = "WallingForm";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.WallingForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.WallingForm_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonGenerate;
        private System.Windows.Forms.CheckBox CheckShowGoals;
        private System.Windows.Forms.CheckBox CheckShowInterior;
        private System.Windows.Forms.Label LabelWallCount;
        private System.Windows.Forms.Label LabelInterior;
        private System.Windows.Forms.CheckBox CheckAllResources;
        private System.Windows.Forms.CheckBox CheckOptimize;
    }
}