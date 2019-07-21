﻿namespace HertZ_ExcelAddIn
{
    partial class BalanceAndJournalSettingForm
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
            this.SubjectCodeButton1 = new System.Windows.Forms.RadioButton();
            this.SubjectCodeButton2 = new System.Windows.Forms.RadioButton();
            this.SubjectCodeGroupBox = new System.Windows.Forms.GroupBox();
            this.SubjectCodeLength6 = new System.Windows.Forms.TextBox();
            this.SubjectCodeLength5 = new System.Windows.Forms.TextBox();
            this.SubjectCodeLength4 = new System.Windows.Forms.TextBox();
            this.SubjectCodeLength3 = new System.Windows.Forms.TextBox();
            this.SubjectCodeLength2 = new System.Windows.Forms.TextBox();
            this.SubjectCodeLength1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SubjectCodeSign = new System.Windows.Forms.TextBox();
            this.SubjectCodeButton3 = new System.Windows.Forms.RadioButton();
            this.SheetHeaderGroupBox = new System.Windows.Forms.GroupBox();
            this.JournalBtn = new System.Windows.Forms.Button();
            this.BalanceBtn = new System.Windows.Forms.Button();
            this.ConfirmBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SubjectCodeGroupBox.SuspendLayout();
            this.SheetHeaderGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SubjectCodeButton1
            // 
            this.SubjectCodeButton1.AutoSize = true;
            this.SubjectCodeButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SubjectCodeButton1.Location = new System.Drawing.Point(6, 50);
            this.SubjectCodeButton1.Name = "SubjectCodeButton1";
            this.SubjectCodeButton1.Size = new System.Drawing.Size(189, 35);
            this.SubjectCodeButton1.TabIndex = 0;
            this.SubjectCodeButton1.TabStop = true;
            this.SubjectCodeButton1.Text = "自动识别编码";
            this.SubjectCodeButton1.UseVisualStyleBackColor = true;
            this.SubjectCodeButton1.Click += new System.EventHandler(this.ChangeState);
            // 
            // SubjectCodeButton2
            // 
            this.SubjectCodeButton2.AutoSize = true;
            this.SubjectCodeButton2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SubjectCodeButton2.Location = new System.Drawing.Point(6, 100);
            this.SubjectCodeButton2.Name = "SubjectCodeButton2";
            this.SubjectCodeButton2.Size = new System.Drawing.Size(262, 35);
            this.SubjectCodeButton2.TabIndex = 1;
            this.SubjectCodeButton2.Text = "使用分隔符       拆分";
            this.SubjectCodeButton2.UseVisualStyleBackColor = true;
            this.SubjectCodeButton2.Click += new System.EventHandler(this.ChangeState);
            // 
            // SubjectCodeGroupBox
            // 
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeLength6);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeLength5);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeLength4);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeLength3);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeLength2);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeLength1);
            this.SubjectCodeGroupBox.Controls.Add(this.label2);
            this.SubjectCodeGroupBox.Controls.Add(this.label1);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeSign);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeButton3);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeButton1);
            this.SubjectCodeGroupBox.Controls.Add(this.SubjectCodeButton2);
            this.SubjectCodeGroupBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SubjectCodeGroupBox.Location = new System.Drawing.Point(150, 50);
            this.SubjectCodeGroupBox.Name = "SubjectCodeGroupBox";
            this.SubjectCodeGroupBox.Size = new System.Drawing.Size(500, 300);
            this.SubjectCodeGroupBox.TabIndex = 2;
            this.SubjectCodeGroupBox.TabStop = false;
            this.SubjectCodeGroupBox.Text = "科目编码设置";
            // 
            // SubjectCodeLength6
            // 
            this.SubjectCodeLength6.Location = new System.Drawing.Point(436, 244);
            this.SubjectCodeLength6.Name = "SubjectCodeLength6";
            this.SubjectCodeLength6.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeLength6.TabIndex = 12;
            // 
            // SubjectCodeLength5
            // 
            this.SubjectCodeLength5.Location = new System.Drawing.Point(291, 244);
            this.SubjectCodeLength5.Name = "SubjectCodeLength5";
            this.SubjectCodeLength5.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeLength5.TabIndex = 11;
            // 
            // SubjectCodeLength4
            // 
            this.SubjectCodeLength4.Location = new System.Drawing.Point(146, 244);
            this.SubjectCodeLength4.Name = "SubjectCodeLength4";
            this.SubjectCodeLength4.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeLength4.TabIndex = 10;
            // 
            // SubjectCodeLength3
            // 
            this.SubjectCodeLength3.Location = new System.Drawing.Point(436, 195);
            this.SubjectCodeLength3.Name = "SubjectCodeLength3";
            this.SubjectCodeLength3.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeLength3.TabIndex = 9;
            // 
            // SubjectCodeLength2
            // 
            this.SubjectCodeLength2.Location = new System.Drawing.Point(291, 195);
            this.SubjectCodeLength2.Name = "SubjectCodeLength2";
            this.SubjectCodeLength2.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeLength2.TabIndex = 8;
            // 
            // SubjectCodeLength1
            // 
            this.SubjectCodeLength1.Location = new System.Drawing.Point(146, 195);
            this.SubjectCodeLength1.Name = "SubjectCodeLength1";
            this.SubjectCodeLength1.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeLength1.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 247);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(400, 31);
            this.label2.TabIndex = 6;
            this.label2.Text = "四级科目       五级科目       六级科目";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 200);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(400, 31);
            this.label1.TabIndex = 5;
            this.label1.Text = "一级科目       二级科目       三级科目";
            // 
            // SubjectCodeSign
            // 
            this.SubjectCodeSign.Location = new System.Drawing.Point(165, 98);
            this.SubjectCodeSign.Name = "SubjectCodeSign";
            this.SubjectCodeSign.Size = new System.Drawing.Size(38, 39);
            this.SubjectCodeSign.TabIndex = 4;
            // 
            // SubjectCodeButton3
            // 
            this.SubjectCodeButton3.AutoSize = true;
            this.SubjectCodeButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SubjectCodeButton3.Location = new System.Drawing.Point(6, 150);
            this.SubjectCodeButton3.Name = "SubjectCodeButton3";
            this.SubjectCodeButton3.Size = new System.Drawing.Size(285, 35);
            this.SubjectCodeButton3.TabIndex = 2;
            this.SubjectCodeButton3.TabStop = true;
            this.SubjectCodeButton3.Text = "按照以下编码长度拆分";
            this.SubjectCodeButton3.UseVisualStyleBackColor = true;
            this.SubjectCodeButton3.Click += new System.EventHandler(this.ChangeState);
            // 
            // SheetHeaderGroupBox
            // 
            this.SheetHeaderGroupBox.Controls.Add(this.JournalBtn);
            this.SheetHeaderGroupBox.Controls.Add(this.BalanceBtn);
            this.SheetHeaderGroupBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SheetHeaderGroupBox.Location = new System.Drawing.Point(150, 397);
            this.SheetHeaderGroupBox.Name = "SheetHeaderGroupBox";
            this.SheetHeaderGroupBox.Size = new System.Drawing.Size(500, 144);
            this.SheetHeaderGroupBox.TabIndex = 3;
            this.SheetHeaderGroupBox.TabStop = false;
            this.SheetHeaderGroupBox.Text = "常用列名修改";
            // 
            // JournalBtn
            // 
            this.JournalBtn.Location = new System.Drawing.Point(300, 60);
            this.JournalBtn.Name = "JournalBtn";
            this.JournalBtn.Size = new System.Drawing.Size(150, 50);
            this.JournalBtn.TabIndex = 1;
            this.JournalBtn.Text = "序时账";
            this.JournalBtn.UseVisualStyleBackColor = true;
            // 
            // BalanceBtn
            // 
            this.BalanceBtn.Location = new System.Drawing.Point(50, 60);
            this.BalanceBtn.Name = "BalanceBtn";
            this.BalanceBtn.Size = new System.Drawing.Size(150, 50);
            this.BalanceBtn.TabIndex = 0;
            this.BalanceBtn.Text = "余额表";
            this.BalanceBtn.UseVisualStyleBackColor = true;
            // 
            // ConfirmBtn
            // 
            this.ConfirmBtn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ConfirmBtn.Location = new System.Drawing.Point(150, 621);
            this.ConfirmBtn.Name = "ConfirmBtn";
            this.ConfirmBtn.Size = new System.Drawing.Size(150, 50);
            this.ConfirmBtn.TabIndex = 4;
            this.ConfirmBtn.Text = "保存";
            this.ConfirmBtn.UseVisualStyleBackColor = true;
            this.ConfirmBtn.Click += new System.EventHandler(this.ConfirmBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CancelBtn.Location = new System.Drawing.Point(500, 621);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(150, 50);
            this.CancelBtn.TabIndex = 5;
            this.CancelBtn.Text = "取消";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // BalanceAndJournalSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 729);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.ConfirmBtn);
            this.Controls.Add(this.SheetHeaderGroupBox);
            this.Controls.Add(this.SubjectCodeGroupBox);
            this.Name = "BalanceAndJournalSettingForm";
            this.Text = "账表加工设置";
            this.Load += new System.EventHandler(this.BalanceAndJournalSettingForm_Load);
            this.SubjectCodeGroupBox.ResumeLayout(false);
            this.SubjectCodeGroupBox.PerformLayout();
            this.SheetHeaderGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton SubjectCodeButton1;
        private System.Windows.Forms.RadioButton SubjectCodeButton2;
        private System.Windows.Forms.GroupBox SubjectCodeGroupBox;
        private System.Windows.Forms.RadioButton SubjectCodeButton3;
        private System.Windows.Forms.TextBox SubjectCodeLength6;
        private System.Windows.Forms.TextBox SubjectCodeLength5;
        private System.Windows.Forms.TextBox SubjectCodeLength4;
        private System.Windows.Forms.TextBox SubjectCodeLength3;
        private System.Windows.Forms.TextBox SubjectCodeLength2;
        private System.Windows.Forms.TextBox SubjectCodeLength1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SubjectCodeSign;
        private System.Windows.Forms.GroupBox SheetHeaderGroupBox;
        private System.Windows.Forms.Button JournalBtn;
        private System.Windows.Forms.Button BalanceBtn;
        private System.Windows.Forms.Button ConfirmBtn;
        private System.Windows.Forms.Button CancelBtn;
    }
}