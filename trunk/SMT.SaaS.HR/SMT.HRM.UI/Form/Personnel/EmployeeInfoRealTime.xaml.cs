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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.IO;
namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeInfoRealTime : System.Windows.Controls.Window, IClient
    {
        private PersonnelServiceClient personClient = new PersonnelServiceClient();
        SMTLoading loadbar = new SMTLoading();
        string sType = "", sValue = "";
       // bool ispaging = true;

        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;

        public EmployeeInfoRealTime()
        {
            InitializeComponent();
            this.TitleContent = Utility.GetResourceStr("员工实时信息");
            initEvents();
            this.Loaded += new RoutedEventHandler(EmployeeInfoRealTime_Loaded);

        }

        void EmployeeInfoRealTime_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void initEvents()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            T_HR_COMPANY companyInit = new T_HR_COMPANY();
            companyInit.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            companyInit.CNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            sType = "Company";
            sValue = companyInit.COMPANYID;
            lkSelectObj.DisplayMemberPath = "CNAME";
            lkSelectObj.DataContext = companyInit;
            personClient.GetEmployeesIntimeCompleted += new EventHandler<GetEmployeesIntimeCompletedEventArgs>(personClient_GetEmployeesIntimeCompleted);
            personClient.ExportEmployeesIntimeCompleted += new EventHandler<ExportEmployeesIntimeCompletedEventArgs>(personClient_ExportEmployeesIntimeCompleted);
        }
        #region  完成事件
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void personClient_ExportEmployeesIntimeCompleted(object sender, ExportEmployeesIntimeCompletedEventArgs e)
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
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
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
        /// <summary>
        /// 获取实时信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void personClient_GetEmployeesIntimeCompleted(object sender, GetEmployeesIntimeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DtGrid.ItemsSource = e.Result;
            }
            loadbar.Stop();
        }
        #endregion
        void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            if (sType == "Company")
            {
                filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                paras.Add(sValue);
            }
            if (sType == "Department")
            {
                filter += " OWNERDEPARTMENTID==@" + paras.Count().ToString();
                paras.Add(sValue);
            }
            if (sType == "Post")
            {
                filter += " OWNERPOSTID==@" + paras.Count().ToString();
                paras.Add(sValue);
            }

            //personClient.GetEmployeesIntimeAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME",
            //   filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            personClient.GetEmployeesIntimeAsync(dataPager.PageIndex, dataPager.PageSize, "DepartmentName",
                filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
           // ispaging = false;
            dialog.Filter = "MS csv Files|*.csv";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                loadbar.Start();
                int pageCount = 0;
                string filter = "";
                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

                if (sType == "Company")
                {
                    filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }
                if (sType == "Department")
                {
                    filter += " OWNERDEPARTMENTID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }
                if (sType == "Post")
                {
                    filter += " OWNERPOSTID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }
                personClient.ExportEmployeesIntimeAsync(dataPager.PageIndex, dataPager.PageSize, "DepartmentName",
                    filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;

            lookup.TitleContent = Utility.GetResourceStr("ORGANNAME");

            lookup.SelectedClick += (obj, ev) =>
            {
                lkSelectObj.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)
                {
                    lkSelectObj.DisplayMemberPath = "CNAME";
                    sType = "Company";
                    sValue = (lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
                }
                else if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)
                {
                    lkSelectObj.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    sType = "Department";
                    sValue = (lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).DEPARTMENTID;
                }
                else if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_POST)
                {
                    lkSelectObj.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    sType = "Post";
                    sValue = (lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_POST).POSTID;
                }
                //else if (lookup.SelectedObj is T_HR_EMPLOYEE)
                //{
                //    lkSelectObj.DisplayMemberPath = "EMPLOYEECNAME";
                //}
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //V_LEFTOFFICEVIEW view = e.Row.DataContext as V_LEFTOFFICEVIEW;
            int rowNumber = e.Row.GetIndex() + 1;
            TextBlock Tnumber = DtGrid.Columns[0].GetCellContent(e.Row).FindName("Tnumber") as TextBlock;
            Tnumber.Text = rowNumber.ToString();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            personClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

    }
}
