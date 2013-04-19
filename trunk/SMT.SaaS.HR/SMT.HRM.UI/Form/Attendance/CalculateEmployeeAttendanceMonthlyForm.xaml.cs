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

using SMT.Saas.Tools.AttendanceWS;      //考勤接口
using SMT.Saas.Tools.OrganizationWS;    //公司组织接口
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class CalculateEmployeeAttendanceMonthlyForm : BaseForm, IEntityEditor
    {
        AttendanceServiceClient clientAtt;
        private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient clientOrg;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        public CalculateEmployeeAttendanceMonthlyForm()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(CalculateEmployeeAttendanceMonthlyForm_Loaded);            
        }

        void CalculateEmployeeAttendanceMonthlyForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt = new AttendanceServiceClient();
            clientOrg = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            clientAtt.CalculateEmployeeAttendanceMonthlyByPostIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceMonthlyByPostIDCompleted);
            clientAtt.CalculateEmployeeAttendanceMonthlyByCompanyIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceMonthlyByCompanyIDCompleted);
            clientAtt.CalculateEmployeeAttendanceMonthlyByDepartmentIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceMonthlyByDepartmentIDCompleted);

            clientOrg.GetCompanyByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs>(clientOrg_GetCompanyByIdCompleted);
        }

        private void InitParas()
        {
            txtBalanceYear.Text = DateTime.Now.Year.ToString();
            txtBalanceMonth.Text = DateTime.Now.AddMonths(-1).Month.ToString();
            

            if (cbxkAssignedObjectType.Items == null)
            {
                return;
            }

            if (cbxkAssignedObjectType.Items.Count == 0)
            {
                return;
            }

            foreach(object obj in cbxkAssignedObjectType.Items)
            {
                T_SYS_DICTIONARY entDic = obj as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    continue;
                }

                if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                {
                    cbxkAssignedObjectType.SelectedItem = obj;
                    clientOrg.GetCompanyByIdAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);                    
                    break;
                }
            }
        }        

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTENDMONTHLYBALANCE");
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public void DoAction(string actionType)
        {
            
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            return items;       
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
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

        #endregion

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为公司）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetCompanyByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY entCompany = e.Result as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                lkAssignObject.DataContext = entCompany;
                lkAssignObject.DisplayMemberPath = "CNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_CalculateEmployeeAttendanceMonthlyByPostIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MONTHLYBALANCESUCCESSED"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void clientAtt_CalculateEmployeeAttendanceMonthlyByDepartmentIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MONTHLYBALANCESUCCESSED"));                
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void clientAtt_CalculateEmployeeAttendanceMonthlyByCompanyIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MONTHLYBALANCESUCCESSED"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.ProgressBar);
        }

        private void lkAssignObject_FindClick(object sender, EventArgs e)
        {
            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            OrganizationLookupForm lookup = new OrganizationLookupForm();
            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Company;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Department;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Post;
            }

            lookup.SelectedClick += (obj, ev) =>
            {
                lkAssignObject.DataContext = lookup.SelectedObj;

                if (lookup.SelectedObj is T_HR_COMPANY)
                {
                    lkAssignObject.DisplayMemberPath = "CNAME";
                }
                else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
                else if (lookup.SelectedObj is T_HR_POST)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBalanceCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                return;
            }

            if (lkAssignObject.DataContext == null)
            {
                return;
            }

            bool flag = false;
            int iYear =0, iMonth = 0;
            flag = int.TryParse(txtBalanceYear.Text, out iYear);
            if (iYear <= 0)
            {
                return;
            }

            flag = int.TryParse(txtBalanceMonth.Text, out iMonth);
            if (iMonth <= 0 || iMonth > 12)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            RefreshUI(RefreshedTypes.ProgressBar);

            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
            {
                T_HR_COMPANY entCompany = lkAssignObject.DataContext as T_HR_COMPANY;
                string strCompanyID = entCompany.COMPANYID;

                clientAtt.CalculateEmployeeAttendanceMonthlyByCompanyIDAsync(iYear.ToString() + "-" + iMonth.ToString(), strCompanyID);
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
            {
                T_HR_DEPARTMENT entDepartment = lkAssignObject.DataContext as T_HR_DEPARTMENT;
                string strDepartmentID = entDepartment.DEPARTMENTID;

                clientAtt.CalculateEmployeeAttendanceMonthlyByDepartmentIDAsync(iYear.ToString() + "-" + iMonth.ToString(), strDepartmentID);
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
            {
                T_HR_POST entPost = lkAssignObject.DataContext as T_HR_POST;
                string strPostID = entPost.POSTID;

                clientAtt.CalculateEmployeeAttendanceMonthlyByPostIDAsync(iYear.ToString() + "-" + iMonth.ToString(), strPostID);
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
            {
                T_HR_EMPLOYEE entEmp = lkAssignObject.DataContext as T_HR_EMPLOYEE;
                string strEmpID = entEmp.EMPLOYEEID;

                clientAtt.CalculateEmployeeAttendanceMonthlyByEmployeeIDAsync(iYear.ToString() + "-" + iMonth.ToString(), strEmpID);
            }
        }        
    }
}
