using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.IO;
using System.Text;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class EmployeeSalarySummary : System.Windows.Controls.Window
    {
        SMTLoading loadbar = new SMTLoading();
        SalaryServiceClient salaryClient = new SalaryServiceClient();
        bool ispaging = true;

        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        public EmployeeSalarySummary()
        {
            InitializeComponent();
            initContrls();
            this.TitleContent = "薪资汇总表";
            LoadData();
        }

        void initContrls()
        {
            DateTime dt = System.DateTime.Now.AddMonths(-1);
            years.Value = dt.Year;
            months.Value = dt.Month;
            salaryClient.GetSalarySummaryCompleted += new EventHandler<GetSalarySummaryCompletedEventArgs>(salaryClient_GetSalarySummaryCompleted);
            salaryClient.ExportSalarySummaryCompleted += new EventHandler<ExportSalarySummaryCompletedEventArgs>(salaryClient_ExportSalarySummaryCompleted);
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
        }

        void salaryClient_ExportSalarySummaryCompleted(object sender, ExportSalarySummaryCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        void salaryClient_GetSalarySummaryCompleted(object sender, GetSalarySummaryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                foreach (var ent in e.Result)
                {
                    ent.Balance = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.Balance);
                    ent.CalculateDeduct = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.CalculateDeduct);
                    ent.Personalincometax = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.Personalincometax);
                    ent.Sum = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.Sum);
                    ent.TaxesBasic = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.TaxesBasic);
                    ent.TaxesRate = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.TaxesRate);
                    ent.AbsenceDays = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.AbsenceDays);
                    ent.AreadifAllowance = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.AreadifAllowance);
                    ent.AttendanceUnusualDeduct = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.AttendanceUnusualDeduct);
                    ent.BasicSalary = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.BasicSalary);
                    ent.DeductTotal = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.DeductTotal);
                    ent.DutyAllowance = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.DutyAllowance);
                    ent.FixIncomeSum = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.FixIncomeSum);
                    ent.FoodAllowance = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.FoodAllowance);
                    ent.HousingAllowance = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.HousingAllowance);
                    ent.HousingDeduction = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.HousingDeduction);
                    ent.MantissaDeduct = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.MantissaDeduct);
                    ent.OtherAddDeduction = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.OtherAddDeduction);
                    ent.OtherSubjoin = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.OtherSubjoin);
                    ent.OvertimeSum = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.OvertimeSum);
                    ent.PerformancerewardRecord = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.PerformancerewardRecord);
                    ent.PersonalsiCost = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.PersonalsiCost);
                    ent.PostSalary = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.PostSalary);
                    ent.PretaxSubTotal = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.PretaxSubTotal);
                    ent.SecurityAllowance = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.SecurityAllowance);
                    ent.SubTotal = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.SubTotal);
                    ent.TaxCoefficient = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.TaxCoefficient);
                    ent.VacationDeduct = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.VacationDeduct);
                    ent.WorkingSalary = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.WorkingSalary);
                    ent.ActuallyPay = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(ent.ActuallyPay);

                }
                DtGrid.ItemsSource = e.Result;
                dataPager.PageCount = e.pageCount;
            }
            else
            {

            }
            loadbar.Stop();
        }

        void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            string month = months.Value.ToString();
            string year = years.Value.ToString();

            if (!string.IsNullOrEmpty(year))
            {
                filter = " salaryyear==@" + paras.Count.ToString();
                paras.Add(year);
            }

            if (!string.IsNullOrEmpty(month))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " and ";
                filter += " salarymonth==@" + paras.Count.ToString();
                paras.Add(month);
            }
            salaryClient.GetSalarySummaryAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEENAME",
        filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, year, month, false);


        }
        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            int rowNumber = e.Row.GetIndex() + 1;
            TextBlock Tnumber = DtGrid.Columns[0].GetCellContent(e.Row).FindName("Tnumber") as TextBlock;
            Tnumber.Text = rowNumber.ToString();

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            ispaging = true;
            LoadData();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            ispaging = true;
            LoadData();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ispaging = false;
            dialog.Filter = "MS csv Files|*.csv";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            string month = months.Value.ToString();
            string year = years.Value.ToString();

            if (!string.IsNullOrEmpty(year))
            {
                filter = " salaryyear==@" + paras.Count.ToString();
                paras.Add(year);
            }

            if (!string.IsNullOrEmpty(month))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " and ";
                filter += " salarymonth==@" + paras.Count.ToString();
                paras.Add(month);
            }
            salaryClient.ExportSalarySummaryAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEENAME",
        filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, year, month, false);
        }
    }
}
