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
    public partial class CalculateEmployeeAttendanceYearlyForm : BaseForm, IEntityEditor
    {
        AttendanceServiceClient clientAtt;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        public CalculateEmployeeAttendanceYearlyForm()
        {
            InitializeComponent();
            clientAtt = new AttendanceServiceClient();
            clientAtt.CalculateEmployeeAttendanceYearlyByPostIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceYearlyByPostIDCompleted);
            clientAtt.CalculateEmployeeAttendanceYearlyByCompanyIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceYearlyByCompanyIDCompleted);
            clientAtt.CalculateEmployeeAttendanceYearlyByDepartmentIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceYearlyByDepartmentIDCompleted);            
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

        void clientAtt_CalculateEmployeeAttendanceYearlyByPostIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("YEARLYBALANCESUCCESSED"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_CalculateEmployeeAttendanceYearlyByDepartmentIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("YEARLYBALANCESUCCESSED"));                
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_CalculateEmployeeAttendanceYearlyByCompanyIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("YEARLYBALANCESUCCESSED"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
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

        private void btnBalanceCalculate_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

            if (lkAssignObject.DataContext == null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

            bool flag = false;
            int iYear =0;
            flag = int.TryParse(txtBalanceYear.Text, out iYear);
            if (iYear <= 0)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

            int iYearCheck = DateTime.Now.Year;

            if (iYear >= iYearCheck)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OUTYEARLYBALANCE"));
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
            {
                T_HR_COMPANY entCompany = lkAssignObject.DataContext as T_HR_COMPANY;
                string strCompanyID = entCompany.COMPANYID;

                clientAtt.CalculateEmployeeAttendanceYearlyByCompanyIDAsync(iYear.ToString(), strCompanyID);
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
            {
                T_HR_DEPARTMENT entDepartment = lkAssignObject.DataContext as T_HR_DEPARTMENT;
                string strDepartmentID = entDepartment.DEPARTMENTID;

                clientAtt.CalculateEmployeeAttendanceYearlyByDepartmentIDAsync(iYear.ToString(), strDepartmentID);
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
            {
                T_HR_POST entPost = lkAssignObject.DataContext as T_HR_POST;
                string strPostID = entPost.POSTID;

                clientAtt.CalculateEmployeeAttendanceYearlyByPostIDAsync(iYear.ToString(), strPostID);
            }
        }        
    }
}
