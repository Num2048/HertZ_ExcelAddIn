﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HertZ_ExcelAddIn
{
    public partial class BalanceAndJournalSettingForm : Form
    {
        
        public BalanceAndJournalSettingForm()
        {
            InitializeComponent();
        }

        private void BalanceAndJournalSettingForm_Load(object sender, EventArgs e)
        {
            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点BalanceAndJournal中读取配置名SubjectCodeButton1为的值，该值为布尔值。默认为true
            SubjectCodeButton1.Checked = clsConfig.ReadConfig<bool>("BalanceAndJournal", "SubjectCodeButton1", true);
            
            //从父节点BalanceAndJournal中读取配置名SubjectCodeButton2为的值，该值为布尔值。默认为false
            SubjectCodeButton2.Checked = clsConfig.ReadConfig<bool>("BalanceAndJournal", "SubjectCodeButton2", false);
            //从父节点BalanceAndJournal中读取配置名SubjectCodeSign为的值，该值为字符串。默认为"."
            SubjectCodeSign.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeSign", ".");

            //从父节点BalanceAndJournal中读取配置名SubjectCodeButton3为的值，该值为布尔值。默认为false
            SubjectCodeButton3.Checked = clsConfig.ReadConfig<bool>("BalanceAndJournal", "SubjectCodeButton3", false);
            //从父节点BalanceAndJournal中读取配置名SubjectCodeLength1为的值，该值为字符串。默认为"4"
            SubjectCodeLength1.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeLength1", "4");
            //从父节点BalanceAndJournal中读取配置名SubjectCodeLength2为的值，该值为字符串。默认为"2"
            SubjectCodeLength2.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeLength2", "2");
            //从父节点BalanceAndJournal中读取配置名SubjectCodeLength3为的值，该值为字符串。默认为"2"
            SubjectCodeLength3.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeLength3", "2");
            //从父节点BalanceAndJournal中读取配置名SubjectCodeLength4为的值，该值为字符串。默认为"2"
            SubjectCodeLength4.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeLength4", "2");
            //从父节点BalanceAndJournal中读取配置名SubjectCodeLength5为的值，该值为字符串。默认为"2"
            SubjectCodeLength5.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeLength5", "2");
            //从父节点BalanceAndJournal中读取配置名SubjectCodeLength6为的值，该值为字符串。默认为"2"
            SubjectCodeLength6.Text = clsConfig.ReadConfig<string>("BalanceAndJournal", "SubjectCodeLength6", "2");
        }

        private void ChangeState(object sender, EventArgs e)
        {
            
            foreach (var btn in SubjectCodeGroupBox.Controls.OfType<RadioButton>().ToList())
            {
                if (btn.Name != (sender as Control).Name)
                {
                    btn.Checked = false;
                }
            }
            //if (SubjectCodeButton2.Checked == false)
            //{
            //    SubjectCodeSign.ReadOnly = true;
            //}
            //if (SubjectCodeButton3.Checked == false)
            //{
            //    SubjectCodeLength1.ReadOnly = true;
            //    SubjectCodeLength2.ReadOnly = true;
            //    SubjectCodeLength3.ReadOnly = true;
            //    SubjectCodeLength4.ReadOnly = true;
            //    SubjectCodeLength5.ReadOnly = true;
            //    SubjectCodeLength6.ReadOnly = true;
            //}
        }

        private void ConfirmBtn_Click(object sender, EventArgs e)
        {
            bool IsValid = true;
            //检查输入字节的长度
            if (SubjectCodeSign.Text.Length != 1 || SubjectCodeLength1.Text.Length != 1 || SubjectCodeLength2.Text.Length != 1 || SubjectCodeLength3.Text.Length != 1 || SubjectCodeLength4.Text.Length != 1 || SubjectCodeLength5.Text.Length != 1 || SubjectCodeLength6.Text.Length != 1)
            {
                MessageBox.Show("输入字节长度有误，请检查并重新输入");
                IsValid = false;
            }

            //检查输入编码是否为数字
            try
            {
                long n = long.Parse(SubjectCodeLength1.Text);
                n = long.Parse(SubjectCodeLength2.Text);
                n = long.Parse(SubjectCodeLength3.Text);
                n = long.Parse(SubjectCodeLength4.Text);
                n = long.Parse(SubjectCodeLength5.Text);
                n = long.Parse(SubjectCodeLength6.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("科目编码长度应为数字，请检查并重新输入");
                IsValid = false;
            }

            //如果输入信息无误，保存并关闭窗体
            if (IsValid)
            {
            //向我的文档写入配置文件
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //写入父节点BalanceAndJournal中配置名SubjectCodeButton1的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeButton1", SubjectCodeButton1.Checked.ToString());

            //写入父节点BalanceAndJournal中配置名SubjectCodeButton2的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeButton2", SubjectCodeButton2.Checked.ToString());
            //写入父节点BalanceAndJournal中配置名SubjectCodeSign的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeSign", SubjectCodeSign.Text);

            //写入父节点BalanceAndJournal中配置名SubjectCodeButton3的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeButton3", SubjectCodeButton3.Checked.ToString());
            //写入父节点BalanceAndJournal中配置名SubjectCodeLength1的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeLength1", SubjectCodeLength1.Text);
            //写入父节点BalanceAndJournal中配置名SubjectCodeLength2的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeLength2", SubjectCodeLength2.Text);
            //写入父节点BalanceAndJournal中配置名SubjectCodeLength3的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeLength3", SubjectCodeLength3.Text);
            //写入父节点BalanceAndJournal中配置名SubjectCodeLength4的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeLength4", SubjectCodeLength4.Text);
            //写入父节点BalanceAndJournal中配置名SubjectCodeLength5的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeLength5", SubjectCodeLength5.Text);
            //写入父节点BalanceAndJournal中配置名SubjectCodeLength6的配置项
            clsConfig.WriteConfig("BalanceAndJournal", "SubjectCodeLength6", SubjectCodeLength6.Text);

            //关闭窗体
            this.Close();
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            //关闭窗体
            this.Close();
        }
    }
}
