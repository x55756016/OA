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

using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.Dialogs
{
    public partial class FlowRoleDefineDialog : ChildWindow
    {
        private List<CounterSignRole> _counterSignRoleList;

        public FlowRoleDefineDialog()
        {
            InitializeComponent();
            #region 加载所有公司
            cboOtherCompany.Visibility = Visibility.Collapsed;
            SMT.Workflow.Platform.Designer.Utils.SLCache.ComboBoxBindAllCompany(cboOtherCompany,null);
            #endregion
            stateList = WfUtils.StateList;
            List<StateType> StateList = WfUtils.GetRoleListByCompanyID(WfUtils.StateList, Utility.CurrentUser.OWNERCOMPANYID);
            cboInfo.DisplayMemberPath = "StateName";
            cboInfo.ItemsSource = StateList;        
            if (StateList.Count > 0) cboInfo.SelectedIndex = 0;

            cboUserType.ItemsSource = WfUtils.GetUserTypeList();
            cboUserType.SelectedIndex =1;
        }


        public List<CounterSignRole> CounterSignRoleList
        {
            get { return _counterSignRoleList; }
            set { _counterSignRoleList = value; }
        }

        public void LoadRoles()        
        {
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _counterSignRoleList;
        }
        #region 确定
       
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        #endregion
        #region 取消
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        #endregion
        #region 增加角色

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cboInfo.SelectedItem as StateType == null)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "处理角色不能为空!", "确定");
                return;
            }
            if (_counterSignRoleList == null) _counterSignRoleList = new List<CounterSignRole>();

            CounterSignRole newCondition = new CounterSignRole();
            newCondition.StateCode = (cboInfo.SelectedItem as StateType).StateCode;
            if (cbOtherCompany.IsChecked == true)
            {
                newCondition.IsOtherCompany = true;
                newCondition.OtherCompanyId = ((cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
            }
            else
            {
                newCondition.IsOtherCompany = false;
                newCondition.OtherCompanyId = "";
            }
            newCondition.StateName = (cboInfo.SelectedItem as StateType).StateName;
            newCondition.TypeCode = (cboUserType.SelectedItem as UserType).TypeCode;
            newCondition.TypeName = (cboUserType.SelectedItem as UserType).TypeName;

            _counterSignRoleList.Add(newCondition);
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _counterSignRoleList;
            //增加完后变空
            CompanyName = "";
            CompanyID = "";
            btnSearch.IsEnabled = false;
            cbOtherCompany.IsChecked = false;
        }
        #endregion
        #region 选择公司
        string CompanyName = "";
        string CompanyID = "";
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //object objs = null;
            //if (Application.Current.Resources["CurrentUserID"] != null)
            //{
            //    objs = Application.Current.Resources["CurrentUserID"];
            //    Application.Current.Resources.Remove("CurrentUserID");
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //if (Application.Current.Resources["CurrentUserID"] == null)
            //{
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                if (lookup.SelectedObj != null)
                {
                    foreach (var item in lookup.SelectedObj)
                    {
                         CompanyName = ((SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance)).CNAME;
                         CompanyID = ((SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance)).COMPANYID;
                        //Application.Current.Resources.Remove("CurrentUserID");
                        //if (objs != null)
                        //{
                        //    Application.Current.Resources.Add("CurrentUserID", objs);
                        //}
                    }
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        #endregion
        #region 是否选中特定公司选项
       
        private void cbOtherCompany_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == false)
            {
                cboOtherCompany.Visibility = Visibility.Collapsed;
                CompanyName = "";
                CompanyID = "";
                #region 绑定当前登录用户所属公司的角色
                var stateType = stateList.Where(p => p.CompanyID == Utility.CurrentUser.OWNERCOMPANYID).ToList();
                cboInfo.ItemsSource = stateType.OrderBy(c => c.StateName);
                if (stateType.Count > 0)
                {
                    cboInfo.SelectedIndex = 0;
                }
                #endregion
            }
            else
            {
                cboOtherCompany.Visibility = Visibility.Visible;
                #region 绑定特定公司的角色
                var t_hr_company = (cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                var stateType = stateList.Where(p => p.CompanyID == t_hr_company.COMPANYID).ToList();
                cboInfo.ItemsSource = stateType.OrderBy(c => c.StateName);
                if (stateType.Count > 0)
                {
                    cboInfo.SelectedIndex = 0;
                }
                #endregion
            }

        }
        #endregion
        #region 选择特定公司
        List<StateType> stateList = new List<StateType>();
        private void cboOtherCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbOtherCompany.IsChecked==true)
            {
                if (cboOtherCompany.SelectedItem != null)
                {
                    var t_hr_company = (cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                    CompanyName = t_hr_company.CNAME;
                    CompanyID = t_hr_company.COMPANYID;
                    #region 绑定特定公司的角色
                    var stateType = stateList.Where(p => p.CompanyID == CompanyID).ToList();
                    cboInfo.ItemsSource = stateType.OrderBy(c => c.StateName);
                    if (stateType.Count > 0)
                    {
                        cboInfo.SelectedIndex = 0;
                    }
                    #endregion
                
                }
            }

        }
        #endregion
        #region 删除条件      
        private void btnDelCodition_Click(object sender, RoutedEventArgs e)
        {
            string itemName = ((Button)e.OriginalSource).Tag.ToString();

            foreach (CounterSignRole cpitem in _counterSignRoleList)
            {
                if (cpitem.StateName == itemName)
                {
                    _counterSignRoleList.Remove(cpitem);
                    break;
                }
            }
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _counterSignRoleList;
        }
        #endregion
    }
}

