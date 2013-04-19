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

using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalaryContrast : BaseForm
    {
        SalaryServiceClient client;
        private SalaryGenerateType salaryGenerateTypes;
        private SalaryGenerateType SalaryGenerateTypes
        {
            get { return salaryGenerateTypes; }
            set { salaryGenerateTypes = value; }
        }
        public SalaryContrast()
        {
            InitializeComponent();
            new BasePage().GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            InitParas();
        }

        private void InitParas()
        {
            client = new SalaryServiceClient();
            client.GetSalaryRecordOneCompleted += new EventHandler<GetSalaryRecordOneCompletedEventArgs>(client_GetSalaryRecordOneCompleted);
            client.GetEmployeeCustomRecordOneCompleted += new EventHandler<GetEmployeeCustomRecordOneCompletedEventArgs>(client_GetEmployeeCustomRecordOneCompleted);
        }

        public void FlashData(string employeeid, string year, string month, SalaryGenerateType salarygeneratetype)
        {
            if (salarygeneratetype == SalaryGenerateType.SalaryRecord)
            {
                DGSalary.Visibility = Visibility.Visible;
                client.GetSalaryRecordOneAsync(employeeid, year, month);
            }
            else if (salarygeneratetype == SalaryGenerateType.CustomSalaryRecord)
            {
                DGSalary2.Visibility = Visibility.Visible;
                client.GetEmployeeCustomRecordOneAsync(employeeid, year, month);
            }
            else if (salarygeneratetype == SalaryGenerateType.PerformanceRecord)
            {
                ;
            }
            else 
            {
                return;
            }
        }

        void client_GetEmployeeCustomRecordOneCompleted(object sender, GetEmployeeCustomRecordOneCompletedEventArgs e)
        {


            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<T_HR_CUSTOMGUERDONRECORD> salaryrecordlast = new List<T_HR_CUSTOMGUERDONRECORD>();
                    txtbtitle.Text = Utility.GetResourceStr("SALARYLAST") + ":";
                    salaryrecordlast.Add(e.Result);
                    DGSalary2.ItemsSource = salaryrecordlast;
                }
                else
                {
                    DGSalary2.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void client_GetSalaryRecordOneCompleted(object sender, GetSalaryRecordOneCompletedEventArgs e)
        {
            //txtbtitle.Text = System.DateTime.Now.ToLongTimeString();

            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<T_HR_EMPLOYEESALARYRECORD> salaryrecordlast = new List<T_HR_EMPLOYEESALARYRECORD>();
                    txtbtitle.Text = Utility.GetResourceStr("SALARYLAST") + ":";
                    DGSalary.DataContext = null;
                    DGSalary.ItemsSource = null;
                    salaryrecordlast.Add(e.Result);
                    DGSalary.ItemsSource = salaryrecordlast;
                    DGSalary.Visibility = Visibility.Visible;
                }
                else
                {
                    DGSalary.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        private void DGSalary_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }
    }
}
