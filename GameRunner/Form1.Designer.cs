
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
            this.TextExe = new System.Windows.Forms.TextBox();
            this.ButtonSetExe = new System.Windows.Forms.Button();
            this.ButtonSetAiFolder = new System.Windows.Forms.Button();
            this.TextAiFolder = new System.Windows.Forms.TextBox();
            this.ComboGameType = new System.Windows.Forms.ComboBox();
            this.LabelGameType = new System.Windows.Forms.Label();
            this.LabelMapType = new System.Windows.Forms.Label();
            this.ComboMapType = new System.Windows.Forms.ComboBox();
            this.LabelDifficulty = new System.Windows.Forms.Label();
            this.ComboDifficulty = new System.Windows.Forms.ComboBox();
            this.LabelStartingResources = new System.Windows.Forms.Label();
            this.ComboStartingResources = new System.Windows.Forms.ComboBox();
            this.LabelRevealMap = new System.Windows.Forms.Label();
            this.ComboRevealMap = new System.Windows.Forms.ComboBox();
            this.LabelStartingAge = new System.Windows.Forms.Label();
            this.ComboStartingAge = new System.Windows.Forms.ComboBox();
            this.LabelVictoryType = new System.Windows.Forms.Label();
            this.ComboVictoryType = new System.Windows.Forms.ComboBox();
            this.CheckTeamsTogether = new System.Windows.Forms.CheckBox();
            this.CheckLockTeams = new System.Windows.Forms.CheckBox();
            this.CheckAllTech = new System.Windows.Forms.CheckBox();
            this.CheckRecorded = new System.Windows.Forms.CheckBox();
            this.LabelMapSize = new System.Windows.Forms.Label();
            this.ComboMapSize = new System.Windows.Forms.ComboBox();
            this.LabelScenario = new System.Windows.Forms.Label();
            this.TextScenario = new System.Windows.Forms.TextBox();
            this.TextVictoryValue = new System.Windows.Forms.TextBox();
            this.LabelVictoryValue = new System.Windows.Forms.Label();
            this.LabelPopulationCap = new System.Windows.Forms.Label();
            this.ComboPopulationCap = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ButtonDev
            // 
            this.ButtonDev.Location = new System.Drawing.Point(627, 24);
            this.ButtonDev.Name = "ButtonDev";
            this.ButtonDev.Size = new System.Drawing.Size(251, 47);
            this.ButtonDev.TabIndex = 0;
            this.ButtonDev.Text = "button1";
            this.ButtonDev.UseVisualStyleBackColor = true;
            this.ButtonDev.Click += new System.EventHandler(this.ButtonDev_Click);
            // 
            // TextExe
            // 
            this.TextExe.Location = new System.Drawing.Point(47, 37);
            this.TextExe.Name = "TextExe";
            this.TextExe.ReadOnly = true;
            this.TextExe.Size = new System.Drawing.Size(368, 23);
            this.TextExe.TabIndex = 1;
            // 
            // ButtonSetExe
            // 
            this.ButtonSetExe.Location = new System.Drawing.Point(421, 37);
            this.ButtonSetExe.Name = "ButtonSetExe";
            this.ButtonSetExe.Size = new System.Drawing.Size(90, 23);
            this.ButtonSetExe.TabIndex = 2;
            this.ButtonSetExe.Text = "Set AoE Exe";
            this.ButtonSetExe.UseVisualStyleBackColor = true;
            // 
            // ButtonSetAiFolder
            // 
            this.ButtonSetAiFolder.Location = new System.Drawing.Point(421, 66);
            this.ButtonSetAiFolder.Name = "ButtonSetAiFolder";
            this.ButtonSetAiFolder.Size = new System.Drawing.Size(90, 23);
            this.ButtonSetAiFolder.TabIndex = 4;
            this.ButtonSetAiFolder.Text = "Set Ai Folder";
            this.ButtonSetAiFolder.UseVisualStyleBackColor = true;
            // 
            // TextAiFolder
            // 
            this.TextAiFolder.Location = new System.Drawing.Point(47, 66);
            this.TextAiFolder.Name = "TextAiFolder";
            this.TextAiFolder.ReadOnly = true;
            this.TextAiFolder.Size = new System.Drawing.Size(368, 23);
            this.TextAiFolder.TabIndex = 3;
            // 
            // ComboGameType
            // 
            this.ComboGameType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboGameType.FormattingEnabled = true;
            this.ComboGameType.Location = new System.Drawing.Point(157, 125);
            this.ComboGameType.Name = "ComboGameType";
            this.ComboGameType.Size = new System.Drawing.Size(246, 23);
            this.ComboGameType.TabIndex = 5;
            // 
            // LabelGameType
            // 
            this.LabelGameType.AutoSize = true;
            this.LabelGameType.Location = new System.Drawing.Point(47, 128);
            this.LabelGameType.Name = "LabelGameType";
            this.LabelGameType.Size = new System.Drawing.Size(65, 15);
            this.LabelGameType.TabIndex = 6;
            this.LabelGameType.Text = "Game Type";
            // 
            // LabelMapType
            // 
            this.LabelMapType.AutoSize = true;
            this.LabelMapType.Location = new System.Drawing.Point(47, 186);
            this.LabelMapType.Name = "LabelMapType";
            this.LabelMapType.Size = new System.Drawing.Size(58, 15);
            this.LabelMapType.TabIndex = 8;
            this.LabelMapType.Text = "Map Type";
            // 
            // ComboMapType
            // 
            this.ComboMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboMapType.FormattingEnabled = true;
            this.ComboMapType.Location = new System.Drawing.Point(157, 183);
            this.ComboMapType.Name = "ComboMapType";
            this.ComboMapType.Size = new System.Drawing.Size(246, 23);
            this.ComboMapType.TabIndex = 7;
            // 
            // LabelDifficulty
            // 
            this.LabelDifficulty.AutoSize = true;
            this.LabelDifficulty.Location = new System.Drawing.Point(47, 244);
            this.LabelDifficulty.Name = "LabelDifficulty";
            this.LabelDifficulty.Size = new System.Drawing.Size(55, 15);
            this.LabelDifficulty.TabIndex = 10;
            this.LabelDifficulty.Text = "Difficulty";
            // 
            // ComboDifficulty
            // 
            this.ComboDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboDifficulty.FormattingEnabled = true;
            this.ComboDifficulty.Location = new System.Drawing.Point(157, 241);
            this.ComboDifficulty.Name = "ComboDifficulty";
            this.ComboDifficulty.Size = new System.Drawing.Size(246, 23);
            this.ComboDifficulty.TabIndex = 9;
            // 
            // LabelStartingResources
            // 
            this.LabelStartingResources.AutoSize = true;
            this.LabelStartingResources.Location = new System.Drawing.Point(47, 273);
            this.LabelStartingResources.Name = "LabelStartingResources";
            this.LabelStartingResources.Size = new System.Drawing.Size(104, 15);
            this.LabelStartingResources.TabIndex = 12;
            this.LabelStartingResources.Text = "Starting Resources";
            // 
            // ComboStartingResources
            // 
            this.ComboStartingResources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboStartingResources.FormattingEnabled = true;
            this.ComboStartingResources.Location = new System.Drawing.Point(157, 270);
            this.ComboStartingResources.Name = "ComboStartingResources";
            this.ComboStartingResources.Size = new System.Drawing.Size(246, 23);
            this.ComboStartingResources.TabIndex = 11;
            // 
            // LabelRevealMap
            // 
            this.LabelRevealMap.AutoSize = true;
            this.LabelRevealMap.Location = new System.Drawing.Point(47, 331);
            this.LabelRevealMap.Name = "LabelRevealMap";
            this.LabelRevealMap.Size = new System.Drawing.Size(68, 15);
            this.LabelRevealMap.TabIndex = 14;
            this.LabelRevealMap.Text = "Reveal Map";
            // 
            // ComboRevealMap
            // 
            this.ComboRevealMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboRevealMap.FormattingEnabled = true;
            this.ComboRevealMap.Location = new System.Drawing.Point(157, 328);
            this.ComboRevealMap.Name = "ComboRevealMap";
            this.ComboRevealMap.Size = new System.Drawing.Size(246, 23);
            this.ComboRevealMap.TabIndex = 13;
            // 
            // LabelStartingAge
            // 
            this.LabelStartingAge.AutoSize = true;
            this.LabelStartingAge.Location = new System.Drawing.Point(47, 360);
            this.LabelStartingAge.Name = "LabelStartingAge";
            this.LabelStartingAge.Size = new System.Drawing.Size(72, 15);
            this.LabelStartingAge.TabIndex = 16;
            this.LabelStartingAge.Text = "Starting Age";
            // 
            // ComboStartingAge
            // 
            this.ComboStartingAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboStartingAge.FormattingEnabled = true;
            this.ComboStartingAge.Location = new System.Drawing.Point(157, 357);
            this.ComboStartingAge.Name = "ComboStartingAge";
            this.ComboStartingAge.Size = new System.Drawing.Size(246, 23);
            this.ComboStartingAge.TabIndex = 15;
            // 
            // LabelVictoryType
            // 
            this.LabelVictoryType.AutoSize = true;
            this.LabelVictoryType.Location = new System.Drawing.Point(47, 389);
            this.LabelVictoryType.Name = "LabelVictoryType";
            this.LabelVictoryType.Size = new System.Drawing.Size(71, 15);
            this.LabelVictoryType.TabIndex = 18;
            this.LabelVictoryType.Text = "Victory Type";
            // 
            // ComboVictoryType
            // 
            this.ComboVictoryType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboVictoryType.FormattingEnabled = true;
            this.ComboVictoryType.Location = new System.Drawing.Point(157, 386);
            this.ComboVictoryType.Name = "ComboVictoryType";
            this.ComboVictoryType.Size = new System.Drawing.Size(246, 23);
            this.ComboVictoryType.TabIndex = 17;
            // 
            // CheckTeamsTogether
            // 
            this.CheckTeamsTogether.AutoSize = true;
            this.CheckTeamsTogether.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckTeamsTogether.Location = new System.Drawing.Point(47, 444);
            this.CheckTeamsTogether.Name = "CheckTeamsTogether";
            this.CheckTeamsTogether.Size = new System.Drawing.Size(108, 19);
            this.CheckTeamsTogether.TabIndex = 19;
            this.CheckTeamsTogether.Text = "Teams Together";
            this.CheckTeamsTogether.UseVisualStyleBackColor = true;
            // 
            // CheckLockTeams
            // 
            this.CheckLockTeams.AutoSize = true;
            this.CheckLockTeams.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckLockTeams.Location = new System.Drawing.Point(161, 444);
            this.CheckLockTeams.Name = "CheckLockTeams";
            this.CheckLockTeams.Size = new System.Drawing.Size(87, 19);
            this.CheckLockTeams.TabIndex = 20;
            this.CheckLockTeams.Text = "Lock Teams";
            this.CheckLockTeams.UseVisualStyleBackColor = true;
            // 
            // CheckAllTech
            // 
            this.CheckAllTech.AutoSize = true;
            this.CheckAllTech.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckAllTech.Location = new System.Drawing.Point(254, 444);
            this.CheckAllTech.Name = "CheckAllTech";
            this.CheckAllTech.Size = new System.Drawing.Size(67, 19);
            this.CheckAllTech.TabIndex = 21;
            this.CheckAllTech.Text = "All Tech";
            this.CheckAllTech.UseVisualStyleBackColor = true;
            // 
            // CheckRecorded
            // 
            this.CheckRecorded.AutoSize = true;
            this.CheckRecorded.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CheckRecorded.Location = new System.Drawing.Point(327, 444);
            this.CheckRecorded.Name = "CheckRecorded";
            this.CheckRecorded.Size = new System.Drawing.Size(76, 19);
            this.CheckRecorded.TabIndex = 22;
            this.CheckRecorded.Text = "Recorded";
            this.CheckRecorded.UseVisualStyleBackColor = true;
            // 
            // LabelMapSize
            // 
            this.LabelMapSize.AutoSize = true;
            this.LabelMapSize.Location = new System.Drawing.Point(47, 215);
            this.LabelMapSize.Name = "LabelMapSize";
            this.LabelMapSize.Size = new System.Drawing.Size(54, 15);
            this.LabelMapSize.TabIndex = 24;
            this.LabelMapSize.Text = "Map Size";
            // 
            // ComboMapSize
            // 
            this.ComboMapSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboMapSize.FormattingEnabled = true;
            this.ComboMapSize.Location = new System.Drawing.Point(157, 212);
            this.ComboMapSize.Name = "ComboMapSize";
            this.ComboMapSize.Size = new System.Drawing.Size(246, 23);
            this.ComboMapSize.TabIndex = 23;
            // 
            // LabelScenario
            // 
            this.LabelScenario.AutoSize = true;
            this.LabelScenario.Location = new System.Drawing.Point(47, 157);
            this.LabelScenario.Name = "LabelScenario";
            this.LabelScenario.Size = new System.Drawing.Size(87, 15);
            this.LabelScenario.TabIndex = 25;
            this.LabelScenario.Text = "Scenario Name";
            // 
            // TextScenario
            // 
            this.TextScenario.Location = new System.Drawing.Point(157, 154);
            this.TextScenario.Name = "TextScenario";
            this.TextScenario.Size = new System.Drawing.Size(246, 23);
            this.TextScenario.TabIndex = 26;
            // 
            // TextVictoryValue
            // 
            this.TextVictoryValue.Location = new System.Drawing.Point(157, 415);
            this.TextVictoryValue.Name = "TextVictoryValue";
            this.TextVictoryValue.Size = new System.Drawing.Size(246, 23);
            this.TextVictoryValue.TabIndex = 28;
            // 
            // LabelVictoryValue
            // 
            this.LabelVictoryValue.AutoSize = true;
            this.LabelVictoryValue.Location = new System.Drawing.Point(47, 418);
            this.LabelVictoryValue.Name = "LabelVictoryValue";
            this.LabelVictoryValue.Size = new System.Drawing.Size(75, 15);
            this.LabelVictoryValue.TabIndex = 27;
            this.LabelVictoryValue.Text = "Victory Value";
            // 
            // LabelPopulationCap
            // 
            this.LabelPopulationCap.AutoSize = true;
            this.LabelPopulationCap.Location = new System.Drawing.Point(47, 302);
            this.LabelPopulationCap.Name = "LabelPopulationCap";
            this.LabelPopulationCap.Size = new System.Drawing.Size(89, 15);
            this.LabelPopulationCap.TabIndex = 30;
            this.LabelPopulationCap.Text = "Population Cap";
            // 
            // ComboPopulationCap
            // 
            this.ComboPopulationCap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPopulationCap.FormattingEnabled = true;
            this.ComboPopulationCap.Location = new System.Drawing.Point(157, 299);
            this.ComboPopulationCap.Name = "ComboPopulationCap";
            this.ComboPopulationCap.Size = new System.Drawing.Size(246, 23);
            this.ComboPopulationCap.TabIndex = 29;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 586);
            this.Controls.Add(this.LabelPopulationCap);
            this.Controls.Add(this.ComboPopulationCap);
            this.Controls.Add(this.TextVictoryValue);
            this.Controls.Add(this.LabelVictoryValue);
            this.Controls.Add(this.TextScenario);
            this.Controls.Add(this.LabelScenario);
            this.Controls.Add(this.LabelMapSize);
            this.Controls.Add(this.ComboMapSize);
            this.Controls.Add(this.CheckRecorded);
            this.Controls.Add(this.CheckAllTech);
            this.Controls.Add(this.CheckLockTeams);
            this.Controls.Add(this.CheckTeamsTogether);
            this.Controls.Add(this.LabelVictoryType);
            this.Controls.Add(this.ComboVictoryType);
            this.Controls.Add(this.LabelStartingAge);
            this.Controls.Add(this.ComboStartingAge);
            this.Controls.Add(this.LabelRevealMap);
            this.Controls.Add(this.ComboRevealMap);
            this.Controls.Add(this.LabelStartingResources);
            this.Controls.Add(this.ComboStartingResources);
            this.Controls.Add(this.LabelDifficulty);
            this.Controls.Add(this.ComboDifficulty);
            this.Controls.Add(this.LabelMapType);
            this.Controls.Add(this.ComboMapType);
            this.Controls.Add(this.LabelGameType);
            this.Controls.Add(this.ComboGameType);
            this.Controls.Add(this.ButtonSetAiFolder);
            this.Controls.Add(this.TextAiFolder);
            this.Controls.Add(this.ButtonSetExe);
            this.Controls.Add(this.TextExe);
            this.Controls.Add(this.ButtonDev);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonDev;
        private System.Windows.Forms.TextBox TextExe;
        private System.Windows.Forms.Button ButtonSetExe;
        private System.Windows.Forms.Button ButtonSetAiFolder;
        private System.Windows.Forms.TextBox TextAiFolder;
        private System.Windows.Forms.ComboBox ComboGameType;
        private System.Windows.Forms.Label LabelGameType;
        private System.Windows.Forms.Label LabelMapType;
        private System.Windows.Forms.ComboBox ComboMapType;
        private System.Windows.Forms.Label LabelDifficulty;
        private System.Windows.Forms.ComboBox ComboDifficulty;
        private System.Windows.Forms.Label LabelStartingResources;
        private System.Windows.Forms.ComboBox ComboStartingResources;
        private System.Windows.Forms.Label LabelRevealMap;
        private System.Windows.Forms.ComboBox ComboRevealMap;
        private System.Windows.Forms.Label LabelStartingAge;
        private System.Windows.Forms.ComboBox ComboStartingAge;
        private System.Windows.Forms.Label LabelVictoryType;
        private System.Windows.Forms.ComboBox ComboVictoryType;
        private System.Windows.Forms.CheckBox CheckTeamsTogether;
        private System.Windows.Forms.CheckBox CheckLockTeams;
        private System.Windows.Forms.CheckBox CheckAllTech;
        private System.Windows.Forms.CheckBox CheckRecorded;
        private System.Windows.Forms.Label LabelMapSize;
        private System.Windows.Forms.ComboBox ComboMapSize;
        private System.Windows.Forms.Label LabelScenario;
        private System.Windows.Forms.TextBox TextScenario;
        private System.Windows.Forms.TextBox TextVictoryValue;
        private System.Windows.Forms.Label LabelVictoryValue;
        private System.Windows.Forms.Label LabelPopulationCap;
        private System.Windows.Forms.ComboBox ComboPopulationCap;
    }
}

