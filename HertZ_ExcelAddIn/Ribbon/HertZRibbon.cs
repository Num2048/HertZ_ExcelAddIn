﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HertZ_ExcelAddIn
{
    public partial class HertZRibbon
    {
        private Excel.Application ExcelApp;
        private Excel.Worksheet WST;
        
        //引用函数模块
        private readonly FunCtion FunC = new FunCtion();


        //判断浮点数是否等于0的参数 PRECISION
        public const double p = 0.0001d;

        private void HertZRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            TableProcessing.Visible = clsConfig.ReadConfig<bool>("GlobalSetting", "TableProcessingCheck", true);
            TableProcessingCheck.Checked = TableProcessing.Visible;
            //JiuQi.Visible = clsConfig.ReadConfig<bool>("GlobalSetting", "JiuQiCheck", false);
            //JiuQiCheck.Checked = JiuQi.Visible;
            Tool.Visible = clsConfig.ReadConfig<bool>("GlobalSetting", "ToolCheck", true);
            ToolCheck.Checked = Tool.Visible;
            Protect.Visible = clsConfig.ReadConfig<bool>("GlobalSetting", "ProtectCheck", true);
            ProtectCheck.Checked = Protect.Visible;
            WorkBook.Visible = clsConfig.ReadConfig<bool>("GlobalSetting", "WorkBookCheck", true);
            WorkBookCheck.Checked = WorkBook.Visible;
            WorkSheet.Visible = clsConfig.ReadConfig<bool>("GlobalSetting", "WorkSheetCheck", true);
            WorkSheetCheck.Checked = WorkSheet.Visible;
        }

        //加工余额表
        private void BalanceSheet_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                int ColumnNumber;
                List<string> ColumnName;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                object[,] NRG;

                //选中科目余额表并继续
                if (FunC.SelectSheet("余额表") == false) { return; };
                WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["余额表"];
                WST.Select();
                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();

                //规范原始数据
                if (FunC.RangeIsStandard() == false)
                {
                    MessageBox.Show("请规范数据格式，保证数据内容不超出首行和首列");
                    return;
                }

                //将表格读入数组ORG
                ORG = WST.Range[string.Format("A1:{0}{1}", FunC.CName(AllColumns), AllRows)].Value2;
                //创建目标新数组NRG
                NRG = new object[AllRows, 9];

                //将列名读入List
                List<string> OName = new List<string> { };
                for (int i = 1; i <= AllColumns; i++)
                {
                    OName.Add(FunC.TS(ORG[1, i]));
                }

                //选择[科目编码]列
                ColumnName = new List<string> { "[科目编码]", "科目编码", "科目编号", "科目号", "科目代码" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 2);
                NRG[0, 1] = ColumnName[0];
                ColumnName.Clear();

                //选择[科目名称]列
                ColumnName = new List<string> { "[科目名称]", "科目名称" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 3);
                if (ColumnNumber == 0) { return; }
                NRG[0, 2] = ColumnName[0];
                ColumnName.Clear();

                //匹配现有列名和目标列名
                DialogResult dr = DialogResult.None; //是否以方向和金额列示

                //选择期初期末余额列示方式
                dr = MessageBox.Show("期初期末余额是否按[借贷方向][金额]列示？" + Environment.NewLine + "若分别[借方余额][贷方余额]按列示，请选否", "请选择", MessageBoxButtons.YesNo);

                if (dr == DialogResult.Yes)
                {
                    //选择[方向]列,可选列
                    ColumnName = new List<string> { "[方向]", "借贷方向" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                    if (ColumnNumber != 0) { FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 4); }
                    NRG[0, 3] = ColumnName[0];
                    ColumnName.Clear();

                    //选择[期初余额]列
                    ColumnName = new List<string> { "[期初余额]", "期初余额", "期初金额", "期初数", "审定期初数" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 5);
                    if (ColumnNumber == 0) { return; }
                    NRG[0, 4] = ColumnName[0];
                    ColumnName.Clear();

                    //选择[期末余额]列
                    ColumnName = new List<string> { "[期末余额]", "期末余额", "期末金额", "期末数", "审定期末数" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 8);
                    if (ColumnNumber == 0) { return; }
                    NRG[0, 7] = ColumnName[0];
                    ColumnName.Clear();
                }
                else if (dr == DialogResult.No)
                {
                    //选择[期初借方]列，先借用NRG的第5列存放数据
                    ColumnName = new List<string> { "[期初借方]", "期初借方", "期初借方金额", "期初借方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //选择[期初贷方]列，先借用NRG的第6列存放数据
                    ColumnName = new List<string> { "[期初贷方]", "期初贷方", "期初贷方金额", "期初贷方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //赋值[方向]列,[期初余额]列
                    NRG[0, 3] = "[方向]";
                    NRG[0, 4] = "[期初余额]";
                    for (int i = 1; i < AllRows; i++)
                    {
                        //规范[期初借方]列数据
                        if (string.IsNullOrWhiteSpace(FunC.TS(NRG[i, 5])))
                        {
                            NRG[i, 5] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 5])))
                            {
                                MessageBox.Show("所选[期初借方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //规范[期初贷方]列数据
                        if (string.IsNullOrWhiteSpace(FunC.TS(NRG[i, 6])))
                        {
                            NRG[i, 6] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 6])))
                            {
                                MessageBox.Show("所选[期初贷方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //计算[方向]列
                        if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) > 0.0001d)
                        {
                            NRG[i, 3] = "借";
                            NRG[i, 4] = FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]);
                        }
                        else if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) < -0.0001d)
                        {
                            NRG[i, 3] = "贷";
                            NRG[i, 4] = FunC.TD(NRG[i, 6]) - FunC.TD(NRG[i, 5]);
                        }
                        else
                        {
                            NRG[i, 3] = "平";
                        }
                    }

                    //选择[期末借方]列，先借用NRG的第5列存放数据
                    ColumnName = new List<string> { "[期末借方]", "期末借方", "期末借方金额", "期末借方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //选择[期末贷方]列，先借用NRG的第6列存放数据
                    ColumnName = new List<string> { "[期末贷方]", "期末贷方", "期末贷方金额", "期末贷方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //赋值[期末余额]列
                    NRG[0, 7] = "[期末余额]";
                    for (int i = 1; i < AllRows; i++)
                    {
                        //规范[期末借方]列数据
                        if (string.IsNullOrWhiteSpace(FunC.TS(NRG[i, 5])))
                        {
                            NRG[i, 5] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 5])))
                            {
                                MessageBox.Show("所选[期末借方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //规范[期末贷方]列数据
                        if (string.IsNullOrWhiteSpace(FunC.TS(NRG[i, 6])))
                        {
                            NRG[i, 6] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 6])))
                            {
                                MessageBox.Show("所选[期初贷方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //计算[期末余额]列
                        if (FunC.TS(NRG[i, 3]) == "借")
                        {
                            NRG[i, 7] = Math.Round(FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]), 2);
                        }
                        else if (FunC.TS(NRG[i, 3]) == "贷")
                        {
                            NRG[i, 7] = FunC.TD(NRG[i, 6]) - FunC.TD(NRG[i, 5]);
                        }
                        else
                        {
                            if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) > 0.00001d)
                            {
                                NRG[i, 3] = "借";
                                NRG[i, 7] = FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]);
                            }
                            else if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) < -0.00001d)
                            {
                                NRG[i, 3] = "贷";
                                NRG[i, 7] = FunC.TD(NRG[i, 6]) - FunC.TD(NRG[i, 5]);
                            }
                        }
                    }

                }
                else { return; }

                //选择[本年借方]列
                ColumnName = new List<string> { "[本年借方]", "本年借方", "本年借方累计", "借方金额累计", "审定借方发生额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                if (ColumnNumber == 0) { return; }
                NRG[0, 5] = ColumnName[0];
                ColumnName.Clear();

                //选择[本年贷方]列
                ColumnName = new List<string> { "[本年贷方]", "本年贷方", "本年贷方累计", "贷方金额累计", "审定贷方发生额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                if (ColumnNumber == 0) { return; }
                NRG[0, 6] = ColumnName[0];
                ColumnName.Clear();

                //规范[科目编码]列
                //使用长度区分科目层级
                Dictionary<int, string> CodeLen = new Dictionary<int, string> { };
                for (int i = 1; i < AllRows; i++)
                {
                    try
                    {
                        CodeLen.Add(FunC.TS(NRG[i, 1]).Length, FunC.TS(NRG[i, 1]));
                    }
                    catch { }
                }

                //字典排序
                CodeLen = CodeLen.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

                //字典转list
                int[] CodeList = (from val in CodeLen select val.Key).ToArray<int>();
                CodeLen.Clear();

                //添加是否显示列
                NRG[0, 0] = "[显示]";
                for (int i = 1; i < AllRows; i++)
                {
                    if (FunC.TS(NRG[i, 1]).Length == CodeList[0])
                    {
                        NRG[i, 0] = 1;
                    }
                    else
                    {
                        NRG[i, 0] = 0;
                    }
                }

                //添加科目层级列
                NRG[0, 8] = "[科目层级]";
                for (int i = 1; i < AllRows; i++)
                {
                    for (int i1 = 1; i1 <= CodeList.Count(); i1++)
                    {
                        if (FunC.TS(NRG[i, 1]).Length == CodeList[i1 - 1])
                        {
                            NRG[i, 8] = i1;
                        }
                    }
                }

                ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

                //删除sheet中的原始数据
                WST.Range["A:" + FunC.CName(AllColumns)].Delete();

                //从我的文档读取配置
                string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

                //如果需要排序,则排序
                if (clsConfig.ReadConfig<bool>("BalanceAndJournal", "OrderCheckBox", false))
                {
                    //将编码列改为字符串格式
                    for (int i = 1; i < AllRows; i++)
                    {
                        if (NRG[i, 1] == null) { continue; }
                        NRG[i, 1] = FunC.TS(NRG[i, 1]);
                    }

                    //写入数据
                    WST.Range["A1:I" + FunC.TS(AllRows)].Value2 = NRG;

                    //清除筛选
                    //if (WST.AutoFilterMode) { WST.AutoFilterMode = false; }

                    //排序
                    //WST.Range["A1:I" + AllRows].AutoFilter(1, 1).Sort();

                    //取消筛选
                    //WST.AutoFilterMode = false;

                }
                else
                {
                    //写入数据
                    WST.Range["A1:I" + FunC.TS(AllRows)].Value2 = NRG;
                }


                //释放数组
                ORG = null;

                //调整格式
                WST.Range["A1:I1"].Interior.Color = Color.LightGray;
                //按科目层级修改颜色
                Excel.Range rg;//定义单元格区域对象
                for (int i = 2; i <= AllRows; i++)
                {
                    rg = WST.Range["A" + i + ":I" + i];
                    switch (NRG[i - 1, 8])
                    {
                        case 1:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                            rg.Interior.TintAndShade = -0.249977111117893;
                            break;
                        case 2:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                            rg.Interior.TintAndShade = -0.149998474074526;
                            break;
                        case 3:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                            rg.Interior.TintAndShade = -4.99893185216834E-02;
                            break;
                        case 4:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent1;
                            rg.Interior.TintAndShade = 0.799981688894314;
                            break;
                        case 5:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent6;
                            rg.Interior.TintAndShade = 0.799981688894314;
                            break;
                    }
                }
                //释放数组
                NRG = null;

                //设置数字格式
                WST.Range["E2:H" + AllRows].NumberFormatLocal = "#,##0.00 ";
                //ABC列靠左显示
                WST.Range["B2:C" + AllRows].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                //设置自动列宽
                WST.Columns["B:H"].EntireColumn.AutoFit();
                //筛选[显示]列
                WST.Range["A1:I" + AllRows].AutoFilter(1, 1);
                //隐藏[显示]列
                WST.Columns["A:A"].Hidden = true;

                ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
                WST.Tab.Color = Color.Red;//设置tab颜色为红色

            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }


        }

        //加工序时账
        private void JournalSheet_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                int ColumnNumber;
                List<string> ColumnName;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                object[,] NRG;

                //选中序时账并继续
                if (FunC.SelectSheet("序时账") == false) { return; };

                //检查余额表是否存在
                if (!FunC.SheetExist("余额表"))
                {
                    MessageBox.Show("请先加工余额表，并请勿修改加工完的余额表名称！");
                    return;
                }

                Excel.Worksheet WST2 = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["余额表"];

                ORG = WST2.Range["B1:C1"].Value2;
                //检查余额表是否被加工
                if (FunC.TS(ORG[1, 1]) != "[科目编码]" || FunC.TS(ORG[1, 2]) != "[科目名称]")
                {
                    MessageBox.Show("请先加工余额表！");
                    return;
                }
                ORG = null;

                WST2.Select();
                //求余额表的行数
                int AllRows2 = FunC.AllRows();

                //选中序时账并继续
                WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["序时账"];
                WST.Select();
                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();

                //规范原始数据
                if (FunC.RangeIsStandard() == false)
                {
                    MessageBox.Show("请规范数据格式，保证数据内容不超出首行和首列");
                    return;
                }

                //将表格读入数组ORG
                ORG = WST.Range["A1:" + FunC.CName(AllColumns) + AllRows.ToString()].Value2;
                //创建目标新数组NRG
                NRG = new object[AllRows, 19];

                //将列名读入List
                List<string> OName = new List<string> { };
                for (int i = 1; i <= AllColumns; i++)
                {
                    if (ORG[1, i] == null) { break; }
                    OName.Add(FunC.TS(ORG[1, i]));
                }

                //选择[日期]列
                ColumnName = new List<string> { "[日期]", "日期", "记账日期", "凭证日期" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 3);
                NRG[0, 2] = ColumnName[0];
                ColumnName.Clear();

                //选择[凭证号码]列
                ColumnName = new List<string> { "[凭证号码]", "凭证号码", "凭证号", "凭证编号", "凭证字号" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 4);
                NRG[0, 3] = ColumnName[0];
                ColumnName.Clear();

                //选择[科目编码]列
                ColumnName = new List<string> { "[科目编码]", "科目编码", "科目编号", "科目代码" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 5);
                NRG[0, 4] = ColumnName[0];
                ColumnName.Clear();

                //选择[科目名称]列
                ColumnName = new List<string> { "[科目名称]", "科目名称", "总账科目成文本" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 10);
                NRG[0, 9] = ColumnName[0];
                ColumnName.Clear();

                //选择[摘要]列
                ColumnName = new List<string> { "[摘要]", "摘要", "凭证文本", "业务说明" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 11);
                NRG[0, 10] = ColumnName[0];
                ColumnName.Clear();

                //选择[辅助项目]列
                ColumnName = new List<string> { "[辅助项目1]" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                if (ColumnNumber != 0) { FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 12); }
                ColumnName.Clear();

                if (ColumnNumber != 0)
                {
                    //选择[辅助项目]列
                    ColumnName = new List<string> { "[辅助项目2]" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                    if (ColumnNumber != 0) { FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 13); }
                    ColumnName.Clear();
                }

                if (ColumnNumber != 0)
                {
                    //选择[辅助项目]列
                    ColumnName = new List<string> { "[辅助项目3]" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                    if (ColumnNumber != 0) { FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 14); }
                    ColumnName.Clear();
                }

                //判断是否为借贷方向列示
                bool DrAndCr = false;
                DialogResult dr = MessageBox.Show("记账方式是否为[借方金额][贷方金额]式？若为[方向][金额]式请选“否”", "请选择", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    DrAndCr = true;
                }
                
                //读取余额表内容
                object[,] ORG2 = WST2.Range["B1:C" + AllRows2].Value2;

                //计算科目长度
                Dictionary<int, string> CodeLen = new Dictionary<int, string> { };
                for (int i = 1; i < AllRows2; i++)
                {
                    if (ORG2[i, 1] == null) { continue; }
                    if (CodeLen.Count > 4) { break; }
                    if (CodeLen.ContainsKey(ORG2[i, 1].ToString().Length)) { continue; }
                    CodeLen.Add(FunC.TS(ORG2[i, 1]).Length, FunC.TS(ORG2[i, 1]));
                }

                //字典排序
                CodeLen = CodeLen.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

                //字典转list
                int[] CodeList = (from val in CodeLen select val.Key).ToArray<int>();
                CodeLen.Clear();

                int CodeCount = Math.Min(CodeList.Count(), 4);

                //把科目编码和科目名称存入字典
                Dictionary<string, string> CodeName = new Dictionary<string, string> { };
                for (int i = 1; i < AllRows2; i++)
                {
                    if (ORG2[i, 1] == null) { continue; }
                    if (CodeName.ContainsKey(FunC.TS(ORG2[i, 1]))) { continue; }
                    if (ORG2[i, 2] != null)
                    {
                        CodeName.Add(FunC.TS(ORG2[i, 1]), FunC.TS(ORG2[i, 2]));
                    }
                    else
                    {
                        CodeName.Add(FunC.TS(ORG2[i, 1]), "");
                    }
                }

                //选择发生额列
                if (DrAndCr)
                {
                    //选择[借方金额]列
                    ColumnName = new List<string> { "[借方金额]", "借方金额", "借方发生额", "借方" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    if (ColumnNumber == 0) { return; }
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 16);
                    NRG[0, 15] = ColumnName[0];
                    ColumnName.Clear();

                    //选择[贷方金额]列
                    ColumnName = new List<string> { "[贷方金额]", "贷方金额", "贷方发生额", "贷方" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    if (ColumnNumber == 0) { return; }
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 17);
                    NRG[0, 16] = ColumnName[0];
                    ColumnName.Clear();

                    //计算借贷方向列和科目列
                    for (int i = 1; i < AllRows; i++)
                    {
                        NRG[i, 1] = string.Format("=year(C{0})&\"年\"&month(C{0})&\"月\"&D{0}", i + 1);
                        if (Math.Abs(FunC.TD(NRG[i, 15])) > p)
                        {
                            NRG[i, 14] = "借方";
                        }
                        else if (Math.Abs(FunC.TD(NRG[i, 16])) > p)
                        {
                            NRG[i, 14] = "贷方";
                        }
                        else
                        {
                            NRG[i, 14] = "平";
                        }

                        //计算1-4级科目
                        try
                        {
                            for (int i1 = 0; i1 < CodeCount; i1++)
                            {
                                if (FunC.TS(NRG[i, 4]).Length < CodeList[i1]) { continue; }
                                NRG[i, 5 + i1] = CodeName[FunC.TS(NRG[i, 4]).Substring(0, CodeList[i1])];
                            }
                        }
                        catch { }
                    }

                }
                else
                {
                    //选择[借贷方向]列
                    ColumnName = new List<string> { "[借贷方向]", "[方向]", "借贷方向", "借贷" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    if (ColumnNumber == 0) { return; }
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 15);
                    NRG[0, 14] = ColumnName[0];
                    ColumnName.Clear();

                    //选择金额列
                    ColumnName = new List<string> { "[金额]", "金额", "发生额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //计算借方、贷方和1-4级科目
                    for (int i = 1; i < AllRows; i++)
                    {
                        NRG[i, 1] = string.Format("=year(C{0})&\"年\"&month(C{0})&\"月\"&D{0}", i + 1);
                        //计算借方金额和贷方金额
                        if (NRG[i, 14] != null)
                        {
                            if (FunC.TS(NRG[i, 14]).Contains("借"))
                            {
                                NRG[i, 15] = ORG[i + 1, ColumnNumber];
                            }
                            else if (FunC.TS(NRG[i, 14]).Contains("贷"))
                            {
                                NRG[i, 16] = ORG[i + 1, ColumnNumber];
                            }
                        }

                        //计算1-4级科目
                        try
                        {
                            for (int i1 = 0; i1 < CodeCount; i1++)
                            {
                                if (FunC.TS(NRG[i, 4]).Length < CodeList[i1]) continue; 
                                NRG[i, 5 + i1] = CodeName[FunC.TS(NRG[i, 4]).Substring(0, CodeList[i1])];
                            }
                        }
                        catch { }
                    }

                    //加16、17表头
                    NRG[0, 15] = "[借方金额]";
                    NRG[0, 16] = "[贷方金额]";
                }

                NRG[0, 14] = "[方向]";//命名第15列
                NRG[0, 5] = "[一级科目]";//命名第6、7、8、9列1-4级科目
                NRG[0, 6] = "[二级科目]";
                NRG[0, 7] = "[三级科目]";
                NRG[0, 8] = "[四级科目]";
                NRG[0, 17] = "[抽凭]";//命名第18列[抽凭]
                NRG[0, 18] = "[对方科目]";//命名第19列[对方科目]
                NRG[0, 1] = "[日期&凭证号]";//命名第2列
                NRG[0, 0] = "[辅助]";
                NRG[AllRows - 1, 0] = "1";

                ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

                //新建字典，计算对方科目
                Dictionary<string, string> KeyDic = new Dictionary<string, string> { };
                string TempStr;
                for (int i = 1; i < AllRows; i++)
                {
                    //检查空值
                    if (NRG[i, 2] == null || NRG[i, 3] == null || NRG[i, 9] == null) { continue; }
                    TempStr = FunC.TS(NRG[i, 2]) + FunC.TS(NRG[i, 3]);

                    //如果字典中不包含该Key，则添加Key
                    if (!KeyDic.ContainsKey(TempStr))
                    {
                        KeyDic.Add(TempStr, FunC.TS(NRG[i, 9]));
                    }
                    else if (!KeyDic[TempStr].Contains(FunC.TS(NRG[i, 9])))
                    {
                        KeyDic[TempStr] = KeyDic[TempStr] + ";" + FunC.TS(NRG[i, 9]);
                    }
                }
                for (int i = 1; i < AllRows; i++)
                {
                    if (NRG[i, 2] == null || NRG[i, 3] == null) { continue; }
                    if (NRG[i, 9] == null)
                    {
                        NRG[i, 18] = KeyDic[FunC.TS(NRG[i, 2]) + FunC.TS(NRG[i, 3])];
                    }
                    else
                    {
                        NRG[i, 18] = KeyDic[FunC.TS(NRG[i, 2]) + FunC.TS(NRG[i, 3])].Replace(FunC.TS(NRG[i, 9]) + ";", string.Empty).Replace(";" + FunC.TS(NRG[i, 9]), string.Empty);
                    }
                }


                //删除sheet中的原始数据
                WST.Range["A:" + FunC.CName(AllColumns)].Delete();

                //赋值
                WST.Range["A1:S" + AllRows].Value2 = NRG;

                //释放数组
                ORG = null;

                //调整表格格式

                //首行颜色
                WST.Range["A1:S1"].Interior.Color = Color.LightGray;
                //加框线
                WST.Range["A1:S" + AllRows].Borders.LineStyle = 1;
                //设置数字格式
                WST.Range["P2:Q" + AllRows].NumberFormatLocal = "#,##0.00 ";
                //设置日期格式
                WST.Range["C2:C" + AllRows].NumberFormatLocal = @"yyyy/m/d";
                //ABC列靠左显示
                WST.Range["B2:M" + AllRows].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                //设置自动列宽
                WST.Columns["B:B"].EntireColumn.AutoFit();
                WST.Columns["P:Q"].EntireColumn.AutoFit();
                //隐藏A、D列
                WST.Columns["A:A"].Hidden = true;
                WST.Columns["D:D"].Hidden = true;
                FunC.AddData(WST.Range["R2:R" + AllRows], "抽,补,");

                //删除未选择的辅助项目列
                if (NRG[0, 13] == null && NRG[1, 13] == null)
                {
                    WST.Columns["N:N"].Delete();
                    if (NRG[0, 12] == null && NRG[1, 12] == null)
                    {
                        WST.Columns["M:M"].Delete();
                        if (NRG[0, 11] == null && NRG[1, 11] == null)
                        {
                            WST.Columns["L:L"].Delete();
                        }
                    }
                }

                //释放数组
                NRG = null;

                //冻结行和列
                ExcelApp.ActiveWindow.SplitColumn = 2;
                ExcelApp.ActiveWindow.SplitRow = 1;
                ExcelApp.ActiveWindow.FreezePanes = true;

                ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
                WST.Tab.Color = Color.Black;//设置tab颜色为黑色

            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }
            
        }

        //生成抽凭清单
        private void VoucherCheckList_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                int ColumnNumber1 = 0;
                int ColumnNumber2 = 0;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                object[,] NRG;

                //是否删除抽凭清单表格并继续
                if (FunC.SheetExist("抽凭清单"))
                {
                    DialogResult dr = MessageBox.Show("已存在“抽凭清单”工作表，是否删除并继续？", "请选择", MessageBoxButtons.YesNo);
                    if (dr != DialogResult.Yes)
                    {
                        return;
                    }
                }

                //选中科目余额表并继续
                if (FunC.SelectSheet("序时账") == false) { return; };
                WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["序时账"];
                WST.Select();
                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();

                //规范原始数据
                if (FunC.RangeIsStandard() == false)
                {
                    MessageBox.Show("请规范数据格式，保证数据内容不超出首行和首列");
                    return;
                }

                //将表格读入数组ORG
                ORG = WST.Range["A1:" + FunC.CName(AllColumns) + AllRows.ToString()].Value2;

                //查找[日期&凭证号]和[抽凭]列
                for (int i = 1; i <= AllColumns; i++)
                {
                    if (ORG[1, i] != null)
                    {
                        if (FunC.TS(ORG[1, i]) == "[日期&凭证号]")
                        {
                            ColumnNumber1 = i;
                        }
                        else if (FunC.TS(ORG[1, i]) == "[抽凭]")
                        {
                            ColumnNumber2 = i;
                        }
                    }
                }

                //判断是否查找到指定列
                if (ColumnNumber1 == 0 || ColumnNumber2 == 0)
                {
                    MessageBox.Show("未找到[日期&凭证号]列或[抽凭]列，请加工序时账后重试");
                    return;
                }

                ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

                //新建字典存放抽到的凭证号
                Dictionary<string, string> KeyDic = new Dictionary<string, string> { };

                //将函证信息存入字典
                for (int i = 2; i <= AllRows; i++)
                {
                    if (ORG[i, ColumnNumber2] != null)
                    {
                        if (FunC.TS(ORG[i, ColumnNumber2]) == "抽")
                        {
                            if (ORG[i, ColumnNumber1] != null && !KeyDic.ContainsKey(FunC.TS(ORG[i, ColumnNumber1])))
                            {
                                KeyDic.Add(FunC.TS(ORG[i, ColumnNumber1]), "抽");
                            }
                        }
                        else if (FunC.TS(ORG[i, ColumnNumber2]) == "补")
                        {
                            ORG[i, ColumnNumber2] = null;
                        }
                    }
                }

                //创建目标新数组NRG
                NRG = new object[AllRows, 1];
                NRG[0, 0] = "[抽凭]";

                //往数组写入抽凭列
                for (int i = 2; i <= AllRows; i++)
                {
                    if (ORG[i, ColumnNumber1] != null && KeyDic.ContainsKey(FunC.TS(ORG[i, ColumnNumber1])))
                    {
                        if (ORG[i, ColumnNumber2] != null)
                        {
                            if (FunC.TS(ORG[i, ColumnNumber2]) == "抽")
                            {
                                NRG[i, 0] = "抽";
                            }
                            else
                            {
                                ORG[i, ColumnNumber2] = "补";
                                NRG[i, 0] = "补";
                            }
                        }
                        else
                        {
                            ORG[i, ColumnNumber2] = "补";
                            NRG[i, 0] = "补";
                        }
                    }
                }

                //赋值给序时账的抽凭列
                WST.Range[string.Format("{0}1:{0}{1}", FunC.CName(ColumnNumber2), AllRows)].Value2 = NRG;
                NRG = null;

                //新建抽凭清单表
                FunC.NewSheet("抽凭清单");
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;
                WST.Range["A1:" + FunC.CName(AllColumns) + AllRows.ToString()].Value2 = ORG;
                ORG = null;
                //调整表格格式

                //首行颜色
                WST.Range[string.Format("A1:{0}1", FunC.CName(AllColumns))].Interior.Color = Color.LightGray;
                //加框线
                WST.Range["A1:" + FunC.CName(AllColumns) + AllRows].Borders.LineStyle = 1;
                //设置数字格式
                WST.Range[string.Format("{0}2:{1}{2}", FunC.CName(ColumnNumber2 - 2), FunC.CName(ColumnNumber2 - 1), AllRows)].NumberFormatLocal = "#,##0.00 ";
                //设置日期格式
                WST.Range["C2:C" + AllRows].NumberFormatLocal = @"yyyy/m/d";
                //ABC列靠左显示
                WST.Range["B2:M" + AllRows].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                //设置自动列宽
                WST.Columns["B:B"].EntireColumn.AutoFit();
                WST.Columns[string.Format("{0}:{1}", FunC.CName(ColumnNumber2 - 2), FunC.CName(ColumnNumber2))].EntireColumn.AutoFit();
                //隐藏A、D列
                WST.Columns["A:A"].Hidden = true;
                WST.Columns["D:D"].Hidden = true;

                //冻结行和列
                ExcelApp.ActiveWindow.SplitColumn = 2;
                ExcelApp.ActiveWindow.SplitRow = 1;
                ExcelApp.ActiveWindow.FreezePanes = true;

                //筛选[抽凭]列并删除
                WST.Range["A1:" + FunC.CName(AllColumns) + AllRows].AutoFilter(ColumnNumber2, "");
                WST.Range["A2:A" + AllRows].Select();
                ExcelApp.Selection.EntireRow.Delete();
                WST.AutoFilterMode = false;//取消筛选

                ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }

        }

        //汇总余额表
        private void TotalBalance_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                int ColumnNumber;
                List<string> ColumnName;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                object[,] NRG;

                //选中科目余额表并继续
                if (FunC.SelectSheet("余额表") == false) { return; };
                WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["余额表"];
                WST.Select();
                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();

                //规范原始数据
                if (FunC.RangeIsStandard() == false)
                {
                    MessageBox.Show("请规范数据格式，保证数据内容不超出首行和首列");
                    return;
                }

                //将表格读入数组ORG
                ORG = WST.Range["A1:" + FunC.CName(AllColumns) + AllRows.ToString()].Value2;
                //创建目标新数组NRG
                NRG = new object[AllRows, 9];

                //将列名读入List
                List<string> OName = new List<string> { };
                for (int i = 1; i <= AllColumns; i++)
                {
                    OName.Add(FunC.TS(ORG[1, i]));
                }

                //选择[科目编码]列
                ColumnName = new List<string> { "[科目编码]", "科目编码", "科目编号", "科目号" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 2);
                NRG[0, 1] = ColumnName[0];
                ColumnName.Clear();

                for (int i = 1; i < AllRows; i++)
                {
                    if (NRG[i, 1] == null) { MessageBox.Show("[科目编码]列不能有空行"); return; }
                }

                //选择[科目名称]列
                ColumnName = new List<string> { "[科目名称]", "科目名称" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 3);
                if (ColumnNumber == 0) { return; }
                NRG[0, 2] = ColumnName[0];
                ColumnName.Clear();

                //弹出窗口让用户选择是否按方向和金额列示
                DialogResult dr = MessageBox.Show("期初期末余额是否按[借贷方向][金额]列示？" + Environment.NewLine + "若分别[借方余额][贷方余额]按列示，请选否", "请选择", MessageBoxButtons.YesNo);
                
                if (dr == DialogResult.Yes)
                {
                    //选择[方向]列,可选列
                    ColumnName = new List<string> { "[方向]", "借贷方向" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                    if (ColumnNumber != 0) { FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 4); }
                    NRG[0, 3] = ColumnName[0];
                    ColumnName.Clear();

                    //选择[期初余额]列
                    ColumnName = new List<string> { "[期初余额]", "期初余额", "期初金额", "期初数", "审定期初数" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 5);
                    if (ColumnNumber == 0) { return; }
                    NRG[0, 4] = ColumnName[0];
                    ColumnName.Clear();

                    //选择[期末余额]列
                    ColumnName = new List<string> { "[期末余额]", "期末余额", "期末金额", "期末数", "审定期末数" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 8);
                    if (ColumnNumber == 0) { return; }
                    NRG[0, 7] = ColumnName[0];
                    ColumnName.Clear();
                }
                else if (dr == DialogResult.No)
                {
                    //选择[期初借方]列，先借用NRG的第5列存放数据
                    ColumnName = new List<string> { "[期初借方]", "期初借方", "期初借方金额", "期初借方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //选择[期初贷方]列，先借用NRG的第6列存放数据
                    ColumnName = new List<string> { "[期初贷方]", "期初贷方", "期初贷方金额", "期初贷方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //赋值[方向]列,[期初余额]列
                    NRG[0, 3] = "[方向]";
                    NRG[0, 4] = "[期初余额]";
                    for (int i = 1; i < AllRows; i++)
                    {
                        //规范[期初借方]列数据
                        if (FunC.TS(NRG[i, 5]) == "" || FunC.TDM(NRG[i, 5]) == 0m)
                        {
                            NRG[i, 5] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 5])))
                            {
                                MessageBox.Show("所选[期初借方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //规范[期初贷方]列数据
                        if (FunC.TS(NRG[i, 6]) == "" || FunC.TDM(NRG[i, 6]) == 0m)
                        {
                            NRG[i, 6] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 6])))
                            {
                                MessageBox.Show("所选[期初贷方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //计算[方向]列
                        if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) > 0.0001d)
                        {
                            NRG[i, 3] = "借";
                            NRG[i, 4] = FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]);
                        }
                        else if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) < -0.0001d)
                        {
                            NRG[i, 3] = "贷";
                            NRG[i, 4] = FunC.TD(NRG[i, 6]) - FunC.TD(NRG[i, 5]);
                        }
                        else
                        {
                            NRG[i, 2] = "平";
                        }
                    }

                    //选择[期末借方]列，先借用NRG的第5列存放数据
                    ColumnName = new List<string> { "[期末借方]", "期末借方", "期末借方金额", "期末借方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //选择[期末贷方]列，先借用NRG的第6列存放数据
                    ColumnName = new List<string> { "[期末贷方]", "期末贷方", "期末贷方金额", "期末贷方余额" };
                    ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                    if (ColumnNumber == 0) { return; }
                    ColumnName.Clear();

                    //赋值[期末余额]列
                    NRG[0, 7] = "[期末余额]";
                    for (int i = 1; i < AllRows; i++)
                    {
                        //规范[期末借方]列数据
                        if (FunC.TS(NRG[i, 5]) == "" || FunC.TDM(NRG[i, 5]) == 0m)
                        {
                            NRG[i, 5] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 5])))
                            {
                                MessageBox.Show("所选[期末借方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //规范[期末贷方]列数据
                        if (FunC.TS(NRG[i, 6]) == "" || FunC.TDM(NRG[i, 6]) == 0m)
                        {
                            NRG[i, 6] = 0;
                        }
                        else
                        {
                            if (!FunC.IsNumber(FunC.TS(NRG[i, 6])))
                            {
                                MessageBox.Show("所选[期初贷方]列,第" + (i + 1) + "行存在非数值内容，请检查");
                                return;
                            }
                        }

                        //计算[期末余额]列
                        if (FunC.TS(NRG[i, 3]) == "借")
                        {
                            NRG[i, 7] = Math.Round(FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]), 2);
                        }
                        else if (FunC.TS(NRG[i, 3]) == "贷")
                        {
                            NRG[i, 7] = FunC.TD(NRG[i, 6]) - FunC.TD(NRG[i, 5]);
                        }
                        else
                        {
                            if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) > 0.00001d)
                            {
                                NRG[i, 3] = "借";
                                NRG[i, 7] = FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]);
                            }
                            else if (FunC.TD(NRG[i, 5]) - FunC.TD(NRG[i, 6]) < -0.00001d)
                            {
                                NRG[i, 3] = "贷";
                                NRG[i, 7] = FunC.TD(NRG[i, 6]) - FunC.TD(NRG[i, 5]);
                            }
                        }
                    }

                }
                else { return; }

                //选择[本年借方]列
                ColumnName = new List<string> { "[本年借方]", "本年借方", "本年借方累计", "借方金额累计", "审定借方发生额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                if (ColumnNumber == 0) { return; }
                NRG[0, 5] = ColumnName[0];
                ColumnName.Clear();

                //选择[本年贷方]列
                ColumnName = new List<string> { "[本年贷方]", "本年贷方", "本年贷方累计", "贷方金额累计", "审定贷方发生额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                if (ColumnNumber == 0) { return; }
                NRG[0, 6] = ColumnName[0];
                ColumnName.Clear();

                //使用长度区分科目层级
                Dictionary<int, int> CodeLen = new Dictionary<int, int> { };
                for (int i = 1; i < AllRows; i++)
                {
                    if (!CodeLen.ContainsKey(FunC.TS(NRG[i, 1]).Length))
                    {
                        CodeLen.Add(FunC.TS(NRG[i, 1]).Length, i);
                    }
                }

                //字典排序
                CodeLen = CodeLen.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

                //字典转list
                int[] CodeList = (from val in CodeLen select val.Key).ToArray<int>();

                for (int i = 0; i < CodeList.Count(); i++)
                {
                    CodeLen[CodeList[i]] = i + 1;
                }

                //添加科目层级列
                NRG[0, 8] = "[科目层级]";
                for (int i = 1; i < AllRows; i++)
                {
                    NRG[i, 8] = CodeLen[FunC.TS(NRG[i, 1]).Length];
                }

                ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

                //释放数组
                ORG = null;

                //创建数组
                if (AllRows < (WST.Rows.Count / 3))
                {
                    ORG = new object[AllRows * 3, 9];
                }
                else
                {
                    MessageBox.Show("行数过多，超出excel处理能力");
                    return;
                }

                //读取表头
                for (int i = 0; i < 9; i++)
                {
                    ORG[0, i] = NRG[0, i];
                }

                //新增上级科目行
                int i3 = 1;
                int TempInt;
                for (int i = 1; i < AllRows; i++)
                {
                    if (i3 > AllRows * 3 - 10) { MessageBox.Show("预设行数不足，请修改"); return; }
                    if (FunC.TS(NRG[i, 8]) == "1" || i == 1)
                    {
                        ORG[i3, 0] = 0;
                        for (int i1 = 1; i1 < 9; i1++)
                        {
                            ORG[i3, i1] = NRG[i, i1];
                        }
                        i3 += 1;
                    }
                    else
                    {
                        TempInt = CodeList[CodeLen[FunC.TS(NRG[i, 1]).Length] - 1];
                        if (FunC.TS(NRG[i, 1]).Substring(0, TempInt) == FunC.TS(NRG[i - 1, 1]).Substring(0, Math.Min(TempInt, FunC.TS(NRG[i - 1, 1]).Length)))
                        {
                            ORG[i3, 0] = 0;
                            for (int i1 = 1; i1 < 9; i1++)
                            {
                                ORG[i3, i1] = NRG[i, i1];
                            }
                            i3 += 1;
                        }
                        else
                        {
                            for (int i2 = 1; i2 < CodeLen[FunC.TS(NRG[i, 1]).Length]; i2++)
                            {
                                if (FunC.TS(NRG[i, 1]).Substring(0, CodeList[i2 - 1]) == FunC.TS(NRG[i - 1, 1]).Substring(0, Math.Min(CodeList[i2 - 1], FunC.TS(NRG[i - 1, 1]).Length))) { continue; }
                                ORG[i3, 1] = FunC.TS(NRG[i, 1]).Substring(0, CodeList[i2 - 1]);
                                ORG[i3, 3] = NRG[i, 3];
                                i3 += 1;
                            }
                            ORG[i3, 0] = 0;
                            for (int i1 = 1; i1 < 9; i1++)
                            {
                                ORG[i3, i1] = NRG[i, i1];
                            }
                            i3 += 1;
                        }
                    }
                }


                //移动数组到NRG,同时计算上级科目的期初借贷余
                NRG = null;
                NRG = new object[i3, 9];
                for (int i1 = 0; i1 < 9; i1++)
                {
                    NRG[0, i1] = ORG[0, i1];
                }
                for (int i = i3 - 1; i > 0; i--)
                {
                    for (int i1 = 0; i1 < 9; i1++)
                    {
                        NRG[i, i1] = ORG[i, i1];
                    }
                    if (NRG[i, 0] == null)
                    {
                        TempInt = FunC.TS(NRG[i, 1]).Length;
                        for (int i2 = i; i2 < i3; i2++)
                        {
                            if (FunC.TS(NRG[i2, 1]).Substring(0, Math.Min(TempInt, FunC.TS(NRG[i2, 1]).Length)) != FunC.TS(NRG[i, 1])) { break; }
                            if (CodeLen[FunC.TS(NRG[i, 1]).Length] + 1 == CodeLen[FunC.TS(NRG[i2, 1]).Length])
                            {
                                NRG[i, 4] = FunC.TD(NRG[i, 4]) + FunC.TD(NRG[i2, 4]);
                                NRG[i, 5] = FunC.TD(NRG[i, 5]) + FunC.TD(NRG[i2, 5]);
                                NRG[i, 6] = FunC.TD(NRG[i, 6]) + FunC.TD(NRG[i2, 6]);
                                NRG[i, 7] = FunC.TD(NRG[i, 7]) + FunC.TD(NRG[i2, 7]);
                            }
                        }
                        NRG[i, 8] = CodeLen[FunC.TS(NRG[i, 1]).Length];
                    }
                    if (CodeLen[FunC.TS(NRG[i, 1]).Length] == 1)
                    {
                        NRG[i, 0] = 1;
                    }
                    else
                    {
                        NRG[i, 0] = 0;
                    }
                }
                ORG = null;

                //删除sheet中的原始数据
                WST.Range["A:" + FunC.CName(AllColumns)].Delete();

                NRG[0, 0] = "[显示]";
                //写入数据
                WST.Range["A1:I" + i3.ToString()].Value2 = NRG;

                //调整格式
                WST.Range["A1:I1"].Interior.Color = Color.LightGray;
                //按科目层级修改颜色
                Excel.Range rg;//定义单元格区域对象
                for (int i = 2; i <= i3; i++)
                {
                    rg = WST.Range["A" + i + ":I" + i];
                    switch (NRG[i - 1, 8])
                    {
                        case 1:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                            rg.Interior.TintAndShade = -0.249977111117893;
                            break;
                        case 2:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                            rg.Interior.TintAndShade = -0.149998474074526;
                            break;
                        case 3:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                            rg.Interior.TintAndShade = -4.99893185216834E-02;
                            break;
                        case 4:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent1;
                            rg.Interior.TintAndShade = 0.799981688894314;
                            break;
                        case 5:
                            rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent6;
                            rg.Interior.TintAndShade = 0.799981688894314;
                            break;
                    }
                }
                //释放数组
                NRG = null;

                //设置数字格式
                WST.Range["E2:H" + i3].NumberFormatLocal = "#,##0.00 ";
                //ABC列靠左显示
                WST.Range["B2:C" + i3].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                //设置自动列宽
                WST.Columns["B:H"].EntireColumn.AutoFit();
                //筛选[显示]列
                WST.Range["A1:I" + i3].AutoFilter(1, 1);
                //隐藏[显示]列
                WST.Columns["A:A"].Hidden = true;

                ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
                WST.Tab.Color = Color.Red;//设置tab颜色为红色
            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }
            
        }

        //看账功能
        private void CheckBAJ_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            if (CheckBAJ.Checked)
            {
                //双击事件
                ExcelApp.SheetBeforeDoubleClick += new Excel.AppEvents_SheetBeforeDoubleClickEventHandler(FunC.CheckDoubleClick);
            }
            else
            {
                try
                {
                    ExcelApp.SheetBeforeDoubleClick -= new Excel.AppEvents_SheetBeforeDoubleClickEventHandler(FunC.CheckDoubleClick);
                }
                catch
                {

                }
            }
        }

        //账表加工设置
        private void BalanceAndJournalSetting_Click(object sender, RibbonControlEventArgs e)
        {
            Form BAJSetting = new BAJSetting
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            BAJSetting.Show();
        }

        //加工往来款
        private void EditCurrentAccount_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                int ColumnNumber;
                List<string> ColumnName;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                object[,] NRG;

                //选中往来款明细表并继续
                if (FunC.SelectSheet("往来款明细") == false) { return; };
                WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["往来款明细"];
                WST.Select();
                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();

                //规范原始数据
                if (FunC.RangeIsStandard() == false)
                {
                    MessageBox.Show("请规范数据格式，保证数据内容不超出首行和首列");
                    return;
                }

                //将表格读入数组ORG
                ORG = WST.Range["A1:" + FunC.CName(AllColumns) + AllRows.ToString()].Value2;
                //创建目标新数组NRG
                NRG = new object[AllRows, 9];

                //将列名读入List
                List<string> OName = new List<string> { };
                for (int i = 1; i <= AllColumns; i++)
                {
                    if (ORG[1, i] != null)
                    {
                        OName.Add(FunC.TS(ORG[1, i]));
                    }
                    else
                    {
                        OName.Add("0");
                    }
                }

                //选择[客户编号]列
                ColumnName = new List<string> { "[客户编号]", "客户编号", "客户编码", "供应商编码" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 1);
                NRG[0, 0] = ColumnName[0];
                ColumnName.Clear();

                //选择[客户名称]列
                ColumnName = new List<string> { "[客户名称]", "客户名称", "供应商名称" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 2);
                if (ColumnNumber == 0) { return; }
                NRG[0, 1] = ColumnName[0];
                ColumnName.Clear();

                //选择[一级科目]列
                ColumnName = new List<string> { "[一级科目]", "一级科目" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 3);
                if (ColumnNumber == 0) { return; }
                NRG[0, 2] = ColumnName[0];
                ColumnName.Clear();

                //选择[明细科目]列,可选列
                ColumnName = new List<string> { "[明细科目]", "明细科目" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                if (ColumnNumber != 0) { FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 4); }
                NRG[0, 3] = ColumnName[0];
                ColumnName.Clear();

                //选择[期初余额]列
                ColumnName = new List<string> { "[期初余额]", "期初余额", "去年余额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 5);
                if (ColumnNumber == 0) { return; }
                if (!FunC.IsNumColumn(NRG, 4, 1, AllRows)) { return; }
                NRG[0, 4] = ColumnName[0];
                ColumnName.Clear();

                //选择[本期借方]列
                ColumnName = new List<string> { "[本期借方]", "本期借方", "本年借方", "本年累计借方" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                if (ColumnNumber == 0) { return; }
                if (!FunC.IsNumColumn(NRG, 5, 1, AllRows)) { return; }
                NRG[0, 5] = ColumnName[0];
                ColumnName.Clear();

                //选择[本期贷方]列
                ColumnName = new List<string> { "[本期贷方]", "本期贷方", "本年贷方", "本年累计贷方" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                if (ColumnNumber == 0) { return; }
                if (!FunC.IsNumColumn(NRG, 6, 1, AllRows)) { return; }
                NRG[0, 6] = ColumnName[0];
                ColumnName.Clear();

                //选择[期末余额]列
                ColumnName = new List<string> { "[期末余额]", "期末余额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 8);
                if (ColumnNumber == 0) { return; }
                if (!FunC.IsNumColumn(NRG, 7, 1, AllRows)) { return; }
                NRG[0, 7] = ColumnName[0];
                ColumnName.Clear();

                //选择[辅助项目]列，可选列
                ColumnName = new List<string> { "[辅助项目]", "辅助项目" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, false);
                if (ColumnNumber != 0)
                {
                    FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 9);
                }
                else
                {
                    NRG[0, 8] = ColumnName[0];
                }
                ColumnName.Clear();

                //删除期初借贷余均为零的行
                DialogResult dr = MessageBox.Show("是否删除期初借贷余均为零的行？", "请选择", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    //释放数组
                    ORG = null;
                    ORG = NRG;
                    NRG = null;
                    NRG = new object[AllRows, 9];

                    for (int i1 = 0; i1 < 9; i1++)
                    {
                        NRG[0, i1] = ORG[0, i1];
                    }

                    int i3 = 1;
                    for (int i = 1; i < AllRows; i++)
                    {
                        if (FunC.TD(ORG[i, 4]) != 0d || FunC.TD(ORG[i, 5]) != 0d || FunC.TD(ORG[i, 6]) != 0d || FunC.TD(ORG[i, 7]) != 0d)
                        {
                            for (int i1 = 0; i1 < 9; i1++)
                            {
                                NRG[i3, i1] = ORG[i, i1];
                            }
                            i3 += 1;
                        }
                    }
                }

                //删除sheet中的原始数据
                WST.Range["A:" + FunC.CName(AllColumns)].Delete();

                //写入数据
                WST.Range["A1:I" + AllRows.ToString()].Value2 = NRG;

                //释放数组
                ORG = null;
                NRG = null;
                AllRows = FunC.AllRows();
                //重新将表格读入数组ORG
                ORG = WST.Range["A1:I" + AllRows.ToString()].Value2;

                //对一级科目去重
                List<string> SheetsName0 = new List<string> { "[一级科目]" };
                for (int i = 2; i <= AllRows; i++)
                {
                    try
                    {
                        if (FunC.TS(ORG[i, 3]) != SheetsName0[SheetsName0.Count - 1])
                        {
                            SheetsName0.Add(FunC.TS(ORG[i, 3]));
                        }
                    }
                    catch (NullReferenceException)
                    {
                        MessageBox.Show(string.Format("一级科目列第{0}行存在空值，请检查",i));
                        ORG = null;
                        return;
                    }

                }
                List<string> SheetsName1 = new List<string> { };
                SheetsName0.Distinct().ToList().ForEach(s => SheetsName1.Add(s));

                //定义往来款字典
                Dictionary<string, string> SheetsName = new Dictionary<string, string>
                {
                    { "应收账款", "借" },
                    { "预付账款", "借" },
                    { "其他应收款", "借" },
                    { "应付账款", "贷" },
                    { "预收账款", "贷" },
                    { "其他应付款", "贷" }
                };

                //检查一级科目是否规范
                for (int i = 1; i < SheetsName1.Count; i++)
                {
                    if (!SheetsName.ContainsKey(SheetsName1[i]))
                    {
                        MessageBox.Show("一级科目列存在非往来款科目，请检查");
                        ORG = null;
                        return;
                    }
                }

                //是否修改贷方往来款发生额方向和贷方发生额方向
                dr = MessageBox.Show("是否修改贷方科目以及贷方发生额列的正负号？" + Environment.NewLine + "如果是SAP导出的往来款，请选择“是”", "请选择", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    for (int i = 2; i <= AllRows; i++)
                    {
                        if (SheetsName[FunC.TS(ORG[i, 3])] == "贷")
                        {
                            ORG[i, 5] = -FunC.TD(ORG[i, 5]);
                            ORG[i, 8] = -FunC.TD(ORG[i, 8]);
                        }
                        ORG[i, 7] = -FunC.TD(ORG[i, 7]);
                    }
                }

                ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

                //应收账款表
                if (!FunC.AddCASheet(ORG, AllRows, "应收账款", "预收账款")) { return; }
                //预付账款表
                if (!FunC.AddCASheet(ORG, AllRows, "预付账款", "应付账款")) { return; }
                //其他应收款表
                if (!FunC.AddCASheet(ORG, AllRows, "其他应收款", "其他应付款")) { return; }
                //应付账款表
                if (!FunC.AddCASheet(ORG, AllRows, "应付账款", "预付账款")) { return; }
                //预收账款表
                if (!FunC.AddCASheet(ORG, AllRows, "预收账款", "应收账款")) { return; }
                //其他应付款表
                if (!FunC.AddCASheet(ORG, AllRows, "其他应付款", "其他应收款")) { return; }

                ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }
        }

        //拆分账龄
        private void AgeOfAccount_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                int ColumnNumber;
                string ProjectName;
                List<string> ColumnName;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                object[,] NRG;

                //选择科目名称
                using (var form = new SelectCA())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        ProjectName = form.ReturnValue;
                    }
                    else
                    {
                        return;
                    }
                }

                //检查科目名称是否有效
                if (!FunC.SheetExist(ProjectName))
                {
                    MessageBox.Show("请将上年账龄表与加工完的往来款表放在同一工作簿");
                    return;
                }
                else if (FunC.TS(ExcelApp.Sheets[ProjectName].Range["A1"].Value) != "[客户编号]")
                {
                    MessageBox.Show("请先加工往来款");
                    return;
                }
                //定义第二个工作表
                Excel.Worksheet WST2 = ExcelApp.ActiveWorkbook.Sheets[ProjectName];

                DialogResult dr = MessageBox.Show(" 请在上年账龄表中使用该功能" + Environment.NewLine + "是否继续？", "请选择", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No) { return; }

                //规范原始数据
                if (FunC.RangeIsStandard() == false)
                {
                    MessageBox.Show("请规范数据格式，保证数据内容不超出首行和首列");
                    return;
                }

                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();
                if (AllRows < 2) { return; }

                //将表格读入数组ORG
                ORG = WST.Range["A1:" + FunC.CName(AllColumns) + AllRows.ToString()].Value2;
                //创建目标新数组NRG
                NRG = new object[AllRows, 8];

                //将列名读入List
                List<string> OName = new List<string> { };
                for (int i = 1; i <= AllColumns; i++)
                {
                    OName.Add(FunC.TS(ORG[1, i]));
                }

                //选择[客户名称]或[客户编号]列，做为引用的依据
                ColumnName = new List<string> { "[客户名称]或[客户编号]" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 1);
                NRG[0, 0] = ColumnName[0];
                ColumnName.Clear();

                //选择审定[期末余额]列
                ColumnName = new List<string> { "[期末余额]", "审定期末数", "期末审定数", "审定期末余额" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 2);
                if (!FunC.IsNumColumn(NRG, 1, 1, AllRows)) { return; }
                NRG[0, 1] = ColumnName[0];
                ColumnName.Clear();

                //选择账龄[1年以内]列
                ColumnName = new List<string> { "[1年以内]", "1年以内", "一年以内" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 3);
                if (!FunC.IsNumColumn(NRG, 2, 1, AllRows)) { return; }
                NRG[0, 2] = ColumnName[0];
                ColumnName.Clear();

                //选择账龄[1-2年]列
                ColumnName = new List<string> { "[1-2年]", "1-2年", "1~2年" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 4);
                if (!FunC.IsNumColumn(NRG, 3, 1, AllRows)) { return; }
                NRG[0, 3] = ColumnName[0];
                ColumnName.Clear();

                //选择账龄[2-3年]列
                ColumnName = new List<string> { "[2-3年]", "2-3年", "2~3年" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 5);
                if (!FunC.IsNumColumn(NRG, 4, 1, AllRows)) { return; }
                NRG[0, 4] = ColumnName[0];
                ColumnName.Clear();

                //选择账龄[3-4年]列
                ColumnName = new List<string> { "[3-4年]", "3-4年", "3~4年" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 6);
                if (!FunC.IsNumColumn(NRG, 5, 1, AllRows)) { return; }
                NRG[0, 5] = ColumnName[0];
                ColumnName.Clear();

                //选择账龄[4-5年]列
                ColumnName = new List<string> { "[4-5年]", "4-5年", "4~5年" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 7);
                if (!FunC.IsNumColumn(NRG, 6, 1, AllRows)) { return; }
                NRG[0, 6] = ColumnName[0];
                ColumnName.Clear();

                //选择账龄[5年以上]列
                ColumnName = new List<string> { "[5年以上]", "5年以上", "五年以上" };
                ColumnNumber = FunC.SelectColumn(ColumnName, OName, true);
                if (ColumnNumber == 0) { return; }
                FunC.TrColumn(ORG, NRG, AllRows, ColumnNumber, 8);
                if (!FunC.IsNumColumn(NRG, 7, 1, AllRows)) { return; }
                NRG[0, 7] = ColumnName[0];
                ColumnName.Clear();

                //借用数组ORG删除NRG中期末余额为0或空值的行
                ORG = new object[AllRows, 8];
                for (int i = 1; i < 8; i++)
                {
                    ORG[0, i] = NRG[0, i];
                }
                int i3 = 1;
                for (int i = 1; i < AllRows; i++)
                {
                    if (Math.Abs(FunC.TD(NRG[i, 1])) > p)
                    {
                        for (int i1 = 0; i1 < 8; i1++)
                        {
                            ORG[i3, i1] = NRG[i, i1];
                        }
                        i3 += 1;
                    }
                }

                NRG = null;
                NRG = new object[i3, 9];
                for (int i = 0; i < i3; i++)
                {
                    for (int i1 = 0; i1 < 8; i1++)
                    {
                        NRG[i, i1] = ORG[i, i1];
                    }
                }


                //检查余额行
                if (i3 == 1)
                {
                    MessageBox.Show("余额表中未发现余额大于零的行，请检查并重新开始");
                    return;
                }

                //选中第二个工作表
                WST2.Select();

                //读取行列数
                AllRows = FunC.AllRows();
                AllColumns = FunC.AllColumns();

                //将表格读入数组ORG
                ORG = WST2.Range["A1:" + FunC.CName(AllColumns + 7) + AllRows].Value2;

                //找对应的列号
                int ColumnNumber1 = 0;//客户编号列或客户名称列
                int ColumnNumber2 = 0;//期初审定数
                int ColumnNumber3 = 0;//期末审定数
                int ColumnNumber4 = 0;//本期借方
                int ColumnNumber5 = 0;//本期贷方
                ColumnName = new List<string> { "[客户编号]", "[客户名称]", "[期初审定数]", "[期末审定数]", "[本期借方]", "[本期贷方]" };
                if (FunC.IsNumber(FunC.TS(NRG[1, 0])))
                {
                    ColumnNumber = 0;
                }
                else
                {
                    ColumnNumber = 1;
                }
                for (int i = 1; i <= AllColumns; i++)
                {
                    if (FunC.TS(ORG[1, i]) == ColumnName[ColumnNumber])
                    {
                        ColumnNumber1 = i;
                    }
                    else if (FunC.TS(ORG[1, i]) == ColumnName[2])
                    {
                        ColumnNumber2 = i;
                    }
                    else if (FunC.TS(ORG[1, i]) == ColumnName[3])
                    {
                        ColumnNumber3 = i;
                    }
                    else if (FunC.TS(ORG[1, i]) == ColumnName[4])
                    {
                        ColumnNumber4 = i;
                    }
                    else if (FunC.TS(ORG[1, i]) == ColumnName[5])
                    {
                        ColumnNumber5 = i;
                    }
                }
                //检查是否匹配成功
                if (ColumnNumber1 == 0 || ColumnNumber2 == 0 || ColumnNumber3 == 0 || ColumnNumber4 == 0 || ColumnNumber5 == 0)
                {
                    MessageBox.Show("未匹配到[期初审定数]、[期末审定数]、[本期借方]或[本期贷方]列，请检查并重新开始");
                    return;
                }

                ////检查指定列是否有非数字内容
                //ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新
                //FunC.ColorNotNum(FunC.CName(ColumnNumber2) + "2:" + FunC.CName(ColumnNumber2) + AllRows);
                //FunC.ColorNotNum(FunC.CName(ColumnNumber3) + "2:" + FunC.CName(ColumnNumber3) + AllRows);
                //FunC.ColorNotNum(FunC.CName(ColumnNumber4) + "2:" + FunC.CName(ColumnNumber4) + AllRows);
                //FunC.ColorNotNum(FunC.CName(ColumnNumber5) + "2:" + FunC.CName(ColumnNumber5) + AllRows);
                //ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
                //改表头
                for (int i = 1; i <= 6; i++)
                {
                    ORG[1, AllColumns + i] = NRG[0, i + 1];
                }
                //匹配余额
                for (int i = 2; i <= AllRows; i++)
                {
                    //如果期末余额为0，则账龄为0
                    if (Math.Abs(FunC.TD(ORG[i, ColumnNumber3])) <= p)
                    {
                        for (int i1 = 1; i1 <= 6; i1++)
                        {
                            ORG[i, AllColumns + i1] = 0;
                        }
                    }
                    //如果期初余额为0，则账龄为[1年以内]
                    else if (Math.Abs(FunC.TD(ORG[i, ColumnNumber2])) <= p)
                    {
                        ORG[i, AllColumns + 1] = ORG[i, ColumnNumber3];
                        for (int i1 = 2; i1 <= 6; i1++)
                        {
                            ORG[i, AllColumns + i1] = 0;
                        }
                    }
                    //如果借方科目借方金额大于期末余额
                    else if ((ProjectName.Contains("应收") || ProjectName.Contains("预付")) && FunC.TD(ORG[i, ColumnNumber4]) >= FunC.TD(ORG[i, ColumnNumber3]))
                    {
                        ORG[i, AllColumns + 1] = FunC.TD(ORG[i, ColumnNumber3]);
                        for (int i2 = 2; i2 <= 6; i2++)
                        {
                            ORG[i, AllColumns + i2] = 0;
                        }
                    }
                    //如果贷方科目贷方金额大于期末余额
                    else if ((ProjectName.Contains("应付") || ProjectName.Contains("预收")) && FunC.TD(ORG[i, ColumnNumber5]) >= FunC.TD(ORG[i, ColumnNumber3]))
                    {
                        ORG[i, AllColumns + 1] = FunC.TD(ORG[i, ColumnNumber3]);
                        for (int i2 = 2; i2 <= 6; i2++)
                        {
                            ORG[i, AllColumns + i2] = 0;
                        }
                    }
                    //否则从账龄表中匹配
                    else
                    {
                        for (int i1 = 1; i1 < i3; i1++)
                        {
                            //如果不匹配，直接下一个
                            if (Math.Abs(FunC.TD(ORG[i, ColumnNumber2]) - FunC.TD(NRG[i1, 1])) > p || FunC.TS(ORG[i, ColumnNumber1]) != FunC.TS(NRG[i1, 0]))
                            {
                                continue;
                            }
                            //检查是否被匹配过，如果被匹配过直接下一个，防止出现同一供应商多个余额相等的情况
                            if (NRG[i1, 8] != null)
                            {
                                ORG[i, AllColumns + 1] = "请手动计算该供应商的账龄";
                                continue;
                            }

                            NRG[i1, 8] = 1;
                            //检查本期发生额，如果为0则平移账龄
                            if (Math.Abs(FunC.TD(ORG[i, ColumnNumber4])) < p && Math.Abs(FunC.TD(ORG[i, ColumnNumber5])) < p)
                            {
                                ORG[i, AllColumns + 1] = 0;
                                for (int i2 = 2; i2 <= 5; i2++)
                                {
                                    ORG[i, AllColumns + i2] = FunC.TD(NRG[i1, i2]);
                                }
                                ORG[i, AllColumns + 6] = FunC.TD(NRG[i1, 6]) + FunC.TD(NRG[i1, 7]);
                            }
                            //如果本期有发生额，则计算账龄
                            else
                            {
                                if (ProjectName.Contains("应收") || ProjectName.Contains("预付"))
                                {
                                    //一年以内
                                    ORG[i, AllColumns + 1] = FunC.TD(ORG[i, ColumnNumber4]);
                                    //1-2年
                                    ORG[i, AllColumns + 2] = Math.Min(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber4]), FunC.TD(NRG[i1, 2]));
                                    //2-3年
                                    ORG[i, AllColumns + 3] = Math.Min(Math.Max(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber4]) - FunC.TD(NRG[i1, 2]), 0), FunC.TD(NRG[i1, 3]));
                                    //3-4年
                                    ORG[i, AllColumns + 4] = Math.Min(Math.Max(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber4]) - FunC.TD(NRG[i1, 2]) - FunC.TD(NRG[i1, 3]), 0), FunC.TD(NRG[i1, 4]));
                                    //4-5年
                                    ORG[i, AllColumns + 5] = Math.Min(Math.Max(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber4]) - FunC.TD(NRG[i1, 2]) - FunC.TD(NRG[i1, 3]) - FunC.TD(NRG[i1, 4]), 0), FunC.TD(NRG[i1, 5]));
                                    //5年以上
                                    ORG[i, AllColumns + 6] = FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, AllColumns + 1]) - FunC.TD(ORG[i, AllColumns + 2]) - FunC.TD(ORG[i, AllColumns + 3]) - FunC.TD(ORG[i, AllColumns + 4]) - FunC.TD(ORG[i, AllColumns + 5]);
                                }
                                else
                                {
                                    //一年以内
                                    ORG[i, AllColumns + 1] = FunC.TD(ORG[i, ColumnNumber5]);
                                    //1-2年
                                    ORG[i, AllColumns + 2] = Math.Min(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber5]), FunC.TD(NRG[i1, 2]));
                                    //2-3年
                                    ORG[i, AllColumns + 3] = Math.Min(Math.Max(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber5]) - FunC.TD(NRG[i1, 2]), 0), FunC.TD(NRG[i1, 3]));
                                    //3-4年
                                    ORG[i, AllColumns + 4] = Math.Min(Math.Max(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber5]) - FunC.TD(NRG[i1, 2]) - FunC.TD(NRG[i1, 3]), 0), FunC.TD(NRG[i1, 4]));
                                    //4-5年
                                    ORG[i, AllColumns + 5] = Math.Min(Math.Max(FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, ColumnNumber5]) - FunC.TD(NRG[i1, 2]) - FunC.TD(NRG[i1, 3]) - FunC.TD(NRG[i1, 4]), 0), FunC.TD(NRG[i1, 5]));
                                    //5年以上
                                    ORG[i, AllColumns + 6] = FunC.TD(ORG[i, ColumnNumber3]) - FunC.TD(ORG[i, AllColumns + 1]) - FunC.TD(ORG[i, AllColumns + 2]) - FunC.TD(ORG[i, AllColumns + 3]) - FunC.TD(ORG[i, AllColumns + 4]) - FunC.TD(ORG[i, AllColumns + 5]);
                                }
                            }
                        }
                    }

                    //规范数据
                    ORG[i, AllColumns + 1] = FunC.TD(ORG[i, AllColumns + 1]);
                    ORG[i, AllColumns + 2] = FunC.TD(ORG[i, AllColumns + 2]);
                    ORG[i, AllColumns + 3] = FunC.TD(ORG[i, AllColumns + 3]);
                    ORG[i, AllColumns + 4] = FunC.TD(ORG[i, AllColumns + 4]);
                    ORG[i, AllColumns + 5] = FunC.TD(ORG[i, AllColumns + 5]);
                    ORG[i, AllColumns + 6] = FunC.TD(ORG[i, AllColumns + 6]);

                    //最后增加一列验证列
                    ORG[i, AllColumns + 7] = "=ABS(" + FunC.CName(ColumnNumber3) + i + "-sum(" + FunC.CName(AllColumns + 1) + i + ":" + FunC.CName(AllColumns + 6) + i + "))<0.01";
                }

                //赋值
                WST2.Range["A1:" + FunC.CName(AllColumns + 7) + AllRows].Value2 = ORG;
                //加框线
                WST2.Range["A1:" + FunC.CName(AllColumns + 7) + AllRows].Borders.LineStyle = 1;
                //设置首行颜色为灰色
                WST2.Range["A1:" + FunC.CName(AllColumns + 7) + "1"].Interior.ColorIndex = 15;
                //设置数字格式
                WST2.Range[FunC.CName(AllColumns + 1) + "2:" + FunC.CName(AllColumns + 7) + "1"].NumberFormatLocal = "#,##0.00 ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }
        }

        //生成函证列表
        private void Confirmation_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                //主键，关联各往来款表用
                string PrKey;

                //存放函证信息的数组NRG
                object[,] NRG;
                NRG = new object[6000, 6];//暂时设置为6000行，太多可能会比较慢
                NRG[0, 0] = 1;
                //临时数组
                object[,] TempNRG;

                //选择用编号还是列名作为主键
                using (var form = new SelectKeyN())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        PrKey = form.ReturnValue;
                    }
                    else
                    {
                        return;
                    }
                }

                ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

                //定义往来款字典
                Dictionary<string, string> KeyDic = new Dictionary<string, string> { };

                //将往来款中标注函证的key列加入字典
                KeyDic = FunC.ConfirmationAddPrKey("应收账款", PrKey, KeyDic);
                KeyDic = FunC.ConfirmationAddPrKey("预付账款", PrKey, KeyDic);
                KeyDic = FunC.ConfirmationAddPrKey("其他应收款", PrKey, KeyDic);
                KeyDic = FunC.ConfirmationAddPrKey("应付账款", PrKey, KeyDic);
                KeyDic = FunC.ConfirmationAddPrKey("预收账款", PrKey, KeyDic);
                KeyDic = FunC.ConfirmationAddPrKey("其他应付款", PrKey, KeyDic);

                //检查KeyDic是否为空
                if (KeyDic.Count == 0) { MessageBox.Show("未发现抽函信息，请检查"); return; }

                //为各往来款表函证列添加[补]并读取到数组
                NRG = FunC.ConfirmationAddCon("应收账款", PrKey, KeyDic, NRG);
                NRG = FunC.ConfirmationAddCon("预付账款", PrKey, KeyDic, NRG);
                NRG = FunC.ConfirmationAddCon("其他应收款", PrKey, KeyDic, NRG);
                NRG = FunC.ConfirmationAddCon("应付账款", PrKey, KeyDic, NRG);
                NRG = FunC.ConfirmationAddCon("预收账款", PrKey, KeyDic, NRG);
                NRG = FunC.ConfirmationAddCon("其他应付款", PrKey, KeyDic, NRG);

                TempNRG = new object[int.Parse(FunC.TS(NRG[0, 0])), 6];

                for (int i = 1; i < int.Parse(FunC.TS(NRG[0, 0])); i++)
                {
                    for (int i1 = 0; i1 < 6; i1++)
                    {
                        TempNRG[i, i1] = NRG[i, i1];
                    }
                }
                //清空数组
                NRG = null;

                TempNRG[0, 0] = "[客户编号]";
                TempNRG[0, 1] = "[客户名称]";
                TempNRG[0, 2] = "[科目名称]";
                TempNRG[0, 3] = "[明细科目]";
                TempNRG[0, 4] = "[审定期末余额]";
                TempNRG[0, 5] = "[函证]";

                FunC.NewSheet("发函清单");//创建发函清单表
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;
                WST.Range["A1:F" + TempNRG.GetLength(0)].Value2 = TempNRG;//赋值函证清单

                //重新定义数组，按公司名称做表
                NRG = new object[KeyDic.Count + 1, 12];
                //定义关键列数组
                object[] KeyDicArr;
                //将客户名称存入数组
                if (PrKey == "客户编号")
                {
                    for (int i = 1; i < TempNRG.GetLength(0); i++)
                    {
                        if (KeyDic[FunC.TS(TempNRG[i, 0])] == "函")
                        {
                            KeyDic[FunC.TS(TempNRG[i, 0])] = FunC.TS(TempNRG[i, 1]);
                        }
                    }

                    KeyDicArr = KeyDic.Values.ToArray();
                }
                else
                {
                    KeyDicArr = KeyDic.Keys.ToArray();
                }
                for (int i = 1; i < NRG.GetLength(0); i++)
                {
                    NRG[i, 0] = KeyDicArr[i - 1];
                    NRG[i, 1] = "=SUMIFS($E:$E,$B:$B,$H" + (i + 1) + ",$C:$C,I$1)";
                    NRG[i, 2] = "=SUMIFS($E:$E,$B:$B,$H" + (i + 1) + ",$C:$C,J$1)";
                    NRG[i, 3] = "=SUMIFS($E:$E,$B:$B,$H" + (i + 1) + ",$C:$C,K$1)";
                    NRG[i, 4] = "=SUMIFS($E:$E,$B:$B,$H" + (i + 1) + ",$C:$C,L$1)";
                    NRG[i, 5] = "=SUMIFS($E:$E,$B:$B,$H" + (i + 1) + ",$C:$C,M$1)";
                    NRG[i, 6] = "=SUMIFS($E:$E,$B:$B,$H" + (i + 1) + ",$C:$C,N$1)";
                }

                TempNRG = null;

                NRG[0, 0] = "[客户名称]";
                NRG[0, 1] = "[应收账款]";
                NRG[0, 2] = "[预付账款]";
                NRG[0, 3] = "[其他应收款]";
                NRG[0, 4] = "[应付账款]";
                NRG[0, 5] = "[预收账款]";
                NRG[0, 6] = "[其他应付款]";
                NRG[0, 7] = "[备注]";
                NRG[0, 8] = "邮编";
                NRG[0, 9] = "联系地址";
                NRG[0, 10] = "联系人";
                NRG[0, 11] = "联系电话";

                Excel.Range rg = WST.Range["H1:S" + NRG.GetLength(0)];//定义有效区域
                rg.Value2 = NRG;//赋值函证清单

                //加框线
                rg.Borders.LineStyle = 1;
                //自动列宽
                rg.EntireColumn.AutoFit();
                //设置数字格式
                WST.Range["I2:N" + NRG.GetLength(0)].NumberFormatLocal = "#,##0.00 ";
                //首行颜色设置为灰色
                WST.Range["H1:S1"].Interior.ColorIndex = 15;
                //冻结行和列
                ExcelApp.ActiveWindow.SplitRow = 1;
                ExcelApp.ActiveWindow.FreezePanes = true;

                NRG = null;

                ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }
            
        }

        //生成word函证
        private void ConfirmationWord_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                ExcelApp = Globals.ThisAddIn.Application;
                WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

                int AllRows;
                int AllColumns;
                string TempStr;
                //原始表格数组ORG
                object[,] ORG;
                //目标新数组NRG
                //object[,] NRG;

                //选中发函清单表并继续
                if (FunC.SelectSheet("发函清单") == false) return;
                WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets["发函清单"];
                WST.Select();
                AllColumns = FunC.AllColumns();

                //将表头读入数组ORG
                ORG = WST.Range[string.Format("A1:{0}1",FunC.CName(AllColumns))].Value2;

                //将需要刷新数据的列加入字典
                Dictionary<string, int> ColDict = new Dictionary<string, int> { };

                Regex rg = new Regex(@"^\[.*\]$");
                //查找需要添加的列
                for (int i = 1; i <= AllColumns; i++)
                {
                    TempStr = FunC.TS(ORG[1, i]);
                    if (rg.IsMatch(TempStr))
                    {
                        if(ColDict.ContainsKey(TempStr))
                        {
                            MessageBox.Show("首行字段名有重复，请检查！");
                            return;
                        }
                        else
                        {
                            ColDict.Add(TempStr, i);
                        }
                    }
                }

                //创建目标新数组NRG
                if (!ColDict.ContainsKey("[客户名称]")) { MessageBox.Show("未发现[客户名称]列，请检查"); return; }
                AllRows = FunC.AllRows(FunC.CName(ColDict["[客户名称]"]));
                ORG = WST.Range[string.Format("A1:{0}{1}", FunC.CName(AllColumns), AllRows)].Value2;
                // NRG = new object[FunC.AllRows(FunC.CName(ColDict["[客户名称]"])), ColDict.Count + 2];

                //获取存放函证的文件夹路径
                string FolderPath = ExcelApp.ActiveWorkbook.Path;
                FolderBrowserDialog folderDialog = new FolderBrowserDialog
                {
                    Description = "请选择文件夹存放函证",
                    SelectedPath = FolderPath
                };
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    FolderPath = folderDialog.SelectedPath;
                }
                else
                {
                    return;
                }


                //从我的文档读取配置
                string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

                //读取指定信息
                //从父节点CurrentAccount中读取配置名为AccountingFirmName的值，作为事务所名称，默认为致同
                string AccountingFirmName = clsConfig.ReadConfig<string>("CurrentAccount", "AccountingFirmName", "xx会计师事务所");
                //从父节点CurrentAccount中读取配置名为Auditee的值，作为被审计单位名称，默认为空
                string OurCompany = clsConfig.ReadConfig<string>("CurrentAccount", "Auditee", "xx公司");
                //从父节点CurrentAccount中读取配置名为ReplyAddress的值，作为回函地址，默认为致同
                string ReplyAddress = clsConfig.ReadConfig<string>("CurrentAccount", "ReplyAddress", "xx省xx市xx区xx街道xx楼xx层");
                //从父节点CurrentAccount中读取配置名为PostalCode的值，作为回函邮编，默认为致同
                string PostalCode = clsConfig.ReadConfig<string>("CurrentAccount", "PostalCode", "000000");
                //从父节点CurrentAccount中读取配置名为AuditDeadline的值，作为审计截止日，默认为2019年12月31日
                string AuditDeadline = clsConfig.ReadConfig<string>("CurrentAccount", "AuditDeadline", "20xx年12月31日");
                //从父节点CurrentAccount中读取配置名为Contact的值，作为联系人名称，默认为空
                string Contact = clsConfig.ReadConfig<string>("CurrentAccount", "Contact", "请修改");
                //从父节点CurrentAccount中读取配置名为Telephone的值，作为联系电话，默认为空
                string Telephone = clsConfig.ReadConfig<string>("CurrentAccount", "Telephone", "请修改");
                //从父节点CurrentAccount中读取配置名为Department的值，作为部门，默认为空
                string Department = clsConfig.ReadConfig<string>("CurrentAccount", "Department", "请修改");
                //从父节点CurrentAccount中读取配置名为Leading的值，作为部门负责人，默认为空
                string Leading = clsConfig.ReadConfig<string>("CurrentAccount", "Leading", "请修改");

                if (Directory.Exists(FolderPath + "\\" + OurCompany))//如果存在就删除文件夹
                {
                    try
                    {
                        DirectoryInfo subdir = new DirectoryInfo(FolderPath + "\\" + OurCompany);
                        subdir.Delete(true);
                    }
                    catch
                    {
                        MessageBox.Show("选定目录存在同名文件夹，请先关闭文件夹中的文件，删除文件夹后重试！");
                        return;
                    }
                }

                Directory.CreateDirectory(FolderPath + "\\" + OurCompany);

                Word.Application WordApp = new Word.Application(); //初始化
                WordApp.Visible = false;//使文档不可见
                Word.Document WordDoc;

                //在我的文档创建模板文件夹
                if (!Directory.Exists(strPath + "\\HertZTemplate"))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory(strPath + "\\HertZTemplate");
                }
                //将模板提取出来
                if (!File.Exists(strPath + "\\HertZTemplate\\往来询证函模板.dotx"))
                {
                    byte[] sampleCA = Properties.Resources.往来询证函模板; //取出Resources中的往来询证函模板
                    FileStream outputExcelFile = new FileStream(strPath + "\\HertZTemplate\\往来询证函模板.dotx", FileMode.Create, FileAccess.Write); //存到我的文档
                    outputExcelFile.Write(sampleCA, 0, sampleCA.Length);
                    outputExcelFile.Close();
                }

                //创建公司名字典，如果有重复的添加编号
                string ItemID = "";
                Dictionary<string, int> CompanyDict = new Dictionary<string, int> { };
                Dictionary<string, string> VariablesDict = new Dictionary<string, string> { };
                Dictionary<string, string> BaseDict = new Dictionary<string, string> { };
                BaseDict.Add("AccountingFirmName", AccountingFirmName);
                BaseDict.Add("OurCompany", OurCompany);
                BaseDict.Add("ReplyAddress", ReplyAddress);
                BaseDict.Add("PostalCode", PostalCode);
                BaseDict.Add("AuditDeadline", AuditDeadline);
                BaseDict.Add("Contact", Contact);
                BaseDict.Add("Telephone", Telephone);
                BaseDict.Add("Department", Department);
                BaseDict.Add("Leading", Leading);

                //生成函证编号
                if (OurCompany.Length > 3)
                {
                    for (int i1 = 1; i1 < 5; i1++)
                    {
                        ItemID += FunC.GetSpellCode(OurCompany.Substring(i1 - 1, 1));
                    }
                }
                else
                {
                    ItemID = "HertZ";
                }
                //读取审计截止日做函证编号
                ItemID = string.Format("{0}-{1}-", AuditDeadline.Substring(0, Math.Min(AuditDeadline.Length, 4)), ItemID);

                for (int i = 2; i <= AllRows; i++)
                {

                    //如果客户名称列为空则跳过
                    TempStr = FunC.TS(ORG[i, ColDict["[客户名称]"]]);
                    if (TempStr == "") { break; }
                    //新建模板
                    WordDoc = WordApp.Documents.Add(strPath + "\\HertZTemplate\\往来询证函模板.dotx");

                    //用于防止存在重复的公司名导致文件无法保存
                    if (CompanyDict.ContainsKey(TempStr))
                    {
                        CompanyDict[TempStr] = CompanyDict[TempStr] + 1;
                    }
                    else
                    {
                        CompanyDict[TempStr] = 0;
                    }

                    //编号
                    WordDoc.Variables.Add("Number", ItemID + i);
                    foreach (string ColName in ColDict.Keys)
                    {
                        TempStr = FunC.TS(ORG[i, ColDict[ColName]]);
                        if(TempStr == "")
                        {
                            TempStr = " ";
                        }
                        else if (FunC.IsNumber(TempStr))
                        {
                            TempStr = String.Format("{0:N}", FunC.TD(TempStr, 0d, 2));
                        }
                        WordDoc.Variables.Add(ColName, TempStr);
                    }
                    foreach (string ColName in BaseDict.Keys)
                    {
                        WordDoc.Variables.Add(ColName, BaseDict[ColName]);
                    }

                    //更新域
                    WordDoc.Fields.Update();
                    //另存为
                    TempStr = FunC.TS(ORG[i, ColDict["[客户名称]"]]);
                    if(CompanyDict[TempStr] != 0)
                    {
                        TempStr += CompanyDict[TempStr];
                    }
                    WordDoc.SaveAs2(string.Format("{0}\\{1}\\{2}.docx", FolderPath, OurCompany, TempStr));
                    WordDoc.Close();
                }

                //WordApp.Visible = true;//使文档可见
                WordApp.Quit();
                MessageBox.Show("函证文件生成成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("运行出错，可以截图联系作者（微信hewish_cn）帮助优化该程序" + Environment.NewLine + ex.Message);
            }

        }

        //往来款加工设置
        private void CurrentAccountSetting_Click(object sender, RibbonControlEventArgs e)
        {
            Form CASetting = new CASetting
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            CASetting.Show();
        }

        //填充空单元格
        private void AutoFillInTheBlanks_Click(object sender, RibbonControlEventArgs e)
        {
            int AllRows;
            int AllColumns;
            int StartColumn;
            object[,] ORG;//原始数组ORG
            object[,] NRG;//新数组NRG

            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }
            if(rg.Count == 1) { return; }
            ORG = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = ORG.GetLength(1);
            AllColumns = Math.Min(AllColumns, FunC.AllColumns(rg.Row) - rg.Column + 1);
            AllColumns = Math.Max(1, AllColumns);

            //取所选区域的前后3列最大行数
            StartColumn = Math.Max(1, rg.Column - 3);
            AllRows = FunC.AllRows(FunC.CName(StartColumn), AllColumns + 6) - rg.Row +1;
            AllRows = Math.Min(AllRows, ORG.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            NRG = new object[AllRows, AllColumns];

            for(int i = 1; i <= AllColumns; i++)
            {
                NRG[0, i - 1] = ORG[1, i];
                for (int i1 = 2; i1 <= AllRows; i1++)
                {
                    if(ORG[i1, i] != null)
                    {
                        NRG[i1 - 1, i - 1] = ORG[i1, i];
                    }
                    else
                    {
                        NRG[i1 - 1, i - 1] = NRG[i1 - 2, i - 1];
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORG = null;
            NRG = null;
        }

        //对比两列数
        private void CompareTwoColumns_Click(object sender, RibbonControlEventArgs e)
        {
            int AllRows;
            string SelectColomn;
            string SelectColomn2;
            object[,] ORG;//原始数组ORG
            object[,] NRG;//新数组NRG
            string[,] ARG;//计算用数组

            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;
            //WBK = ExcelApp.ActiveWorkbook;

            //选择第一列，并捕获用户 直接点击取消 的情况
            try
            {
                SelectColomn = FunC.CName(ExcelApp.InputBox(Prompt: "请选择第一列", Type: 8).Column);
            }
            catch
            {
                return;
            }

            //获取所选列的行数
            AllRows = FunC.AllRows(SelectColomn);
            //如果行数为1，则终止程序
            if (AllRows < 2)
            {
                MessageBox.Show("所选列行数小于2，请重新开始");
                return;
            }

            //将所选列的赋值至数组
            ORG = WST.Range[SelectColomn + "1:" + SelectColomn + AllRows].Value2;

            //选择第二列，并捕获用户 直接点击取消 的情况
            try
            {
                SelectColomn2 = FunC.CName(ExcelApp.InputBox(Prompt: "请选择第二列", Type: 8).Column);
            }
            catch
            {
                return;
            }

            //获取所选列的行数
            AllRows = FunC.AllRows(SelectColomn2);
            //如果行数为1，则终止程序
            if (AllRows < 2)
            {
                MessageBox.Show("所选列行数小于2，请重新开始");
                return;
            }

            //将所选列的赋值至数组
            NRG = WST.Range[SelectColomn2 + "1:" + SelectColomn2 + AllRows].Value2;

            ExcelApp.ScreenUpdating = false;//关闭Excel视图刷新

            AllRows = Math.Max(ORG.GetLength(0), NRG.GetLength(0));
            ARG = new string[AllRows, 2];
            //将数组org存入
            for (int i = 1; i <= ORG.GetLength(0); i++)
            {
                if (ORG[i, 1] != null)
                {
                    ARG[i - 1, 0] = FunC.TS(ORG[i, 1]);
                }
                else
                {
                    ARG[i - 1, 0] = "0";
                }
            }
            ORG = null;//释放数组
            //将数组nrg存入
            for (int i = 1; i <= NRG.GetLength(0); i++)
            {
                if (NRG[i, 1] != null)
                {
                    ARG[i - 1, 1] = FunC.TS(NRG[i, 1]);
                }
                else
                {
                    ARG[i - 1, 1] = "0";
                }
            }
            NRG = null;//释放数组

            //重新定义NRG存放计算过程
            int[,] Arr = new int[AllRows, 8];

            //计算是否重复出现
            for (int i = 0; i < AllRows; i++)
            {
                for (int i1 = 0; i1 < AllRows; i1++)
                {
                    //Arr第三列表示第一列中重复的次数
                    if (ARG[i, 0] != null && ARG[i, 0] == ARG[i1, 0])
                    {
                        Arr[i, 2] = Arr[i, 2] + 1;
                    }

                    //Arr第四列表示第二列中重复的次数
                    if (ARG[i, 1] != null && ARG[i, 1] == ARG[i1, 1])
                    {
                        Arr[i, 3] = Arr[i, 3] + 1;
                    }

                    //第五列表示第一列数在第二列中出现的次数
                    if (ARG[i, 0] != null && ARG[i, 0] == ARG[i1, 1])
                    {
                        Arr[i, 4] = Arr[i, 4] + 1;
                    }

                    //第六列表示第二列数在第一列中出现的次数
                    if (ARG[i, 1] != null && ARG[i, 1] == ARG[i1, 0])
                    {
                        Arr[i, 5] = Arr[i, 5] + 1;
                    }
                }
            }

            for (int i = 0; i < AllRows; i++)
            {
                for (int i1 = 0; i1 < AllRows; i1++)
                {
                    //第七列为第二列中是否存在与第一列数相同的值
                    if (ARG[i, 0] == ARG[i1, 1] && Arr[i, 2] == Arr[i, 4])
                    {
                        Arr[i, 6] = 1;
                    }

                    //第八列为第一列中是否存在与第二列数相同的值
                    if (ARG[i, 1] == ARG[i1, 0] && Arr[i, 3] == Arr[i, 5])
                    {
                        Arr[i, 7] = 1;
                    }
                }
            }

            //调整单元格颜色
            int TempInt = FunC.CNumber(SelectColomn2);
            //调整第二列的格式
            WST.Columns[SelectColomn2 + ":" + SelectColomn2].Interior.ColorIndex = 0;
            for (int i = 0; i < AllRows; i++)
            {
                if (Arr[i, 7] != 1 && ARG[i, 1] != null && ARG[i, 1] != "0")
                {
                    WST.Cells[i + 1, TempInt].Interior.Color = Color.Yellow;
                    //CellsRange = CellsRange + "," + SelectColomn2 + (i+1);
                }
            }
            TempInt = FunC.CNumber(SelectColomn);
            //调整第一列的格式
            WST.Columns[SelectColomn + ":" + SelectColomn].Interior.ColorIndex = 0;
            
            for (int i = 0; i < AllRows; i++)
            {
                if (Arr[i, 6] != 1 && ARG[i, 0] != null && ARG[i, 0] != "0")
                {
                    WST.Cells[i + 1, TempInt].Interior.Color = Color.Yellow;
                    //CellsRange = CellsRange + "," + SelectColomn + (i + 1);
                }
            }
            ExcelApp.ScreenUpdating = true;//打开Excel视图刷新
        }

        //另存为xlsx文件
        private void Exportxlsx_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            string OFullName = ExcelApp.ActiveWorkbook.FullName;
            string NFullName;
            //检查是否为xlsx文件
            if(Path.GetExtension(OFullName).Substring(1).ToLower() == "xlsx") { return; }

            NFullName = Path.Combine(Path.GetDirectoryName(OFullName), Path.GetFileNameWithoutExtension(OFullName) + ".xlsx");
            //检查是否已存在xlsx文件
            if (File.Exists(NFullName))
            {
                DialogResult dr = MessageBox.Show("当前路径已存在同名工作表，是否删除并继续？", "请选择", MessageBoxButtons.YesNo);
                if (dr != DialogResult.Yes){ return; }
                try
                {
                    File.Delete(NFullName);
                }
                catch
                {
                    MessageBox.Show("删除文件失败！请关闭文件后重试");
                    return;
                }
            }

            bool DeleteFile = false;
            if (Path.GetExtension(OFullName).Substring(1).ToLower() == "xls") { DeleteFile = true; }

            //另存为
            ExcelApp.ActiveWorkbook.SaveAs(NFullName, FileFormat:Excel.XlFileFormat.xlOpenXMLWorkbook, CreateBackup:false);

            //如果是xls文件，则删除文件
            if (DeleteFile) { File.Delete(OFullName); }
        }

        //修改正负号
        private void ChangeSign_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            string TempStr;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Text != null && rg.Text != "0" && FunC.IsNumber(rg.Value))
                {
                    TempStr = rg.Formula;
                    if (TempStr.Substring(0, 1) == "=")
                    {
                        if (FunC.AddParen(TempStr))
                        {
                            rg.Formula = "=-(" + TempStr.Substring(1) + ")";
                        }
                        else
                        {
                            if(TempStr.Substring(1, 1) == "-")
                            {
                                if(TempStr.Substring(2, 1) == "(" && TempStr.Substring(TempStr.Length -1) == ")")
                                {
                                    try
                                    {
                                        rg.Formula = "=" + TempStr.Substring(3, TempStr.Length - 4);
                                    }
                                    catch
                                    {
                                        rg.Formula = "=" + TempStr.Substring(2);
                                    }
                                }
                                else
                                {
                                    rg.Formula = "=" + TempStr.Substring(2);
                                }
                            }
                            else
                            {
                                rg.Formula = "=-" + TempStr.Substring(1);
                            }
                        }
                    }
                    else
                    {
                        rg.Value2 = -double.Parse(rg.Value);
                    }
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGf;//原始数组ORGf 读取公式
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGf = rg.Formula;
            ORGv = rg.Value;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column))+10) - rg.Column +1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数
            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row +1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    //如果非空且是数字
                    if (ORGv[i1, i] != null && FunC.IsNumber(FunC.TS(ORGv[i1, i])))
                    {
                        if (Math.Abs(FunC.TD(ORGv[i1, i])) < p)
                        {
                            NRG[i1 - 1, i - 1] = ORGf[i1, i];
                        }
                        else
                        {
                            TempStr = FunC.TS(ORGf[i1, i]);
                            if (TempStr.Substring(0,1) == "=")
                            {
                                if(FunC.AddParen(TempStr))
                                {
                                    NRG[i1 - 1, i - 1] = "=-(" + TempStr.Substring(1) + ")";
                                }
                                else
                                {
                                    if (TempStr.Substring(1, 1) == "-")
                                    {
                                        if (TempStr.Substring(2, 1) == "(" && TempStr.Substring(TempStr.Length - 1) == ")")
                                        {
                                            try
                                            {
                                                NRG[i1 - 1, i - 1] = "=" + TempStr.Substring(3, TempStr.Length - 4);
                                            }
                                            catch
                                            {
                                                NRG[i1 - 1, i - 1] = "=" + TempStr.Substring(2);
                                            }
                                        }
                                        else
                                        {
                                            NRG[i1 - 1, i - 1] = "=" + TempStr.Substring(2);
                                        }
                                    }
                                    else
                                    {
                                        NRG[i1 - 1, i - 1] = "=-" + TempStr.Substring(1);
                                    }
                                }
                            }
                            else
                            {
                                NRG[i1 - 1, i - 1] = -double.Parse(FunC.TS(ORGv[i1, i], "0"));
                            }
                        }
                    }
                    else
                    {
                        NRG[i1 - 1, i - 1] = ORGf[i1, i];
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGf = null;
            ORGv = null;
            NRG = null;
        }

        //加Round
        private void RoundButton_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Round中读取配置名为Num的值，默认为2
            int RoundNum = clsConfig.ReadConfig<int>("Round", "Num", 2);

            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            string TempStr;
            string HeadStr;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //改选区格式
            TempStr = "#,##0.00";
            if (RoundNum > 2) 
            { 
                for(int i =3;i<= RoundNum; i++)
                {
                    TempStr += "0";
                }
            }
            rg.NumberFormatLocal = TempStr;

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Text != null && FunC.IsNumber(rg.Text))
                {
                    TempStr = rg.Formula;
                    if (TempStr.Substring(0, 1) == "=")
                    {
                        if (FunC.AddParen(TempStr))
                        {
                            rg.Formula = "=ROUND(" + TempStr.Substring(1) + ","+ RoundNum + ")";
                        }
                        else
                        {
                            if (TempStr.Substring(0, 2) == "=-")
                            {
                                TempStr = TempStr.Substring(2);
                                HeadStr = "=-";
                            }
                            else
                            {
                                TempStr = TempStr.Substring(1);
                                HeadStr = "=";
                            }

                            if (TempStr.Substring(0, 1) == "(" && TempStr.Substring(TempStr.Length - 1) == ")")
                            {
                                rg.Formula = HeadStr + "ROUND" + TempStr.Substring(0,TempStr.Length-1) + "," + RoundNum + ")";
                            }
                            else if (TempStr.Length > 6 && TempStr.Substring(0, 6) == "ROUND(" && TempStr.Substring(TempStr.Length - 1) == ")")
                            {
                                rg.Formula = HeadStr + TempStr.Substring(0, TempStr.LastIndexOf(',') + 1) + RoundNum + ")";
                            }
                            else
                            {
                                rg.Formula = HeadStr + "ROUND(" + TempStr + "," + RoundNum + ")";
                            }
                            
                        }
                    }
                    else
                    {
                        rg.Value2 = "=ROUND(" + rg.Formula+ "," + RoundNum + ")";
                    }
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGf;//原始数组ORGf 读取公式
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGf = rg.Formula;
            ORGv = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column +1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数
            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns)- rg.Row+1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    //如果非空且是数字
                    if (ORGv[i1, i] != null && FunC.IsNumber(FunC.TS(ORGv[i1, i])))
                    {
                        TempStr = FunC.TS(ORGf[i1, i]);
                        if (TempStr.Substring(0, 1) == "=")
                        {

                            if (FunC.AddParen(TempStr))
                            {
                                NRG[i1 - 1, i - 1] = "=ROUND(" + TempStr.Substring(1) + "," + RoundNum + ")";
                            }
                            else
                            {
                                if (TempStr.Substring(0, 2) == "=-")
                                {
                                    TempStr = TempStr.Substring(2);
                                    HeadStr = "=-";
                                }
                                else
                                {
                                    TempStr = TempStr.Substring(1);
                                    HeadStr = "=";
                                }

                                if (TempStr.Substring(0, 1) == "(" && TempStr.Substring(TempStr.Length - 1) == ")")
                                {
                                    NRG[i1 - 1, i - 1] = HeadStr + "ROUND" + TempStr.Substring(0, TempStr.Length - 1) + "," + RoundNum + ")";
                                }
                                else if (TempStr.Length > 6 && TempStr.Substring(0, 6) == "ROUND(" && TempStr.Substring(TempStr.Length - 1) == ")")
                                {
                                    NRG[i1 - 1, i - 1] = HeadStr + TempStr.Substring(0, TempStr.LastIndexOf(',') + 1) + RoundNum + ")";
                                }
                                else
                                {
                                    NRG[i1 - 1, i - 1] = HeadStr + "ROUND(" + TempStr + "," + RoundNum + ")";
                                }
                            }
                        }
                        else
                        {
                            NRG[i1 - 1, i - 1] = "=ROUND(" + TempStr + "," + RoundNum + ")"; 
                        }
                    }
                    else
                    {
                        NRG[i1 - 1, i - 1] = ORGf[i1, i];
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGf = null;
            ORGv = null;
            NRG = null;
        }

        //设置小数位数
        private void RoundSetting_Click(object sender, RibbonControlEventArgs e)
        {
            Form RoundSetting = new RoundSetting
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            RoundSetting.Show();
        }

        //去除Round函数
        private void NoRound_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            string TempStr;
            string HeadStr;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Text != null && FunC.IsNumber(rg.Text))
                {
                    TempStr = rg.Formula;
                    if (TempStr.Substring(0, 1) == "=")
                    {
                        if (!FunC.AddParen(TempStr))
                        {
                            if (TempStr.Substring(0, 2) == "=-")
                            {
                                TempStr = TempStr.Substring(2);
                                HeadStr = "=-";
                            }
                            else
                            {
                                TempStr = TempStr.Substring(1);
                                HeadStr = "=";
                            }

                            if (TempStr.Length > 6 && TempStr.Substring(0, 6) == "ROUND(" && TempStr.Substring(TempStr.Length - 1) == ")")
                            {
                                TempStr = TempStr.Substring(6, TempStr.LastIndexOf(',')-6);
                                if (FunC.IsNumber(TempStr))
                                {
                                    if(HeadStr == "=-")
                                    {
                                        rg.Value2 = "-" + TempStr;
                                    }
                                    else
                                    {
                                        rg.Value2 = TempStr;
                                    }
                                }
                                else
                                {
                                    if (FunC.AddParen("=" + TempStr) && HeadStr == "=-")
                                    {
                                        rg.Formula = HeadStr + "(" + TempStr + ")";
                                    }
                                    else
                                    {
                                        rg.Formula = HeadStr + TempStr;
                                    }
                                    rg.Formula = HeadStr + TempStr;
                                }
                            }
                        }
                    }
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGf;//原始数组ORGf 读取公式
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGf = rg.Formula;
            ORGv = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column+1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数
            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row+1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    //如果非空且是数字
                    if (ORGv[i1, i] != null && FunC.IsNumber(FunC.TS(ORGv[i1, i])))
                    {
                        TempStr = FunC.TS(ORGf[i1, i]);
                        if (TempStr.Substring(0, 1) == "=" && !FunC.AddParen(TempStr))
                        {
                            if (TempStr.Substring(0, 2) == "=-")
                            {
                                TempStr = TempStr.Substring(2);
                                HeadStr = "=-";
                            }
                            else
                            {
                                TempStr = TempStr.Substring(1);
                                HeadStr = "=";
                            }

                            if (TempStr.Length > 6 && TempStr.Substring(0, 6) == "ROUND(" && TempStr.Substring(TempStr.Length - 1) == ")")
                            {
                                TempStr = TempStr.Substring(6, TempStr.LastIndexOf(',') - 6);
                                if (FunC.IsNumber(TempStr))
                                {
                                    if(HeadStr == "=-")
                                    {
                                        NRG[i1 - 1, i - 1] = "-" + TempStr;
                                    }
                                    else
                                    {
                                        NRG[i1 - 1, i - 1] = TempStr;
                                    }
                                }
                                else
                                {
                                    if(FunC.AddParen("=" + TempStr) && HeadStr == "=-")
                                    {
                                        NRG[i1 - 1, i - 1] = HeadStr +"("+ TempStr+")";
                                    }
                                    else
                                    {
                                        NRG[i1 - 1, i - 1] = HeadStr + TempStr;
                                    }
                                }
                            }
                            else
                            {
                                NRG[i1 - 1, i - 1] = ORGf[i1, i];
                            }
                            
                        }
                        else
                        {
                            NRG[i1 - 1, i - 1] = ORGf[i1, i];
                        }
                    }
                    else
                    {
                        NRG[i1 - 1, i - 1] = ORGf[i1, i];
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGf = null;
            ORGv = null;
            NRG = null;
        }

        //检查非数字单元格
        private void CheckNum_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;//Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;
            Excel.Range rg = ExcelApp.Selection;
            if(rg.Count == 1) 
            {
                if(rg.Value2 != null && !FunC.IsNumber(FunC.TS(rg.Value2)))
                {
                    WST.Range[FunC.CName(rg.Column) + rg.Row].Interior.Color = Color.Yellow;
                }
                return;
            }

            //限制列数，防止选择整行时多余的计算
            int AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, rg.Columns.Count);
            AllColumns = Math.Max(1, AllColumns);

            //限制行数
            int AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, rg.Rows.Count);
            AllRows = Math.Max(1, AllRows);

            string rgTostring = FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns -1 ) + (rg.Row + AllRows-1);
            rg = null;
            FunC.ColorNotNum(rgTostring);
        }

        //日期格式
        private void DateFormate_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;//Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;
            Excel.Range rg = ExcelApp.Selection;
            rg.NumberFormatLocal = "yyyy/mm/dd";

            //限制列数，防止选择整行时多余的计算
            int AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, rg.Columns.Count);
            AllColumns = Math.Max(1, AllColumns);

            //限制行数
            int AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, rg.Rows.Count);
            AllRows = Math.Max(1, AllRows);

            int[] myField = { 1, 5 };
            for (int i = rg.Column;i< rg.Column + AllColumns; i++)
            {
                try
                {
                    WST.Range[FunC.CName(i) + rg.Row + ":" + FunC.CName(i) + (rg.Row + AllRows - 1)].TextToColumns(DataType: Excel.XlTextParsingType.xlDelimited, TextQualifier: Excel.XlTextQualifier.xlTextQualifierDoubleQuote, ConsecutiveDelimiter: false, FieldInfo: myField, TrailingMinusNumbers: true);
                }
                catch{}
            }
        }

        //万元格式
        private void TenThousand_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            string TempStr;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Text != null && FunC.IsNumber(rg.Text))
                {
                    TempStr = rg.Formula;
                    if (TempStr.Substring(0, 1) == "=")
                    {
                        if (!FunC.AddParen(TempStr))
                        {
                            rg.Formula = TempStr + "/10000";
                        }
                        else
                        {
                            rg.Formula = "=(" + TempStr.Substring(1) + ")/10000";
                        }
                    }
                    else
                    {
                        if(rg.Text != "0") { rg.Value2 = double.Parse(TempStr) / 10000; }
                    }
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGf;//原始数组ORGf 读取公式
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGf = rg.Formula;
            ORGv = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数

            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    //如果非空且是数字
                    if (ORGv[i1, i] != null  && FunC.IsNumber(FunC.TS(ORGv[i1, i])))
                    {
                        TempStr = FunC.TS(ORGf[i1, i]);
                        if (TempStr.Substring(0, 1) == "=")
                        {
                            if (!FunC.AddParen(TempStr))
                            {
                                NRG[i1 - 1, i - 1] = TempStr + "/10000";
                            }
                            else
                            {
                                NRG[i1 - 1, i - 1] = "=(" + TempStr.Substring(1) + ")/10000";
                            }
                        }
                        else
                        {
                            if(FunC.TS(ORGv[i1, i]) != "0")
                            {
                                NRG[i1 - 1, i - 1] = double.Parse(FunC.TS(ORGv[i1, i])) / 10000;
                            }
                            else
                            {
                                NRG[i1 - 1, i - 1] = ORGf[i1, i];
                            }
                        }
                    }
                    else
                    {
                        NRG[i1 - 1, i - 1] = ORGf[i1, i];
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGf = null;
            ORGv = null;
            NRG = null;
        }

        //乘一万，去除万元格式
        private void NoTenThousand_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            string TempStr;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Text != null && FunC.IsNumber(rg.Text))
                {
                    TempStr = rg.Formula;
                    if (TempStr.Substring(0, 1) == "=")
                    {
                        if (!FunC.AddParen(TempStr))
                        {
                            if(TempStr.Length > 7 && TempStr.Substring(TempStr.Length-6) == "/10000")
                            {
                                if(TempStr.Substring(1,1) == "(" && TempStr.Substring(TempStr.Length - 7, 1) == ")")
                                {
                                    rg.Formula = "=" + TempStr.Substring(2, TempStr.Length - 9);
                                }
                                else
                                {
                                    TempStr = TempStr.Substring(0, TempStr.Length - 6);
                                    if (FunC.IsNumber(TempStr.Substring(1)))
                                    {
                                        rg.Formula = TempStr.Substring(1);
                                    }
                                    else
                                    {
                                        rg.Formula = TempStr;
                                    }
                                }
                            }
                            else
                            {
                                rg.Formula = TempStr + "*10000";
                            }
                        }
                        else
                        {
                            rg.Formula = "=(" + TempStr.Substring(1) + ")*10000";
                        }
                    }
                    else
                    {
                        rg.Formula = double.Parse(TempStr) * 10000;
                    }
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGf;//原始数组ORGf 读取公式
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGf = rg.Formula;
            ORGv = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数

            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    //如果非空且是数字
                    if (ORGv[i1, i] != null  && FunC.IsNumber(FunC.TS(ORGv[i1, i])))
                    {
                        TempStr = FunC.TS(ORGf[i1, i]);
                        if (TempStr.Substring(0, 1) == "=")
                        {
                            if (!FunC.AddParen(TempStr))
                            {
                                if (TempStr.Length > 7 && TempStr.Substring(TempStr.Length - 6) == "/10000")
                                {
                                    if (TempStr.Substring(1, 1) == "(" && TempStr.Substring(TempStr.Length - 7, 1) == ")")
                                    {
                                        NRG[i1 - 1, i - 1] = "=" + TempStr.Substring(2, TempStr.Length - 9);
                                    }
                                    else
                                    {
                                        TempStr = TempStr.Substring(0, TempStr.Length - 6);
                                        if (FunC.IsNumber(TempStr.Substring(1)))
                                        {
                                            NRG[i1 - 1, i - 1] = TempStr.Substring(1);
                                        }
                                        else
                                        {
                                            NRG[i1 - 1, i - 1] = TempStr;
                                        }
                                    }
                                }
                                else
                                {
                                    NRG[i1 - 1, i - 1] = TempStr + "*10000";
                                }
                            }
                            else
                            {
                                NRG[i1 - 1, i - 1] = "=(" + TempStr.Substring(1) + ")*10000";
                            }
                        }
                        else
                        {
                            NRG[i1 - 1, i - 1] = double.Parse(FunC.TS(ORGv[i1, i],"0")) * 10000;
                        }
                    }
                    else
                    {
                        NRG[i1 - 1, i - 1] = ORGf[i1, i];
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGf = null;
            ORGv = null;
            NRG = null;
        }

        //锁定全部工作表
        private void ProtectBook_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Protect中读取配置名为Password的值，默认为Password
            string Password = clsConfig.ReadConfig<string>("Protect", "Password", "Password");

            ExcelApp = Globals.ThisAddIn.Application;

            foreach (Excel.Worksheet wst in ExcelApp.Worksheets)
            {
                wst.Protect(Password);
            }

            MessageBox.Show("已将所有工作表添加保护！");
        }

        //锁定当前工作表
        private void ProtectSheet_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //检查是否已被保护
            if (WST.ProtectContents) { MessageBox.Show("当前工作表已被锁定！"); return; }

            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Protect中读取配置名为Password的值，默认为Password
            string Password = clsConfig.ReadConfig<string>("Protect", "Password", "Password");

            WST.Protect(Password);

            MessageBox.Show("已为当前工作表添加保护！");
        }

        //锁定选中单元格
        private void ProtectRange_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Protect中读取配置名为Password的值，默认为Password
            string Password = clsConfig.ReadConfig<string>("Protect", "Password", "Password");

            //检查是否已被保护
            if (WST.ProtectContents)
            {
                if ((int)MessageBox.Show("当前单元格已被锁定，是否先解除锁定？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == 1)
                {
                    try
                    {
                        WST.Unprotect(Password);
                    }
                    catch
                    {
                        if ((int)MessageBox.Show("密码错误，是否暴力解除锁定？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == 1)
                        {

                            //强制解锁
                            try
                            {
                                WST.Protect(AllowFiltering: true);
                                WST.Unprotect();
                            }
                            catch
                            {
                                MessageBox.Show("暴力解锁失败！"); return;
                            }

                            //检查是否解锁成功
                            if (WST.ProtectContents) { MessageBox.Show("暴力解锁失败！"); return; }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }

            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                MessageBox.Show("未选中单元格");
                return;
            }

            //加保护
            WST.Cells.Locked = false;
            rg.SpecialCells(Excel.XlCellType.xlCellTypeVisible).Locked = true;
            WST.Protect(Password, true, true, true, AllowFormattingCells: true, AllowFormattingColumns: true, AllowFormattingRows: true,AllowInsertingRows:true,AllowDeletingRows:true,AllowSorting:true,AllowFiltering:true,AllowUsingPivotTables:true);

        }

        //解锁当前工作簿中的全部工作表
        private void UnlockBook_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Protect中读取配置名为Password的值，默认为Password
            string Password = clsConfig.ReadConfig<string>("Protect", "Password", "Password");

            //解除锁定
            foreach (Excel.Worksheet wst in ExcelApp.Worksheets)
            {
                if (wst.ProtectContents)
                {
                    try
                    {
                        wst.Unprotect(Password);
                    }
                    catch
                    {
                        //强制解锁
                        try
                        {
                            wst.Protect(AllowFiltering: true);
                            wst.Unprotect();
                        }
                        catch
                        {
                            MessageBox.Show("暴力解锁失败，请尝试其他方式！");
                            return;
                        }
                    }
                }
            }

            MessageBox.Show("解锁完成，请检查！");
        }

        //解锁当前工作簿
        private void UnlockSheet_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Protect中读取配置名为Password的值，默认为Password
            string Password = clsConfig.ReadConfig<string>("Protect", "Password", "Password");

            if (WST.ProtectContents)
            {
                try
                {
                    WST.Unprotect(Password);
                    MessageBox.Show("已解除当前工作表锁定！");
                }
                catch
                {
                    if ((int)MessageBox.Show("密码错误，是否暴力解除锁定？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == 1)
                    {
                        //强制解锁
                        try
                        {
                            WST.Protect(AllowFiltering: true);
                            WST.Unprotect();
                        }
                        catch
                        {
                            MessageBox.Show("暴力解锁失败！"); return;
                        }

                        //检查是否解锁成功
                        if (WST.ProtectContents)
                        {
                            MessageBox.Show("暴力解锁失败！");
                        }
                        else
                        {
                            MessageBox.Show("已解除当前工作表锁定！");
                        }
                    }
                }
            }
        }

        //设置密码
        private void ProtectSetting_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取配置
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            //从父节点Protect中读取配置名为Password的值，默认为Password
            string Password = clsConfig.ReadConfig<string>("Protect", "Password", "Password");

            //定义Excelapp
            ExcelApp = Globals.ThisAddIn.Application;

            //输入密码
            try
            {
                string NewPassword = ExcelApp.InputBox("请输入默认密码", "输入密码", Password, Type: 1 + 2);
                Regex rg = new Regex("^[a-z0-9A-Z]+$");
                if (rg.IsMatch(NewPassword))
                {
                    if (NewPassword != Password)
                    {
                        clsConfig.WriteConfig("Protect", "Password", NewPassword);
                    }
                }
                else
                {
                    MessageBox.Show("请使用数字及字母组合！");
                }
            }
            catch
            {
            }
        }

        //版本信息
        private void VersionInfo_Click(object sender, RibbonControlEventArgs e)
        {
            Form InfoForm = new VerInfo
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            InfoForm.Show();
        }

        //账表加工 组 显示
        private void TableProcessingCheck_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            if (TableProcessingCheck.Checked)
            {
                TableProcessing.Visible = true;
                clsConfig.WriteConfig("GlobalSetting", "TableProcessingCheck", true.ToString());
            }
            else
            {
                TableProcessing.Visible = false;
                clsConfig.WriteConfig("GlobalSetting", "TableProcessingCheck", false.ToString());
            }
        }

        //久其 组 显示
        private void JiuQiCheck_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            if (JiuQiCheck.Checked)
            {
                JiuQi.Visible = true;
                clsConfig.WriteConfig("GlobalSetting", "JiuQiCheck", true.ToString());
            }
            else
            {
                JiuQi.Visible = false;
                clsConfig.WriteConfig("GlobalSetting", "JiuQiCheck", false.ToString());
            }
        }

        //工具 组 显示
        private void ToolCheck_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            if (ToolCheck.Checked)
            {
                Tool.Visible = true;
                clsConfig.WriteConfig("GlobalSetting", "ToolCheck", true.ToString());
            }
            else
            {
                Tool.Visible = false;
                clsConfig.WriteConfig("GlobalSetting", "ToolCheck", false.ToString());
            }
        }

        //保护 组 显示
        private void ProtectCheck_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            if (ProtectCheck.Checked)
            {
                Protect.Visible = true;
                clsConfig.WriteConfig("GlobalSetting", "ProtectCheck", true.ToString());
            }
            else
            {
                Protect.Visible = false;
                clsConfig.WriteConfig("GlobalSetting", "ProtectCheck", false.ToString());
            }
        }

        //加工久其表
        private void EditJiuQi_Click(object sender, RibbonControlEventArgs e)
        {
            
        }

        //生成附注Word
        private void ExportNotes_Click(object sender, RibbonControlEventArgs e)
        {
            
        }

        /// <summary>
        /// 打开附注模板文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenNoteTemplate_Click(object sender, RibbonControlEventArgs e)
        {
            
        }

        /// <summary>
        /// 打开模板文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFloder_Click(object sender, RibbonControlEventArgs e)
        {
            string TempStr = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            TempStr += "\\HertZTemplate";

            if (Directory.Exists(TempStr))
            {
                System.Diagnostics.Process.Start("explorer.exe", TempStr);
            }
            else
            {
                System.Diagnostics.Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
            }
        }

        /// <summary>
        /// 生成索引表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MakeIndex_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;

            //索引表名
            string SheetNameStr = "索引";
            if (FunC.SheetExist(SheetNameStr))
            {
                for(int i = 1; i < 11; i++)
                {
                    if (!FunC.SheetExist(SheetNameStr + i))
                    {
                        SheetNameStr = SheetNameStr + i;
                        break;
                    }
                }
                if(SheetNameStr == "索引")
                {
                    MessageBox.Show("已存在多个“索引”表，请删除或重命名后再试");
                    return;
                }
            }

            ExcelApp.ScreenUpdating = false;//关闭屏幕刷新

            //创建表名列表
            string[,] NameArr = new string[ExcelApp.ActiveWorkbook.Worksheets.Count,1];
            int TempInt = 0;
            //为每一张表添加首行返回链接，并将表名加入字典
            foreach (Excel.Worksheet wst in ExcelApp.ActiveWorkbook.Worksheets)
            {
                wst.Range["1:1"].Insert();
                wst.Range["A1"].Value2 = "//点击返回索引";
                wst.Hyperlinks.Add(wst.Range["A1"], "", SheetNameStr + "!A1");
                NameArr[TempInt,0] = wst.Name;
                TempInt++;
            }

            //添加新表
            WST = (Excel.Worksheet)ExcelApp.ActiveWorkbook.Worksheets.Add(ExcelApp.ActiveWorkbook.Worksheets[1]);
            WST.Name = SheetNameStr;

            //添加表头并居中
            WST.Range["A1"].Value2 = "表名";
            WST.Range["A1:B1"].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            //添加内容并靠左对齐
            WST.Range["A2:A" + (NameArr.Length + 1)].Value2 = NameArr;
            WST.Range["A2:B" + (NameArr.Length + 1)].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            for(int i = 2;i<= NameArr.Length + 1; i++)
            {
                WST.Hyperlinks.Add(WST.Range["A" + i], "", NameArr[i - 2, 0] + "!A1");
            }

            ExcelApp.ScreenUpdating = true;//打开屏幕刷新

            MessageBox.Show("索引表已生成！");
        }

        /// <summary>
        /// 删除首行索引链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteFirstRow_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;

            ExcelApp.ScreenUpdating = false;//关闭屏幕刷新

            //遍历表格删除首行
            foreach (Excel.Worksheet wst in ExcelApp.ActiveWorkbook.Worksheets)
            {
                if(FunC.TS(wst.Range["A1"].Value2) == "//点击返回索引")
                {
                    wst.Range["1:1"].Delete();
                }
            }

            ExcelApp.ScreenUpdating = true;//打开屏幕刷新
        }

        /// <summary>
        /// 批量修改表名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeName_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //检查表名
            if (WST.Name.Length < 2 || WST.Name.Substring(0,2) != "索引")
            {
                DialogResult IsWait = MessageBox.Show("请在索引表中使用该功能！（如未生成索引请先点击生成）" + Environment.NewLine + "是否继续？", "请选择", MessageBoxButtons.YesNo);
                if (IsWait != DialogResult.Yes) { return; }
            }

            //检查表行
            if(FunC.AllRows("B") < 2)
            {
                DialogResult IsWait = MessageBox.Show("请保持首列为当前表名，并在第二列添加新表名！" + Environment.NewLine + "是否继续？", "请选择", MessageBoxButtons.YesNo);
                if (IsWait != DialogResult.Yes) { return; }
            }

            int AllRows = FunC.AllRows();

            ExcelApp.ScreenUpdating = false;

            //读取有效区域
            object[,] ORG = WST.Range["A1:B" + AllRows].Value2;

            string TempStr;//数组中的字符串
            bool Full = true;//是否全部修改
            for (int i = 2;i<= AllRows; i++)
            {
                TempStr = FunC.TS(ORG[i, 2]);

                if (TempStr == "") { continue; }
                if(TempStr == FunC.TS(ORG[i, 1])) { continue; }

                if (!FunC.SheetExist(FunC.TS(ORG[i, 1])))
                {
                    WST.Range["A" + i].Interior.ColorIndex = 6;
                    Full = false;
                    continue;
                }
                
                if (!FunC.CheckSheetName(TempStr))
                {
                    WST.Range["B" + i].Interior.ColorIndex = 6;
                    Full = false;
                    continue;
                }
                
                try
                {
                    (ExcelApp.ActiveWorkbook.Worksheets[FunC.TS(ORG[i, 1])]).Name = TempStr;
                }
                catch
                {
                    WST.Range["B" + i].Interior.ColorIndex = 6;
                    Full = false;
                    continue;
                }
            }

            ExcelApp.ScreenUpdating = true;

            if (Full)
            {
                MessageBox.Show("修改完成！");
            }
            else
            {
                MessageBox.Show("部分未修改，请检查黄色单元格！");
            }

        }

        /// <summary>
        /// 多表合并
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnionSheet_Click(object sender, RibbonControlEventArgs e)
        {
            Form UnionSheetForm = new MyForm.WorkSheet.UnionSheetForm
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            UnionSheetForm.Show();
        }

        /// <summary>
        /// 按列拆表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitSheet_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            Excel.Workbook WBK = ExcelApp.ActiveWorkbook;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;
            int ColumnNum = 0;
            int AllRows;
            int AllColumns;
            int StartRows;

            string PromptText = "请选择需拆分列";
            //捕获用户 直接点击取消 的情况
            try
            {
                ColumnNum = ExcelApp.InputBox(Prompt: PromptText, Type: 8).Column;
            }
            catch
            {
                return;
            }
            if(ColumnNum == 0) { return; }

            //输入标题行数
            try
            {
                object TempRows = ExcelApp.InputBox("请输入标题行数（0<标题行<20）", "输入行数", 1, Type: 1);
                StartRows = FunC.TI(TempRows) + 1;
                if(StartRows>21 || StartRows < 2)
                {
                    MessageBox.Show("仅支持标题行1-20行，请重新输入！");
                    return;
                }
            }
            catch
            {
                return;
            }

            //计算行列数
            AllRows = FunC.AllRows(FunC.CName(ColumnNum));
            if (AllRows <= StartRows)
            {
                MessageBox.Show("所选列行数较少，请重新选择！");
                return;
            }
            AllColumns = FunC.AllColumns(1, 5);
            AllColumns = Math.Max(AllColumns, FunC.AllColumns(AllRows - 3, 3));
            AllColumns = Math.Max(AllColumns, 2);

            //新表名字典
            Dictionary<string, int> NewSheetNameDic = new Dictionary<string, int> { };
            string TempStr;
            bool FullAdd = true;
            object[,] ORG = WST.Range[FunC.CName(ColumnNum) +"1:" + FunC.CName(ColumnNum) + AllRows].Value2;
            for(int i = StartRows; i <= AllRows; i++)
            {
                TempStr = FunC.TS(ORG[i, 1]);
                //空值
                if (TempStr == "") { continue; }
                //字典已包含
                if (NewSheetNameDic.ContainsKey(TempStr)) { continue; }
                //是否符合命名规范
                if (!FunC.CheckSheetName(TempStr))
                {
                    WST.Range[FunC.CName(ColumnNum) + i].Interior.ColorIndex = 6;
                    FullAdd = false;
                    continue;
                }
                NewSheetNameDic.Add(TempStr, i);

                //检查表名是否超出限制
                if (NewSheetNameDic.Count >= 100)
                {
                    MessageBox.Show("最多支持100张表！请重新选择");
                    return;
                }
            }
            ORG = null;

            ExcelApp.ScreenUpdating = false;

            //检查是否删除同名表
            bool IsDelete = false;
            bool AskDelete = false;

            //遍历表名
            foreach (Excel.Worksheet wst in WBK.Worksheets)
            {
                TempStr = wst.Name;
                if (NewSheetNameDic.ContainsKey(TempStr))
                {
                    //询问是否删除
                    if (!AskDelete)
                    {
                        //弹出窗体提示
                        DialogResult IsWait = MessageBox.Show("发现重复的表名是否删除？", "请选择", MessageBoxButtons.YesNo);
                        if (IsWait == DialogResult.Yes) 
                        {
                            AskDelete = true;
                            IsDelete = true;
                            ExcelApp.DisplayAlerts = false;//关闭警示弹窗
                        }
                        else if(IsWait == DialogResult.No)
                        {
                            AskDelete = true;
                            IsDelete = false;
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (IsDelete)
                    {
                        ((Excel.Worksheet)WBK.Worksheets[TempStr]).Delete();
                    }
                    else
                    {
                        WST.Range[FunC.CName(ColumnNum) + NewSheetNameDic[TempStr]].Interior.ColorIndex = 6;
                        NewSheetNameDic.Remove(TempStr);
                    }
                }
            }
            ExcelApp.DisplayAlerts = true;//打开警示弹窗

            //复制表
            Excel.Worksheet WST1;
            foreach (string s in NewSheetNameDic.Keys)
            {
                //复制表
                WST.Copy(After: (Excel.Worksheet)WBK.Worksheets[WBK.Worksheets.Count]);
                WST1 = WBK.ActiveSheet;
                //如果开启了筛选，先关闭
                if(WST1.AutoFilterMode == true) { WST1.AutoFilterMode = false; }
                WST1.Name = s;
                //筛选非当前项
                WST1.Range[string.Format("{0}:{0}", StartRows - 1)].AutoFilter(ColumnNum, "<>" + s);
                //删除行
                WST1.Range[string.Format("A{0}:A{1}", StartRows, AllRows + 5)].SpecialCells(Excel.XlCellType.xlCellTypeVisible).EntireRow.Delete();
                //取消筛选
                WST1.AutoFilterMode = false;
            }

            WST.Select();

            ExcelApp.ScreenUpdating = true;

            if (FullAdd)
            {
                MessageBox.Show("已拆分为多个表格，请检查");
            }
            else
            {
                MessageBox.Show("部分表格未拆分，请检查所选列黄色单元格");
            }
        }

        /// <summary>
        /// 汇总工作簿
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnionBook_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;

            //让用户选择文件夹
            string FolderPath = ExcelApp.ActiveWorkbook.Path;
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "请选择工作簿存放的文件夹";
            folderDialog.SelectedPath = FolderPath;
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                FolderPath = folderDialog.SelectedPath;
            }
            else
            {
                return;
            }

            int i = 0;//合并计数
            int i1 = 0;//跳过计数
            int AllRows = 2;
            int AllColumns = 2;
            int TempInt;//存储行列数
            int MaxRows = 0;
            bool CloseIt = true;//是否关闭该文件
            
            Excel.Workbook WBK;
            Excel.Worksheet WST;

            //获取文件夹下的所有xls和xlsx文件
            string[] files = Directory.GetFiles(FolderPath, "*.xls*");
            //string[] files2 = Directory.GetFiles(FolderPath, "*.xlsx");
            //string[] files = new string[files1.Length + files2.Length];
            //files1.CopyTo(files, 0);
            //files2.CopyTo(files, files1.Length);
            //files1 = null;
            //files2 = null;

            if (files.Length == 0)
            {
                MessageBox.Show("未发现Excel文件，目前仅支持xls和xlsx格式");
                return;
            }

            ExcelApp.ScreenUpdating = false;//关闭屏幕刷新
            //创建工作簿
            Excel.Workbook NewWBK = ExcelApp.Workbooks.Add();
            Excel.Worksheet NewWST = NewWBK.Sheets[1];

            foreach (string file in files)
            {
                if (FunC.IsFileInUse(file))//如果目标文件已被打开
                {
                    try
                    {
                        WBK = ExcelApp.Workbooks[Path.GetFileName(file)];
                        CloseIt = false;
                    }
                    catch
                    {
                        i1++;
                        continue;//如果文件被后台其他文件占用，就跳过这个文件
                    }
                }
                else
                {
                    WBK = ExcelApp.Workbooks.Open(file);
                    CloseIt = true;
                }

                WST = WBK.ActiveSheet;//获取工作表

                //获取行数
                for (int i3 = 1; i3 <= 10; i3++)
                {
                    TempInt = ((Excel.Range)(WST.Cells[WST.Rows.Count, i3])).End[Excel.XlDirection.xlUp].Row;
                    AllRows = Math.Max(AllRows, TempInt);
                }
                //获取列数
                for (int i3 = 1; i3 <= 10; i3++)
                {
                    TempInt = ((Excel.Range)(WST.Cells[i3, "IV"])).End[Excel.XlDirection.xlToLeft].Column;
                    AllColumns = Math.Max(AllColumns, TempInt);
                }

                MaxRows += AllRows;
                if (MaxRows >= NewWST.Rows.Count)
                {
                    MessageBox.Show("行数超过Excel最多行数！");
                    break;
                }
                //读取数据到新工作簿，从C列开始
                NewWST.Range["C" + (MaxRows - AllRows + 1) + ":" + FunC.CName(AllColumns + 2) + MaxRows].Value2 = WST.Range["A1:" + FunC.CName(AllColumns) + AllRows].Value2;
                NewWST.Range["A" + (MaxRows - AllRows + 1) + ":A" + MaxRows].Value2 = WBK.Name;//A列存储工作簿名
                NewWST.Range["B" + (MaxRows - AllRows + 1) + ":B" + MaxRows].Value2 = WST.Name;//B列存储工作表名

                if (CloseIt) { WBK.Close(); }
                i++;
            }

            if (i != files.Length)
            {
                if (i1 == 0)
                {
                    MessageBox.Show(string.Format("已合并{0}个工作簿，{1}个被后台占用，请检查", i,i1));
                }
                else
                {
                    MessageBox.Show(string.Format("已合并{0}个工作簿，请检查" ,i));
                }
            }
            else
            {
                MessageBox.Show(string.Format("已合并所有Excel，共{0}个，请检查", i));
            }

            NewWST.Select();
            ExcelApp.ScreenUpdating = true;//打开屏幕刷新

        }

        /// <summary>
        /// 拆分工作簿
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitBook_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            Excel.Workbook WBK = ExcelApp.ActiveWorkbook;
            Excel.Worksheet WST;

            //让用户选择文件夹
            string FolderPath = ExcelApp.ActiveWorkbook.Path;
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "请选择文件夹存放拆分后的工作簿";
            folderDialog.SelectedPath = FolderPath;
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                FolderPath = folderDialog.SelectedPath;
            }
            else
            {
                return;
            }

            //将表名添加到字典
            Dictionary<string, int> SheetName = new Dictionary<string, int> { };
            foreach (Excel.Worksheet wst in WBK.Worksheets)
            {
                if(wst.Visible != Excel.XlSheetVisibility.xlSheetHidden)
                {
                    SheetName.Add(wst.Name, wst.Index);
                }
            }

            if(SheetName.Count <= 1) { MessageBox.Show("当前工作簿中工作表太少，无需拆分！");return; }

            //获取文件夹下的所有xls和xlsx文件
            string[] files = Directory.GetFiles(FolderPath, "*.xls");

            bool AskDelete = false;
            bool DeleteIt = false;

            foreach (string file in files)
            {
                //如果所选文件包含同名文件
                if (SheetName.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                {
                    if (!AskDelete)
                    {
                        DialogResult IsWait = MessageBox.Show("所选文件夹包含同名工作簿" + Environment.NewLine + "是否替换相关文件？", "请选择", MessageBoxButtons.YesNo);
                        if (IsWait == DialogResult.Yes) 
                        {
                            DeleteIt = true;
                        }
                        else if(IsWait == DialogResult.No)
                        {
                            DeleteIt = false;
                        }
                        else
                        {
                            return;
                        }
                        AskDelete = true;
                    }

                    if (DeleteIt)
                    {
                        if (FunC.IsFileInUse(file))//如果目标文件已被打开
                        {
                            MessageBox.Show("文件夹中存在打开的同名工作簿，请关闭工作簿再试");
                            return;
                        }
                        File.Delete(file);
                    }
                    else
                    {
                        SheetName.Remove(Path.GetFileNameWithoutExtension(file));
                    }
                    
                }
            }

            if (SheetName.Count == 0) { MessageBox.Show("未发现需拆分的工作表！"); return; }

            ExcelApp.ScreenUpdating = false;//关闭屏幕刷新

            //创建工作簿
            Excel.Workbook NewWBK;
            //遍历字典
            int i4 = 1;
            int i5 = SheetName.Count;
            foreach (string STName in SheetName.Keys)
            {
                WST = WBK.Worksheets[STName];
                NewWBK = ExcelApp.Workbooks.Add();
                WST.Copy(After: NewWBK.Worksheets[1]);
                (NewWBK.Worksheets[1]).Delete();
                NewWBK.SaveAs(FolderPath+"\\"+STName+".xlsx", FileFormat: Excel.XlFileFormat.xlOpenXMLWorkbook, CreateBackup: false);
                NewWBK.Close();

                //显示进度
                ExcelApp.StatusBar = "当前进度：" + Math.Round((i4 * 100d / i5), 2) + "%";
                i4++;
            }

            WBK.Activate();
            ExcelApp.StatusBar = false;
            ExcelApp.ScreenUpdating = true;
            MessageBox.Show(string.Format("已将拆分工作簿存放到{0}，请检查！", FolderPath));

        }

        /// <summary>
        /// 文本格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFormat_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if(rg.Formula != null)
                {
                    rg.NumberFormatLocal = "@";
                    rg.Value2 = FunC.TS(rg.Formula);
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGv = rg.Formula;
            rg.NumberFormatLocal = "" + "@";

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数

            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    if (ORGv[i1, i] != null)
                    {
                        NRG[i1 - 1, i - 1] = FunC.TS(ORGv[i1, i]);
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGv = null;
            NRG = null;

        }

        /// <summary>
        /// 数字格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumFormat_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Value2 != null && FunC.IsNumber(FunC.TS(rg.Value2)))
                {
                    rg.Value2 = FunC.TDM(FunC.TS(rg.Value2));
                    rg.NumberFormatLocal = "#,##0.00";
                }

                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGv = rg.Value2;
            rg.NumberFormatLocal = "#,##0.00";

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数

            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    if(ORGv[i1, i] != null)
                    {
                        if (FunC.IsNumber(FunC.TS(ORGv[i1, i])))
                        {
                            NRG[i1 - 1, i - 1] = FunC.TDM(ORGv[i1, i]);
                        }
                        else
                        {
                            NRG[i1 - 1, i - 1] = ORGv[i1, i];
                        }
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGv = null;
            NRG = null;
        }

        /// <summary>
        /// 将选区中的字母大写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToUpper_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Value2 != null && FunC.IncludeLetter(FunC.TS(rg.Value2)))
                {
                    rg.Value2 = (FunC.TS(rg.Value2)).ToUpper();
                }

                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGv = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数

            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for(int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    if (ORGv[i1, i] != null)
                    {
                        if (FunC.IncludeLetter(FunC.TS(ORGv[i1, i])))
                        {
                            NRG[i1 - 1, i - 1] = (FunC.TS(ORGv[i1, i])).ToUpper();
                        }
                        else
                        {
                            NRG[i1 - 1, i - 1] = ORGv[i1, i];
                        }
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGv = null;
            NRG = null;
        }

        /// <summary>
        /// 将选区中的字母小写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToLower_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Value2 != null && FunC.IncludeLetter(FunC.TS(rg.Value2)))
                {
                    rg.Value2 = (FunC.TS(rg.Value2)).ToLower();
                }

                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGv;//原始数组ORGv 读取值
            object[,] NRG;//新数组NRG

            ORGv = rg.Value2;

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数

            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            //定义新数组
            NRG = new object[AllRows, AllColumns];

            for (int i = 1; i <= AllColumns; i++)
            {
                for (int i1 = 1; i1 <= AllRows; i1++)
                {
                    if (ORGv[i1, i] != null)
                    {
                        if (FunC.IncludeLetter(FunC.TS(ORGv[i1, i])))
                        {
                            NRG[i1 - 1, i - 1] = (FunC.TS(ORGv[i1, i])).ToLower();
                        }
                        else
                        {
                            NRG[i1 - 1, i - 1] = ORGv[i1, i];
                        }
                    }
                }
            }

            //赋值
            WST.Range[FunC.CName(rg.Column) + rg.Row + ":" + FunC.CName(rg.Column + AllColumns - 1) + (rg.Row + AllRows - 1)].Value2 = NRG;

            ORGv = null;
            NRG = null;
        }

        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegText_Click(object sender, RibbonControlEventArgs e)
        {
            Form RegTextForm = new MyForm.Tool.RegForm{};
            RegTextForm.Show();
        }

        /// <summary>
        /// 显示或隐藏工作簿选项卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkBookCheck_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            if (WorkBookCheck.Checked)
            {
                WorkBook.Visible = true;
                clsConfig.WriteConfig("GlobalSetting", "WorkBookCheck", true.ToString());
            }
            else
            {
                WorkBook.Visible = false;
                clsConfig.WriteConfig("GlobalSetting", "WorkBookCheck", false.ToString());
            }
        }

        /// <summary>
        /// 显示或隐藏工作表选项卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkSheetCheck_Click(object sender, RibbonControlEventArgs e)
        {
            //从我的文档读取文件路径
            string strPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ClsThisAddinConfig clsConfig = new ClsThisAddinConfig(strPath);

            if (WorkSheetCheck.Checked)
            {
                WorkSheet.Visible = true;
                clsConfig.WriteConfig("GlobalSetting", "WorkSheetCheck", true.ToString());
            }
            else
            {
                WorkSheet.Visible = false;
                clsConfig.WriteConfig("GlobalSetting", "WorkSheetCheck", false.ToString());
            }
        }

        /// <summary>
        /// 公式转结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToResult_Click(object sender, RibbonControlEventArgs e)
        {
            ExcelApp = Globals.ThisAddIn.Application;
            WST = (Excel.Worksheet)ExcelApp.ActiveSheet;

            //读取选中区域
            Excel.Range rg;
            try
            {
                rg = ExcelApp.Selection;
            }
            catch
            {
                return;
            }

            //如果只选中一个单元格
            if (rg.Count == 1)
            {
                if (rg.Formula != null)
                {
                    rg.NumberFormatLocal = "G/通用格式";
                    rg.TextToColumns();
                }
                return;
            }

            //如果选中了一个区域
            int AllRows;
            int AllColumns;
            object[,] ORGv = rg.Value2;//原始数组ORGv 读取值

            rg.NumberFormatLocal = "G/通用格式";

            //限制列数，防止选择整行时多余的计算
            AllColumns = FunC.AllColumns(rg.Row, FunC.AllRows(FunC.CName(rg.Column)) + 10) - rg.Column + 1;//坑
            AllColumns = Math.Min(AllColumns, ORGv.GetLength(1));
            AllColumns = Math.Max(1, AllColumns);

            //限制行数
            AllRows = FunC.AllRows(FunC.CName(rg.Column), AllColumns) - rg.Row + 1;
            AllRows = Math.Min(AllRows, ORGv.GetLength(0));
            AllRows = Math.Max(1, AllRows);

            for (int i = 0; i < AllColumns; i++)
            {
                rg = WST.Range[string.Format("{0}{1}:{0}{2}",FunC.CName(rg.Column+i),rg.Row,(rg.Row + AllRows - 1))];
                rg.TextToColumns();
            }

            ORGv = null;
        }

        /// <summary>
        /// 工作簿公式转值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookToValue_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Application ExcelApp = Globals.ThisAddIn.Application;
            Excel.Workbook WBK = ExcelApp.ActiveWorkbook;
            WST = ExcelApp.ActiveSheet;
            //Excel.Worksheet wst;
            //Excel.Range Rg;
            int AllRows = WST.Rows.Count - 1;
            int AllColumns = WST.Columns.Count - 1;
            int TempInt;
            string TempStr;

            //输入最大行数
            try
            {
                object TempRows = ExcelApp.InputBox("请输入最大行数", "输入行数", 300, Type: 1);
                TempInt = AllRows;
                AllRows = FunC.TI(TempRows);
                if (AllRows > TempInt || AllRows < 2)
                {
                    MessageBox.Show("仅支持2-" + TempInt + "行，请重新输入！");
                    return;
                }
            }
            catch
            {
                return;
            }
            
            //输入最大列数
            try
            {
                object TempRows = ExcelApp.InputBox("请输入最大列数", "输入列数", 30, Type: 1);
                TempInt = AllColumns;
                AllColumns = FunC.TI(TempRows);
                if (AllColumns > TempInt || AllColumns < 2)
                {
                    MessageBox.Show("仅支持2-" + TempInt + "列，请重新输入！");
                    return;
                }
            }
            catch
            {
                return;
            }

            TempStr = String.Format("A1:{0}{1}", FunC.CName(AllColumns), AllRows);
            double OneStep = Math.Round(1 / double.Parse(WBK.Worksheets.Count.ToString()) * 100, 6);//WBK.Worksheets.Count;
            double i = 0;
            ExcelApp.StatusBar = "正在进行公式转值操作";
            //遍历工作表
            //var SheetE = WBK.Worksheets.GetEnumerator();
            foreach(Excel.Worksheet wst in WBK.Worksheets)
            {
                //AllRows = Math.Max(FunC.AllRows(wst, "A", 13), 2);
                //AllColumns = Math.Max(FunC.AllColumns(wst, 1, 13), 2);
                //Rg = wst.Range[TempStr];//String.Format("A1:{0}{1}", FunC.CName(AllColumns), AllRows)];
                wst.Range[TempStr].Value2 = wst.Range[TempStr].Value2;
                ExcelApp.StatusBar = "已完成 " + Math.Round(i += OneStep, 2) + "%";
            }

            ExcelApp.StatusBar = false;
        }

        private void SheetToValue_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Application ExcelApp = Globals.ThisAddIn.Application;
            WST = ExcelApp.ActiveSheet;
            int AllRows = Math.Max(FunC.AllRows(WST, "A", 13), 2);
            int AllColumns = Math.Max(FunC.AllColumns(WST, 1, 13), 2);
            Excel.Range Rg = WST.Range[String.Format("A1:{0}{1}", FunC.CName(AllColumns), AllRows)];
            Rg.Value2 = Rg.Value2;
        }
    }
}
