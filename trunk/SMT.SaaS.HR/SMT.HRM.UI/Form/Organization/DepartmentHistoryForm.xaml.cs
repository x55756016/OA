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
using SMT.Saas.Tools.OrganizationWS;

using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Form
{
    public partial class DepartmentHistoryForm : BaseForm
    {
        private T_HR_DEPARTMENTHISTORY companyDepart;

        public T_HR_DEPARTMENTHISTORY Department
        {
            get { return companyDepart; }
            set
            {
                companyDepart = value;
                this.DataContext = companyDepart;
            }
        }

        OrganizationServiceClient client;

        public DepartmentHistoryForm(T_HR_DEPARTMENTHISTORY companydepart, string companyName)
        {
            InitializeComponent();
            InitControlEvent();
            Department = companydepart;
            //公司名称
            txtCompanyName.Text = companyName;
            //绑定部门字典
            client.GetDepartmentDictionaryAllAsync();
            this.IsEnabled = false;
        }

        private void InitControlEvent()
        {
            client = new OrganizationServiceClient();
            client.GetDepartmentDictionaryAllCompleted += new EventHandler<GetDepartmentDictionaryAllCompletedEventArgs>(client_GetDepartmentDictionaryAllCompleted);

        }

        void client_GetDepartmentDictionaryAllCompleted(object sender, GetDepartmentDictionaryAllCompletedEventArgs e)
        {
            cbxDepartMent.ItemsSource = e.Result;
            cbxDepartMent.DisplayMemberPath = "DEPARTMENTNAME";
            if (Department.T_HR_DEPARTMENTDICTIONARY != null)
            {
                foreach (var item in cbxDepartMent.Items)
                {
                    T_HR_DEPARTMENTDICTIONARY dict = item as T_HR_DEPARTMENTDICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DEPARTMENTDICTIONARYID == Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID)
                        {
                            cbxDepartMent.SelectedItem = item;
                            txtDepartmentCode.Text = dict.DEPARTMENTCODE;
                            break;
                        }
                    }
                }
            }
        }
    }
}
