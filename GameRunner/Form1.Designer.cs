
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
            this.LabelPlayer1 = new System.Windows.Forms.Label();
            this.ComboPlayer1Name = new System.Windows.Forms.ComboBox();
            this.ComboPlayer1Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer1Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer1Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer2Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer2Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer2Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer2Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer2 = new System.Windows.Forms.Label();
            this.ComboPlayer3Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer3Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer3Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer3Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer3 = new System.Windows.Forms.Label();
            this.ComboPlayer4Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer4Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer4Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer4Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer4 = new System.Windows.Forms.Label();
            this.ComboPlayer5Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer5Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer5Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer5Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer5 = new System.Windows.Forms.Label();
            this.ComboPlayer6Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer6Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer6Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer6Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer6 = new System.Windows.Forms.Label();
            this.ComboPlayer7Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer7Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer7Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer7Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer7 = new System.Windows.Forms.Label();
            this.ComboPlayer8Team = new System.Windows.Forms.ComboBox();
            this.ComboPlayer8Color = new System.Windows.Forms.ComboBox();
            this.ComboPlayer8Civ = new System.Windows.Forms.ComboBox();
            this.ComboPlayer8Name = new System.Windows.Forms.ComboBox();
            this.LabelPlayer8 = new System.Windows.Forms.Label();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonDev
            // 
            this.ButtonDev.Location = new System.Drawing.Point(592, 455);
            this.ButtonDev.Name = "ButtonDev";
            this.ButtonDev.Size = new System.Drawing.Size(251, 47);
            this.ButtonDev.TabIndex = 0;
            this.ButtonDev.Text = "button1";
            this.ButtonDev.UseVisualStyleBackColor = true;
            this.ButtonDev.Click += new System.EventHandler(this.ButtonDev_Click);
            // 
            // TextExe
            // 
            this.TextExe.Location = new System.Drawing.Point(143, 43);
            this.TextExe.Name = "TextExe";
            this.TextExe.ReadOnly = true;
            this.TextExe.Size = new System.Drawing.Size(923, 23);
            this.TextExe.TabIndex = 1;
            // 
            // ButtonSetExe
            // 
            this.ButtonSetExe.Location = new System.Drawing.Point(47, 42);
            this.ButtonSetExe.Name = "ButtonSetExe";
            this.ButtonSetExe.Size = new System.Drawing.Size(90, 23);
            this.ButtonSetExe.TabIndex = 2;
            this.ButtonSetExe.Text = "Set AoE Exe";
            this.ButtonSetExe.UseVisualStyleBackColor = true;
            this.ButtonSetExe.Click += new System.EventHandler(this.ButtonSetExe_Click);
            // 
            // ButtonSetAiFolder
            // 
            this.ButtonSetAiFolder.Location = new System.Drawing.Point(47, 71);
            this.ButtonSetAiFolder.Name = "ButtonSetAiFolder";
            this.ButtonSetAiFolder.Size = new System.Drawing.Size(90, 23);
            this.ButtonSetAiFolder.TabIndex = 4;
            this.ButtonSetAiFolder.Text = "Set Ai Folder";
            this.ButtonSetAiFolder.UseVisualStyleBackColor = true;
            this.ButtonSetAiFolder.Click += new System.EventHandler(this.ButtonSetAiFolder_Click);
            // 
            // TextAiFolder
            // 
            this.TextAiFolder.Location = new System.Drawing.Point(143, 72);
            this.TextAiFolder.Name = "TextAiFolder";
            this.TextAiFolder.ReadOnly = true;
            this.TextAiFolder.Size = new System.Drawing.Size(923, 23);
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
            this.ComboGameType.SelectedIndexChanged += new System.EventHandler(this.ComboGameType_SelectedIndexChanged);
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
            this.ComboVictoryType.SelectedIndexChanged += new System.EventHandler(this.ComboVictoryType_SelectedIndexChanged);
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
            // LabelPlayer1
            // 
            this.LabelPlayer1.AutoSize = true;
            this.LabelPlayer1.Location = new System.Drawing.Point(459, 128);
            this.LabelPlayer1.Name = "LabelPlayer1";
            this.LabelPlayer1.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer1.TabIndex = 31;
            this.LabelPlayer1.Text = "1";
            // 
            // ComboPlayer1Name
            // 
            this.ComboPlayer1Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer1Name.FormattingEnabled = true;
            this.ComboPlayer1Name.Location = new System.Drawing.Point(478, 125);
            this.ComboPlayer1Name.Name = "ComboPlayer1Name";
            this.ComboPlayer1Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer1Name.TabIndex = 32;
            // 
            // ComboPlayer1Civ
            // 
            this.ComboPlayer1Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer1Civ.FormattingEnabled = true;
            this.ComboPlayer1Civ.Location = new System.Drawing.Point(633, 125);
            this.ComboPlayer1Civ.Name = "ComboPlayer1Civ";
            this.ComboPlayer1Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer1Civ.TabIndex = 33;
            // 
            // ComboPlayer1Color
            // 
            this.ComboPlayer1Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer1Color.FormattingEnabled = true;
            this.ComboPlayer1Color.Location = new System.Drawing.Point(862, 125);
            this.ComboPlayer1Color.Name = "ComboPlayer1Color";
            this.ComboPlayer1Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer1Color.TabIndex = 34;
            // 
            // ComboPlayer1Team
            // 
            this.ComboPlayer1Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer1Team.FormattingEnabled = true;
            this.ComboPlayer1Team.Location = new System.Drawing.Point(954, 125);
            this.ComboPlayer1Team.Name = "ComboPlayer1Team";
            this.ComboPlayer1Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer1Team.TabIndex = 35;
            // 
            // ComboPlayer2Team
            // 
            this.ComboPlayer2Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer2Team.FormattingEnabled = true;
            this.ComboPlayer2Team.Location = new System.Drawing.Point(954, 154);
            this.ComboPlayer2Team.Name = "ComboPlayer2Team";
            this.ComboPlayer2Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer2Team.TabIndex = 40;
            // 
            // ComboPlayer2Color
            // 
            this.ComboPlayer2Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer2Color.FormattingEnabled = true;
            this.ComboPlayer2Color.Location = new System.Drawing.Point(862, 154);
            this.ComboPlayer2Color.Name = "ComboPlayer2Color";
            this.ComboPlayer2Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer2Color.TabIndex = 39;
            // 
            // ComboPlayer2Civ
            // 
            this.ComboPlayer2Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer2Civ.FormattingEnabled = true;
            this.ComboPlayer2Civ.Location = new System.Drawing.Point(633, 154);
            this.ComboPlayer2Civ.Name = "ComboPlayer2Civ";
            this.ComboPlayer2Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer2Civ.TabIndex = 38;
            // 
            // ComboPlayer2Name
            // 
            this.ComboPlayer2Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer2Name.FormattingEnabled = true;
            this.ComboPlayer2Name.Location = new System.Drawing.Point(478, 154);
            this.ComboPlayer2Name.Name = "ComboPlayer2Name";
            this.ComboPlayer2Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer2Name.TabIndex = 37;
            // 
            // LabelPlayer2
            // 
            this.LabelPlayer2.AutoSize = true;
            this.LabelPlayer2.Location = new System.Drawing.Point(459, 157);
            this.LabelPlayer2.Name = "LabelPlayer2";
            this.LabelPlayer2.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer2.TabIndex = 36;
            this.LabelPlayer2.Text = "2";
            // 
            // ComboPlayer3Team
            // 
            this.ComboPlayer3Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer3Team.FormattingEnabled = true;
            this.ComboPlayer3Team.Location = new System.Drawing.Point(954, 183);
            this.ComboPlayer3Team.Name = "ComboPlayer3Team";
            this.ComboPlayer3Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer3Team.TabIndex = 45;
            // 
            // ComboPlayer3Color
            // 
            this.ComboPlayer3Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer3Color.FormattingEnabled = true;
            this.ComboPlayer3Color.Location = new System.Drawing.Point(862, 183);
            this.ComboPlayer3Color.Name = "ComboPlayer3Color";
            this.ComboPlayer3Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer3Color.TabIndex = 44;
            // 
            // ComboPlayer3Civ
            // 
            this.ComboPlayer3Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer3Civ.FormattingEnabled = true;
            this.ComboPlayer3Civ.Location = new System.Drawing.Point(633, 183);
            this.ComboPlayer3Civ.Name = "ComboPlayer3Civ";
            this.ComboPlayer3Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer3Civ.TabIndex = 43;
            // 
            // ComboPlayer3Name
            // 
            this.ComboPlayer3Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer3Name.FormattingEnabled = true;
            this.ComboPlayer3Name.Location = new System.Drawing.Point(478, 183);
            this.ComboPlayer3Name.Name = "ComboPlayer3Name";
            this.ComboPlayer3Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer3Name.TabIndex = 42;
            // 
            // LabelPlayer3
            // 
            this.LabelPlayer3.AutoSize = true;
            this.LabelPlayer3.Location = new System.Drawing.Point(459, 186);
            this.LabelPlayer3.Name = "LabelPlayer3";
            this.LabelPlayer3.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer3.TabIndex = 41;
            this.LabelPlayer3.Text = "3";
            // 
            // ComboPlayer4Team
            // 
            this.ComboPlayer4Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer4Team.FormattingEnabled = true;
            this.ComboPlayer4Team.Location = new System.Drawing.Point(954, 212);
            this.ComboPlayer4Team.Name = "ComboPlayer4Team";
            this.ComboPlayer4Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer4Team.TabIndex = 50;
            // 
            // ComboPlayer4Color
            // 
            this.ComboPlayer4Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer4Color.FormattingEnabled = true;
            this.ComboPlayer4Color.Location = new System.Drawing.Point(862, 212);
            this.ComboPlayer4Color.Name = "ComboPlayer4Color";
            this.ComboPlayer4Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer4Color.TabIndex = 49;
            // 
            // ComboPlayer4Civ
            // 
            this.ComboPlayer4Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer4Civ.FormattingEnabled = true;
            this.ComboPlayer4Civ.Location = new System.Drawing.Point(633, 212);
            this.ComboPlayer4Civ.Name = "ComboPlayer4Civ";
            this.ComboPlayer4Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer4Civ.TabIndex = 48;
            // 
            // ComboPlayer4Name
            // 
            this.ComboPlayer4Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer4Name.FormattingEnabled = true;
            this.ComboPlayer4Name.Location = new System.Drawing.Point(478, 212);
            this.ComboPlayer4Name.Name = "ComboPlayer4Name";
            this.ComboPlayer4Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer4Name.TabIndex = 47;
            // 
            // LabelPlayer4
            // 
            this.LabelPlayer4.AutoSize = true;
            this.LabelPlayer4.Location = new System.Drawing.Point(459, 215);
            this.LabelPlayer4.Name = "LabelPlayer4";
            this.LabelPlayer4.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer4.TabIndex = 46;
            this.LabelPlayer4.Text = "4";
            // 
            // ComboPlayer5Team
            // 
            this.ComboPlayer5Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer5Team.FormattingEnabled = true;
            this.ComboPlayer5Team.Location = new System.Drawing.Point(954, 241);
            this.ComboPlayer5Team.Name = "ComboPlayer5Team";
            this.ComboPlayer5Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer5Team.TabIndex = 55;
            // 
            // ComboPlayer5Color
            // 
            this.ComboPlayer5Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer5Color.FormattingEnabled = true;
            this.ComboPlayer5Color.Location = new System.Drawing.Point(862, 241);
            this.ComboPlayer5Color.Name = "ComboPlayer5Color";
            this.ComboPlayer5Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer5Color.TabIndex = 54;
            // 
            // ComboPlayer5Civ
            // 
            this.ComboPlayer5Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer5Civ.FormattingEnabled = true;
            this.ComboPlayer5Civ.Location = new System.Drawing.Point(633, 241);
            this.ComboPlayer5Civ.Name = "ComboPlayer5Civ";
            this.ComboPlayer5Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer5Civ.TabIndex = 53;
            // 
            // ComboPlayer5Name
            // 
            this.ComboPlayer5Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer5Name.FormattingEnabled = true;
            this.ComboPlayer5Name.Location = new System.Drawing.Point(478, 241);
            this.ComboPlayer5Name.Name = "ComboPlayer5Name";
            this.ComboPlayer5Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer5Name.TabIndex = 52;
            // 
            // LabelPlayer5
            // 
            this.LabelPlayer5.AutoSize = true;
            this.LabelPlayer5.Location = new System.Drawing.Point(459, 244);
            this.LabelPlayer5.Name = "LabelPlayer5";
            this.LabelPlayer5.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer5.TabIndex = 51;
            this.LabelPlayer5.Text = "5";
            // 
            // ComboPlayer6Team
            // 
            this.ComboPlayer6Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer6Team.FormattingEnabled = true;
            this.ComboPlayer6Team.Location = new System.Drawing.Point(954, 270);
            this.ComboPlayer6Team.Name = "ComboPlayer6Team";
            this.ComboPlayer6Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer6Team.TabIndex = 60;
            // 
            // ComboPlayer6Color
            // 
            this.ComboPlayer6Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer6Color.FormattingEnabled = true;
            this.ComboPlayer6Color.Location = new System.Drawing.Point(862, 270);
            this.ComboPlayer6Color.Name = "ComboPlayer6Color";
            this.ComboPlayer6Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer6Color.TabIndex = 59;
            // 
            // ComboPlayer6Civ
            // 
            this.ComboPlayer6Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer6Civ.FormattingEnabled = true;
            this.ComboPlayer6Civ.Location = new System.Drawing.Point(633, 270);
            this.ComboPlayer6Civ.Name = "ComboPlayer6Civ";
            this.ComboPlayer6Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer6Civ.TabIndex = 58;
            // 
            // ComboPlayer6Name
            // 
            this.ComboPlayer6Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer6Name.FormattingEnabled = true;
            this.ComboPlayer6Name.Location = new System.Drawing.Point(478, 270);
            this.ComboPlayer6Name.Name = "ComboPlayer6Name";
            this.ComboPlayer6Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer6Name.TabIndex = 57;
            // 
            // LabelPlayer6
            // 
            this.LabelPlayer6.AutoSize = true;
            this.LabelPlayer6.Location = new System.Drawing.Point(459, 273);
            this.LabelPlayer6.Name = "LabelPlayer6";
            this.LabelPlayer6.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer6.TabIndex = 56;
            this.LabelPlayer6.Text = "6";
            // 
            // ComboPlayer7Team
            // 
            this.ComboPlayer7Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer7Team.FormattingEnabled = true;
            this.ComboPlayer7Team.Location = new System.Drawing.Point(954, 299);
            this.ComboPlayer7Team.Name = "ComboPlayer7Team";
            this.ComboPlayer7Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer7Team.TabIndex = 65;
            // 
            // ComboPlayer7Color
            // 
            this.ComboPlayer7Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer7Color.FormattingEnabled = true;
            this.ComboPlayer7Color.Location = new System.Drawing.Point(862, 299);
            this.ComboPlayer7Color.Name = "ComboPlayer7Color";
            this.ComboPlayer7Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer7Color.TabIndex = 64;
            // 
            // ComboPlayer7Civ
            // 
            this.ComboPlayer7Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer7Civ.FormattingEnabled = true;
            this.ComboPlayer7Civ.Location = new System.Drawing.Point(633, 299);
            this.ComboPlayer7Civ.Name = "ComboPlayer7Civ";
            this.ComboPlayer7Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer7Civ.TabIndex = 63;
            // 
            // ComboPlayer7Name
            // 
            this.ComboPlayer7Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer7Name.FormattingEnabled = true;
            this.ComboPlayer7Name.Location = new System.Drawing.Point(478, 299);
            this.ComboPlayer7Name.Name = "ComboPlayer7Name";
            this.ComboPlayer7Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer7Name.TabIndex = 62;
            // 
            // LabelPlayer7
            // 
            this.LabelPlayer7.AutoSize = true;
            this.LabelPlayer7.Location = new System.Drawing.Point(459, 302);
            this.LabelPlayer7.Name = "LabelPlayer7";
            this.LabelPlayer7.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer7.TabIndex = 61;
            this.LabelPlayer7.Text = "7";
            // 
            // ComboPlayer8Team
            // 
            this.ComboPlayer8Team.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer8Team.FormattingEnabled = true;
            this.ComboPlayer8Team.Location = new System.Drawing.Point(954, 328);
            this.ComboPlayer8Team.Name = "ComboPlayer8Team";
            this.ComboPlayer8Team.Size = new System.Drawing.Size(112, 23);
            this.ComboPlayer8Team.TabIndex = 70;
            // 
            // ComboPlayer8Color
            // 
            this.ComboPlayer8Color.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer8Color.FormattingEnabled = true;
            this.ComboPlayer8Color.Location = new System.Drawing.Point(862, 328);
            this.ComboPlayer8Color.Name = "ComboPlayer8Color";
            this.ComboPlayer8Color.Size = new System.Drawing.Size(86, 23);
            this.ComboPlayer8Color.TabIndex = 69;
            // 
            // ComboPlayer8Civ
            // 
            this.ComboPlayer8Civ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer8Civ.FormattingEnabled = true;
            this.ComboPlayer8Civ.Location = new System.Drawing.Point(633, 328);
            this.ComboPlayer8Civ.Name = "ComboPlayer8Civ";
            this.ComboPlayer8Civ.Size = new System.Drawing.Size(223, 23);
            this.ComboPlayer8Civ.TabIndex = 68;
            // 
            // ComboPlayer8Name
            // 
            this.ComboPlayer8Name.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboPlayer8Name.FormattingEnabled = true;
            this.ComboPlayer8Name.Location = new System.Drawing.Point(478, 328);
            this.ComboPlayer8Name.Name = "ComboPlayer8Name";
            this.ComboPlayer8Name.Size = new System.Drawing.Size(149, 23);
            this.ComboPlayer8Name.TabIndex = 67;
            // 
            // LabelPlayer8
            // 
            this.LabelPlayer8.AutoSize = true;
            this.LabelPlayer8.Location = new System.Drawing.Point(459, 331);
            this.LabelPlayer8.Name = "LabelPlayer8";
            this.LabelPlayer8.Size = new System.Drawing.Size(13, 15);
            this.LabelPlayer8.TabIndex = 66;
            this.LabelPlayer8.Text = "8";
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(478, 373);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(251, 47);
            this.ButtonStart.TabIndex = 71;
            this.ButtonStart.Text = "Start Game";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1078, 586);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.ComboPlayer8Team);
            this.Controls.Add(this.ComboPlayer8Color);
            this.Controls.Add(this.ComboPlayer8Civ);
            this.Controls.Add(this.ComboPlayer8Name);
            this.Controls.Add(this.LabelPlayer8);
            this.Controls.Add(this.ComboPlayer7Team);
            this.Controls.Add(this.ComboPlayer7Color);
            this.Controls.Add(this.ComboPlayer7Civ);
            this.Controls.Add(this.ComboPlayer7Name);
            this.Controls.Add(this.LabelPlayer7);
            this.Controls.Add(this.ComboPlayer6Team);
            this.Controls.Add(this.ComboPlayer6Color);
            this.Controls.Add(this.ComboPlayer6Civ);
            this.Controls.Add(this.ComboPlayer6Name);
            this.Controls.Add(this.LabelPlayer6);
            this.Controls.Add(this.ComboPlayer5Team);
            this.Controls.Add(this.ComboPlayer5Color);
            this.Controls.Add(this.ComboPlayer5Civ);
            this.Controls.Add(this.ComboPlayer5Name);
            this.Controls.Add(this.LabelPlayer5);
            this.Controls.Add(this.ComboPlayer4Team);
            this.Controls.Add(this.ComboPlayer4Color);
            this.Controls.Add(this.ComboPlayer4Civ);
            this.Controls.Add(this.ComboPlayer4Name);
            this.Controls.Add(this.LabelPlayer4);
            this.Controls.Add(this.ComboPlayer3Team);
            this.Controls.Add(this.ComboPlayer3Color);
            this.Controls.Add(this.ComboPlayer3Civ);
            this.Controls.Add(this.ComboPlayer3Name);
            this.Controls.Add(this.LabelPlayer3);
            this.Controls.Add(this.ComboPlayer2Team);
            this.Controls.Add(this.ComboPlayer2Color);
            this.Controls.Add(this.ComboPlayer2Civ);
            this.Controls.Add(this.ComboPlayer2Name);
            this.Controls.Add(this.LabelPlayer2);
            this.Controls.Add(this.ComboPlayer1Team);
            this.Controls.Add(this.ComboPlayer1Color);
            this.Controls.Add(this.ComboPlayer1Civ);
            this.Controls.Add(this.ComboPlayer1Name);
            this.Controls.Add(this.LabelPlayer1);
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
        private System.Windows.Forms.Label LabelPlayer1;
        private System.Windows.Forms.ComboBox ComboPlayer1Name;
        private System.Windows.Forms.ComboBox ComboPlayer1Civ;
        private System.Windows.Forms.ComboBox ComboPlayer1Color;
        private System.Windows.Forms.ComboBox ComboPlayer1Team;
        private System.Windows.Forms.ComboBox ComboPlayer2Team;
        private System.Windows.Forms.ComboBox ComboPlayer2Color;
        private System.Windows.Forms.ComboBox ComboPlayer2Civ;
        private System.Windows.Forms.ComboBox ComboPlayer2Name;
        private System.Windows.Forms.Label LabelPlayer2;
        private System.Windows.Forms.ComboBox ComboPlayer3Team;
        private System.Windows.Forms.ComboBox ComboPlayer3Color;
        private System.Windows.Forms.ComboBox ComboPlayer3Civ;
        private System.Windows.Forms.ComboBox ComboPlayer3Name;
        private System.Windows.Forms.Label LabelPlayer3;
        private System.Windows.Forms.ComboBox ComboPlayer4Team;
        private System.Windows.Forms.ComboBox ComboPlayer4Color;
        private System.Windows.Forms.ComboBox ComboPlayer4Civ;
        private System.Windows.Forms.ComboBox ComboPlayer4Name;
        private System.Windows.Forms.Label LabelPlayer4;
        private System.Windows.Forms.ComboBox ComboPlayer5Team;
        private System.Windows.Forms.ComboBox ComboPlayer5Color;
        private System.Windows.Forms.ComboBox ComboPlayer5Civ;
        private System.Windows.Forms.ComboBox ComboPlayer5Name;
        private System.Windows.Forms.Label LabelPlayer5;
        private System.Windows.Forms.ComboBox ComboPlayer6Team;
        private System.Windows.Forms.ComboBox ComboPlayer6Color;
        private System.Windows.Forms.ComboBox ComboPlayer6Civ;
        private System.Windows.Forms.ComboBox ComboPlayer6Name;
        private System.Windows.Forms.Label LabelPlayer6;
        private System.Windows.Forms.ComboBox ComboPlayer7Team;
        private System.Windows.Forms.ComboBox ComboPlayer7Color;
        private System.Windows.Forms.ComboBox ComboPlayer7Civ;
        private System.Windows.Forms.ComboBox ComboPlayer7Name;
        private System.Windows.Forms.Label LabelPlayer7;
        private System.Windows.Forms.ComboBox ComboPlayer8Team;
        private System.Windows.Forms.ComboBox ComboPlayer8Color;
        private System.Windows.Forms.ComboBox ComboPlayer8Civ;
        private System.Windows.Forms.ComboBox ComboPlayer8Name;
        private System.Windows.Forms.Label LabelPlayer8;
        private System.Windows.Forms.Button ButtonStart;
    }
}

