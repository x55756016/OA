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
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI.PersonControl
{
    public partial class PersonControl : System.Windows.Controls.Window, IClient 
    {
        public PersonControl()
        {
            InitializeComponent();
            InitEvent();
        }

        public V_EMPLOYEEVIEW SelectedObj
        {
            get;
            set;
        }
        SMTLoading loadbar = new SMTLoading();
        PersonnelServiceClient client;
        public event EventHandler SelectedClick;
       
        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client = new PersonnelServiceClient();
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            client.GetEmployeeViewsPagingCompleted += new EventHandler<GetEmployeeViewsPagingCompletedEventArgs>(client_GetEmployeeViewsPagingCompleted);
        }

        void client_GetEmployeeViewsPagingCompleted(object sender, GetEmployeeViewsPagingCompletedEventArgs e)
        {
            List<V_EMPLOYEEVIEW> list = new List<V_EMPLOYEEVIEW>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                // ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                DtGrid.ItemsSource = e.Result;
                dataPager.PageCount = e.pageCount;
            }
            loadbar.Stop();
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            if (!string.IsNullOrEmpty(txtEmpName.Text))
            {

                // filter += "T_HR_EMPLOYEE.EMPLOYEECNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)";
                paras.Add(txtEmpName.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtEmpCode.Text))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
             
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECODE)";
                paras.Add(txtEmpCode.Text.Trim());
            }
            string sType = treeOrganization.sType, sValue = treeOrganization.sValue;
            client.GetEmployeeViewsPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME", filter,
                paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           // SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEE");
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedObj = DtGrid.SelectedItems[0] as V_EMPLOYEEVIEW;
            if (this.SelectedClick != null)
            {
                this.SelectedClick(this, null);
            }
            this.Close();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            SelectedObj = null;
            if (this.SelectedClick != null)
            {
                this.SelectedClick(this, null);
            }
            this.Close();
        }

        
    }
}
