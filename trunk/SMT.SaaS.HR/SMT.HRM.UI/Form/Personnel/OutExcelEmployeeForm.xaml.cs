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

using SMT.SaaS.FrameworkUI.ChildWidow;
using System.IO;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.OrganizationWS;
using  SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.AutoCompleteComboBox;

namespace SMT.HRM.UI.Form.Personnel
{
    /// <summary>
    /// 批量导入部门和岗位信息
    /// </summary>
    public partial class OutExcelEmployeeForm : System.Windows.Controls.Window
    {
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        OrganizationServiceClient client;
        PersonnelServiceClient perClient;
        /// <summary>
        /// 构造函数
        /// </summary>
        public OutExcelEmployeeForm()
        {
            InitializeComponent();
            this.TitleContent = "导出员工档案";
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();
            };
        }

        /// <summary>
        ///  注册事件
        /// </summary>
        void InitControlEvent()
        {
            client = new OrganizationServiceClient();
            perClient = new PersonnelServiceClient();
            client.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            perClient = new PersonnelServiceClient();
            perClient.ExportEmployeeCompleted += new EventHandler<ExportEmployeeCompletedEventArgs>(perClient_ExportEmployeeCompleted);
            perClient.GetEmployeeByCompanyIDCompleted += new EventHandler<GetEmployeeByCompanyIDCompletedEventArgs>(perClient_GetEmployeeByCompanyIDCompleted);
            ImportOrgInfoForm_Load();
        }

        /// <summary>
        /// 导出数据完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perClient_ExportEmployeeCompleted(object sender, ExportEmployeeCompletedEventArgs e)
        {
            try
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
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void ImportOrgInfoForm_Load()
        {
            client.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        /// <summary>
        /// 加载该员工可以看到的有效的公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                acbCompanyName.ItemsSource = e.Result;
                acbCompanyName.ValueMemberPath = "CNAME";
            }
        }

        /// <summary>
        /// 选项公司事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acbCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (acbCompanyName.SelectedItem == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTCOMPANY"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            string companyID = (acbCompanyName.SelectedItem as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
            perClient.GetEmployeeByCompanyIDAsync(companyID);
        }
        /// <summary>
        /// 加载选择公司员工档案信息进行预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perClient_GetEmployeeByCompanyIDCompleted(object sender, GetEmployeeByCompanyIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                DtGrid.ItemsSource = e.Result;
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        /// <summary>
        ///导出数据按钮 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutExcel_Click(object sender, RoutedEventArgs e)
        {
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            string companyID = (acbCompanyName.SelectedItem as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
            perClient.ExportEmployeeAsync(companyID);
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        /// <summary>
        /// DataGrid序号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE tmp = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)e.Row.DataContext;
            int index = e.Row.GetIndex();
            var cell = DtGrid.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();
            var IdNum = DtGrid.Columns[4].GetCellContent(e.Row) as TextBlock;
        }
        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
    }
}
