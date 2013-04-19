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
    public partial class MonthDeductionTax : System.Windows.Controls.Window
    {
        SMTLoading loadbar = new SMTLoading();
        SalaryServiceClient salaryClient = new SalaryServiceClient();
        bool ispaging = true;

        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        public MonthDeductionTax()
        {
            InitializeComponent();
            initContrls();
            this.TitleContent = "月度薪资扣税表";
            LoadData();
        }
        void initContrls()
        {
            DateTime dt = System.DateTime.Now.AddMonths(-1);
            years.Value = dt.Year;
            months.Value = dt.Month;
            salaryClient.GetMonthDeductionTaxsCompleted += new EventHandler<GetMonthDeductionTaxsCompletedEventArgs>(salaryClient_GetMonthDeductionTaxsCompleted);
            salaryClient.ExportMonthDeductionTaxsCompleted += new EventHandler<ExportMonthDeductionTaxsCompletedEventArgs>(salaryClient_ExportMonthDeductionTaxsCompleted);
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
        }

        void salaryClient_ExportMonthDeductionTaxsCompleted(object sender, ExportMonthDeductionTaxsCompletedEventArgs e)
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

        void salaryClient_GetMonthDeductionTaxsCompleted(object sender, GetMonthDeductionTaxsCompletedEventArgs e)
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
            salaryClient.GetMonthDeductionTaxsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEENAME",
        filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, false);


        }
        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {

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
            if (result.Value == true)
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
                salaryClient.ExportMonthDeductionTaxsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEENAME",
            filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, false);
            }
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
    }
}
