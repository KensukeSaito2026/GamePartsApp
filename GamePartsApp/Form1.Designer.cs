namespace GamePartsApp
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
            components = new System.ComponentModel.Container();
            gachaPictureBox = new PictureBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            skill1Panel = new Panel();
            skill1RarityLabel = new Label();
            skill1DescLabel = new Label();
            skill1Label = new Label();
            skill2Panel = new Panel();
            skill2RarityLabel = new Label();
            skill2DescLabel = new Label();
            skill2Label = new Label();
            skill3Panel = new Panel();
            skill3RarityLabel = new Label();
            skill3Label = new Label();
            skill3DescLabel = new Label();
            rerollButton = new Button();
            tabPage2 = new TabPage();
            gachaResultPictureBox = new PictureBox();
            gachaButton = new Button();
            tabPage3 = new TabPage();
            rightBottom2PictureBox = new PictureBox();
            rightTop2PictureBox = new PictureBox();
            rightBottom1PictureBox = new PictureBox();
            rightTop1PictureBox = new PictureBox();
            rightCenterPictureBox = new PictureBox();
            centerBottom2PictureBox = new PictureBox();
            centerTop2PictureBox = new PictureBox();
            centerBottom1PictureBox = new PictureBox();
            centerTop1PictureBox = new PictureBox();
            centerCenterPictureBox = new PictureBox();
            slotStopButton = new Button();
            slotStartButton = new Button();
            leftBottom2PictureBox = new PictureBox();
            leftTop2PictureBox = new PictureBox();
            leftBottom1PictureBox = new PictureBox();
            leftTop1PictureBox = new PictureBox();
            leftCenterPictureBox = new PictureBox();
            gachaTimer = new System.Windows.Forms.Timer(components);
            walkAnimationTimer = new System.Windows.Forms.Timer(components);
            leftTimer = new System.Windows.Forms.Timer(components);
            colorDialog1 = new ColorDialog();
            centerTimer = new System.Windows.Forms.Timer(components);
            rightTimer = new System.Windows.Forms.Timer(components);
            colorDialog2 = new ColorDialog();
            printDialog1 = new PrintDialog();
            ((System.ComponentModel.ISupportInitialize)gachaPictureBox).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            skill1Panel.SuspendLayout();
            skill2Panel.SuspendLayout();
            skill3Panel.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gachaResultPictureBox).BeginInit();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rightBottom2PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightTop2PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightBottom1PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightTop1PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightCenterPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)centerBottom2PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)centerTop2PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)centerBottom1PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)centerTop1PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)centerCenterPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)leftBottom2PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)leftTop2PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)leftBottom1PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)leftTop1PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)leftCenterPictureBox).BeginInit();
            SuspendLayout();
            // 
            // gachaPictureBox
            // 
            gachaPictureBox.Location = new Point(69, 67);
            gachaPictureBox.Name = "gachaPictureBox";
            gachaPictureBox.Size = new Size(170, 137);
            gachaPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            gachaPictureBox.TabIndex = 1;
            gachaPictureBox.TabStop = false;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Location = new Point(24, 26);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(666, 384);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(skill1Panel);
            tabPage1.Controls.Add(skill2Panel);
            tabPage1.Controls.Add(skill3Panel);
            tabPage1.Controls.Add(rerollButton);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(658, 351);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "スキル選択";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // skill1Panel
            // 
            skill1Panel.Controls.Add(skill1RarityLabel);
            skill1Panel.Controls.Add(skill1DescLabel);
            skill1Panel.Controls.Add(skill1Label);
            skill1Panel.Location = new Point(37, 64);
            skill1Panel.Name = "skill1Panel";
            skill1Panel.Size = new Size(162, 162);
            skill1Panel.TabIndex = 10;
            skill1Panel.Click += skill1Panel_Click;
            skill1Panel.MouseEnter += skill1Panel_MouseEnter;
            skill1Panel.MouseLeave += skill1Panel_MouseLeave;
            // 
            // skill1RarityLabel
            // 
            skill1RarityLabel.AutoSize = true;
            skill1RarityLabel.Location = new Point(47, 105);
            skill1RarityLabel.Name = "skill1RarityLabel";
            skill1RarityLabel.Size = new Size(50, 20);
            skill1RarityLabel.TabIndex = 5;
            skill1RarityLabel.Text = "label5";
            skill1RarityLabel.MouseEnter += skill1Panel_MouseEnter;
            skill1RarityLabel.MouseLeave += skill1Panel_MouseLeave;
            // 
            // skill1DescLabel
            // 
            skill1DescLabel.AutoSize = true;
            skill1DescLabel.Location = new Point(47, 63);
            skill1DescLabel.Name = "skill1DescLabel";
            skill1DescLabel.Size = new Size(50, 20);
            skill1DescLabel.TabIndex = 4;
            skill1DescLabel.Text = "label4";
            skill1DescLabel.MouseEnter += skill1Panel_MouseEnter;
            skill1DescLabel.MouseLeave += skill1Panel_MouseLeave;
            // 
            // skill1Label
            // 
            skill1Label.AutoSize = true;
            skill1Label.Location = new Point(47, 17);
            skill1Label.Name = "skill1Label";
            skill1Label.Size = new Size(50, 20);
            skill1Label.TabIndex = 1;
            skill1Label.Text = "label1";
            skill1Label.MouseEnter += skill1Panel_MouseEnter;
            skill1Label.MouseLeave += skill1Panel_MouseLeave;
            // 
            // skill2Panel
            // 
            skill2Panel.Controls.Add(skill2RarityLabel);
            skill2Panel.Controls.Add(skill2DescLabel);
            skill2Panel.Controls.Add(skill2Label);
            skill2Panel.Location = new Point(245, 64);
            skill2Panel.Name = "skill2Panel";
            skill2Panel.Size = new Size(156, 162);
            skill2Panel.TabIndex = 11;
            skill2Panel.Click += skill2Panel_Click;
            skill2Panel.MouseEnter += skill2Panel_MouseEnter;
            skill2Panel.MouseLeave += skill2Panel_MouseLeave;
            // 
            // skill2RarityLabel
            // 
            skill2RarityLabel.AutoSize = true;
            skill2RarityLabel.Location = new Point(36, 105);
            skill2RarityLabel.Name = "skill2RarityLabel";
            skill2RarityLabel.Size = new Size(50, 20);
            skill2RarityLabel.TabIndex = 7;
            skill2RarityLabel.Text = "label7";
            skill2RarityLabel.MouseEnter += skill2Panel_MouseEnter;
            skill2RarityLabel.MouseLeave += skill2Panel_MouseLeave;
            // 
            // skill2DescLabel
            // 
            skill2DescLabel.AutoSize = true;
            skill2DescLabel.Location = new Point(36, 63);
            skill2DescLabel.Name = "skill2DescLabel";
            skill2DescLabel.Size = new Size(50, 20);
            skill2DescLabel.TabIndex = 6;
            skill2DescLabel.Text = "label6";
            skill2DescLabel.MouseEnter += skill2Panel_MouseEnter;
            skill2DescLabel.MouseLeave += skill2Panel_MouseLeave;
            // 
            // skill2Label
            // 
            skill2Label.AutoSize = true;
            skill2Label.Location = new Point(36, 17);
            skill2Label.Name = "skill2Label";
            skill2Label.Size = new Size(50, 20);
            skill2Label.TabIndex = 2;
            skill2Label.Text = "label2";
            skill2Label.MouseEnter += skill2Panel_MouseEnter;
            skill2Label.MouseLeave += skill2Panel_MouseLeave;
            // 
            // skill3Panel
            // 
            skill3Panel.Controls.Add(skill3RarityLabel);
            skill3Panel.Controls.Add(skill3Label);
            skill3Panel.Controls.Add(skill3DescLabel);
            skill3Panel.Location = new Point(455, 64);
            skill3Panel.Name = "skill3Panel";
            skill3Panel.Size = new Size(162, 162);
            skill3Panel.TabIndex = 12;
            skill3Panel.Click += skill3Panel_Click;
            skill3Panel.MouseEnter += skill3Panel_MouseEnter;
            skill3Panel.MouseLeave += skill3Panel_MouseLeave;
            // 
            // skill3RarityLabel
            // 
            skill3RarityLabel.AutoSize = true;
            skill3RarityLabel.Location = new Point(42, 105);
            skill3RarityLabel.Name = "skill3RarityLabel";
            skill3RarityLabel.Size = new Size(50, 20);
            skill3RarityLabel.TabIndex = 9;
            skill3RarityLabel.Text = "label9";
            skill3RarityLabel.MouseEnter += skill3Panel_MouseEnter;
            skill3RarityLabel.MouseLeave += skill3Panel_MouseLeave;
            // 
            // skill3Label
            // 
            skill3Label.AutoSize = true;
            skill3Label.Location = new Point(42, 17);
            skill3Label.Name = "skill3Label";
            skill3Label.Size = new Size(50, 20);
            skill3Label.TabIndex = 3;
            skill3Label.Text = "label3";
            skill3Label.MouseEnter += skill3Panel_MouseEnter;
            skill3Label.MouseLeave += skill3Panel_MouseLeave;
            // 
            // skill3DescLabel
            // 
            skill3DescLabel.AutoSize = true;
            skill3DescLabel.Location = new Point(42, 63);
            skill3DescLabel.Name = "skill3DescLabel";
            skill3DescLabel.Size = new Size(50, 20);
            skill3DescLabel.TabIndex = 8;
            skill3DescLabel.Text = "label8";
            skill3DescLabel.MouseEnter += skill3Panel_MouseEnter;
            skill3DescLabel.MouseLeave += skill3Panel_MouseLeave;
            // 
            // rerollButton
            // 
            rerollButton.Location = new Point(233, 254);
            rerollButton.Name = "rerollButton";
            rerollButton.Size = new Size(152, 29);
            rerollButton.TabIndex = 0;
            rerollButton.Text = "rerollbutton";
            rerollButton.UseVisualStyleBackColor = true;
            rerollButton.Click += rerollButton_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(gachaResultPictureBox);
            tabPage2.Controls.Add(gachaPictureBox);
            tabPage2.Controls.Add(gachaButton);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(658, 351);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "ガチャガチャ";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // gachaResultPictureBox
            // 
            gachaResultPictureBox.Location = new Point(277, 98);
            gachaResultPictureBox.Name = "gachaResultPictureBox";
            gachaResultPictureBox.Size = new Size(111, 106);
            gachaResultPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            gachaResultPictureBox.TabIndex = 2;
            gachaResultPictureBox.TabStop = false;
            // 
            // gachaButton
            // 
            gachaButton.Location = new Point(104, 243);
            gachaButton.Name = "gachaButton";
            gachaButton.Size = new Size(94, 29);
            gachaButton.TabIndex = 0;
            gachaButton.Text = "button1";
            gachaButton.UseVisualStyleBackColor = true;
            gachaButton.Click += gachaButton_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(rightBottom2PictureBox);
            tabPage3.Controls.Add(rightTop2PictureBox);
            tabPage3.Controls.Add(rightBottom1PictureBox);
            tabPage3.Controls.Add(rightTop1PictureBox);
            tabPage3.Controls.Add(rightCenterPictureBox);
            tabPage3.Controls.Add(centerBottom2PictureBox);
            tabPage3.Controls.Add(centerTop2PictureBox);
            tabPage3.Controls.Add(centerBottom1PictureBox);
            tabPage3.Controls.Add(centerTop1PictureBox);
            tabPage3.Controls.Add(centerCenterPictureBox);
            tabPage3.Controls.Add(slotStopButton);
            tabPage3.Controls.Add(slotStartButton);
            tabPage3.Controls.Add(leftBottom2PictureBox);
            tabPage3.Controls.Add(leftTop2PictureBox);
            tabPage3.Controls.Add(leftBottom1PictureBox);
            tabPage3.Controls.Add(leftTop1PictureBox);
            tabPage3.Controls.Add(leftCenterPictureBox);
            tabPage3.Location = new Point(4, 29);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(658, 351);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "スロット";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // rightBottom2PictureBox
            // 
            rightBottom2PictureBox.Location = new Point(486, 272);
            rightBottom2PictureBox.Name = "rightBottom2PictureBox";
            rightBottom2PictureBox.Size = new Size(43, 30);
            rightBottom2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightBottom2PictureBox.TabIndex = 18;
            rightBottom2PictureBox.TabStop = false;
            // 
            // rightTop2PictureBox
            // 
            rightTop2PictureBox.Location = new Point(484, 49);
            rightTop2PictureBox.Name = "rightTop2PictureBox";
            rightTop2PictureBox.Size = new Size(45, 31);
            rightTop2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightTop2PictureBox.TabIndex = 17;
            rightTop2PictureBox.TabStop = false;
            // 
            // rightBottom1PictureBox
            // 
            rightBottom1PictureBox.Location = new Point(472, 221);
            rightBottom1PictureBox.Name = "rightBottom1PictureBox";
            rightBottom1PictureBox.Size = new Size(70, 45);
            rightBottom1PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightBottom1PictureBox.TabIndex = 16;
            rightBottom1PictureBox.TabStop = false;
            // 
            // rightTop1PictureBox
            // 
            rightTop1PictureBox.Location = new Point(472, 86);
            rightTop1PictureBox.Name = "rightTop1PictureBox";
            rightTop1PictureBox.Size = new Size(70, 51);
            rightTop1PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightTop1PictureBox.TabIndex = 15;
            rightTop1PictureBox.TabStop = false;
            // 
            // rightCenterPictureBox
            // 
            rightCenterPictureBox.Location = new Point(443, 143);
            rightCenterPictureBox.Name = "rightCenterPictureBox";
            rightCenterPictureBox.Size = new Size(134, 72);
            rightCenterPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rightCenterPictureBox.TabIndex = 14;
            rightCenterPictureBox.TabStop = false;
            // 
            // centerBottom2PictureBox
            // 
            centerBottom2PictureBox.Location = new Point(305, 272);
            centerBottom2PictureBox.Name = "centerBottom2PictureBox";
            centerBottom2PictureBox.Size = new Size(43, 30);
            centerBottom2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            centerBottom2PictureBox.TabIndex = 13;
            centerBottom2PictureBox.TabStop = false;
            // 
            // centerTop2PictureBox
            // 
            centerTop2PictureBox.Location = new Point(303, 49);
            centerTop2PictureBox.Name = "centerTop2PictureBox";
            centerTop2PictureBox.Size = new Size(45, 31);
            centerTop2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            centerTop2PictureBox.TabIndex = 12;
            centerTop2PictureBox.TabStop = false;
            // 
            // centerBottom1PictureBox
            // 
            centerBottom1PictureBox.Location = new Point(291, 221);
            centerBottom1PictureBox.Name = "centerBottom1PictureBox";
            centerBottom1PictureBox.Size = new Size(70, 45);
            centerBottom1PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            centerBottom1PictureBox.TabIndex = 11;
            centerBottom1PictureBox.TabStop = false;
            // 
            // centerTop1PictureBox
            // 
            centerTop1PictureBox.Location = new Point(291, 86);
            centerTop1PictureBox.Name = "centerTop1PictureBox";
            centerTop1PictureBox.Size = new Size(70, 51);
            centerTop1PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            centerTop1PictureBox.TabIndex = 10;
            centerTop1PictureBox.TabStop = false;
            // 
            // centerCenterPictureBox
            // 
            centerCenterPictureBox.Location = new Point(260, 143);
            centerCenterPictureBox.Name = "centerCenterPictureBox";
            centerCenterPictureBox.Size = new Size(134, 72);
            centerCenterPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            centerCenterPictureBox.TabIndex = 9;
            centerCenterPictureBox.TabStop = false;
            // 
            // slotStopButton
            // 
            slotStopButton.Location = new Point(370, 301);
            slotStopButton.Name = "slotStopButton";
            slotStopButton.Size = new Size(94, 29);
            slotStopButton.TabIndex = 8;
            slotStopButton.Text = "button1";
            slotStopButton.UseVisualStyleBackColor = true;
            slotStopButton.Click += slotStopButton_Click;
            // 
            // slotStartButton
            // 
            slotStartButton.Location = new Point(192, 301);
            slotStartButton.Name = "slotStartButton";
            slotStartButton.Size = new Size(94, 29);
            slotStartButton.TabIndex = 5;
            slotStartButton.Text = "button1";
            slotStartButton.UseVisualStyleBackColor = true;
            slotStartButton.Click += slotStartButton_Click;
            // 
            // leftBottom2PictureBox
            // 
            leftBottom2PictureBox.Location = new Point(138, 272);
            leftBottom2PictureBox.Name = "leftBottom2PictureBox";
            leftBottom2PictureBox.Size = new Size(43, 30);
            leftBottom2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftBottom2PictureBox.TabIndex = 4;
            leftBottom2PictureBox.TabStop = false;
            // 
            // leftTop2PictureBox
            // 
            leftTop2PictureBox.Location = new Point(138, 49);
            leftTop2PictureBox.Name = "leftTop2PictureBox";
            leftTop2PictureBox.Size = new Size(45, 31);
            leftTop2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftTop2PictureBox.TabIndex = 3;
            leftTop2PictureBox.TabStop = false;
            // 
            // leftBottom1PictureBox
            // 
            leftBottom1PictureBox.Location = new Point(126, 221);
            leftBottom1PictureBox.Name = "leftBottom1PictureBox";
            leftBottom1PictureBox.Size = new Size(70, 45);
            leftBottom1PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftBottom1PictureBox.TabIndex = 2;
            leftBottom1PictureBox.TabStop = false;
            // 
            // leftTop1PictureBox
            // 
            leftTop1PictureBox.Location = new Point(126, 86);
            leftTop1PictureBox.Name = "leftTop1PictureBox";
            leftTop1PictureBox.Size = new Size(70, 51);
            leftTop1PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftTop1PictureBox.TabIndex = 1;
            leftTop1PictureBox.TabStop = false;
            // 
            // leftCenterPictureBox
            // 
            leftCenterPictureBox.Location = new Point(89, 143);
            leftCenterPictureBox.Name = "leftCenterPictureBox";
            leftCenterPictureBox.Size = new Size(134, 72);
            leftCenterPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            leftCenterPictureBox.TabIndex = 0;
            leftCenterPictureBox.TabStop = false;
            // 
            // gachaTimer
            // 
            gachaTimer.Interval = 300;
            gachaTimer.Tick += gachaTimer_Tick;
            // 
            // walkAnimationTimer
            // 
            walkAnimationTimer.Interval = 200;
            walkAnimationTimer.Tick += walkAnimationTimer_Tick;
            // 
            // leftTimer
            // 
            leftTimer.Interval = 500;
            leftTimer.Tick += leftTimer_Tick;
            // 
            // centerTimer
            // 
            centerTimer.Tick += centerTimer_Tick;
            // 
            // rightTimer
            // 
            rightTimer.Tick += rightTimer_Tick;
            // 
            // printDialog1
            // 
            printDialog1.UseEXDialog = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(713, 439);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)gachaPictureBox).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            skill1Panel.ResumeLayout(false);
            skill1Panel.PerformLayout();
            skill2Panel.ResumeLayout(false);
            skill2Panel.PerformLayout();
            skill3Panel.ResumeLayout(false);
            skill3Panel.PerformLayout();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gachaResultPictureBox).EndInit();
            tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rightBottom2PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)rightTop2PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)rightBottom1PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)rightTop1PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)rightCenterPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)centerBottom2PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)centerTop2PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)centerBottom1PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)centerTop1PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)centerCenterPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)leftBottom2PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)leftTop2PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)leftBottom1PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)leftTop1PictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)leftCenterPictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label skill3RarityLabel;
        private Label skill3DescLabel;
        private Label skill2RarityLabel;
        private Label skill2DescLabel;
        private Label skill1RarityLabel;
        private Label skill1DescLabel;
        private Label skill3Label;
        private Label skill2Label;
        private Label skill1Label;
        private Button rerollButton;
        private TabPage tabPage3;
        private Panel skill3Panel;
        private Panel skill1Panel;
        private Panel skill2Panel;
        private PictureBox gachaResultPictureBox;
        private PictureBox gachaPictureBox;
        private Button gachaButton;
        private System.Windows.Forms.Timer gachaTimer;
        private System.Windows.Forms.Timer walkAnimationTimer;
        private PictureBox leftBottom2PictureBox;
        private PictureBox leftTop2PictureBox;
        private PictureBox leftBottom1PictureBox;
        private PictureBox leftTop1PictureBox;
        private PictureBox leftCenterPictureBox;
        private Button slotStartButton;
        private System.Windows.Forms.Timer leftTimer;
        private ColorDialog colorDialog1;
        private Button slotStopButton;
        private PictureBox rightBottom2PictureBox;
        private PictureBox rightTop2PictureBox;
        private PictureBox rightBottom1PictureBox;
        private PictureBox rightTop1PictureBox;
        private PictureBox rightCenterPictureBox;
        private PictureBox centerBottom2PictureBox;
        private PictureBox centerTop2PictureBox;
        private PictureBox centerBottom1PictureBox;
        private PictureBox centerTop1PictureBox;
        private PictureBox centerCenterPictureBox;
        private System.Windows.Forms.Timer centerTimer;
        private System.Windows.Forms.Timer rightTimer;
        private ColorDialog colorDialog2;
        private PrintDialog printDialog1;
    }
}
