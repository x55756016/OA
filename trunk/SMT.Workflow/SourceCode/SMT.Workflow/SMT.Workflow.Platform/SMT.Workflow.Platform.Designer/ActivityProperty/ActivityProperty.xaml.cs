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


using SMT.Workflow.Platform.Designer.Dialogs;
using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;

namespace SMT.Workflow.Platform.Designer.ActivityProperty
{
    public partial class ActivityProperty : UserControl, IProperty
    {
        private SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl _activity;//原来的      
        private IList<ActivityObject> _ActivityObjects = new List<ActivityObject>();
        List<CounterSignRole> counterSignRoleList = null;
        /// <summary>
        /// 当前流程活动的集合
        /// </summary>
        public IList<ActivityObject> ActivityObjects
        {
            get { return _ActivityObjects; }
            set { _ActivityObjects = value; }
        }
        /// <summary>
        /// 活动对象
        /// </summary>
        private ActivityObject _activityObject;

        public ActivityProperty()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 活动的属性数据
        /// </summary>
        public ActivityObject ActivityObjectData
        {
            get { return _activityObject; }
            set { _activityObject = value; }
        }
        public bool isInit = false;
        public void ShowPropertyWindow(UIElement element)
        {
            firstBind = 0;
            _activity = element as SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl;       
            if (_activity == null) return;
            _activity.Title = _activity.Title; 
            #region 新建时创建_activityObject
            if (_activityObject == null)
            {
                ActivityObject obj = ActivityObjects.Where(p => p.ActivityId.Equals("State" + _activity.UniqueID)).SingleOrDefault();
                if (obj == null)
                {
                    _activityObject = new ActivityObject();
                    _activityObject.ActivityId = "State" + _activity.UniqueID;
                    //if(_activity.
                    chkGroupAudit.IsChecked = false;
                    HideRows();
                    if (_activity.Title.IndexOf("会签") < 0)
                    {
                        stateList = WfUtils.StateList;
                        if (Utility.CurrentUser != null)
                        {
                            List<StateType> StateList = WfUtils.GetRoleListByCompanyID(WfUtils.StateList, Utility.CurrentUser.OWNERCOMPANYID);
                            cboRoles.ItemsSource = StateList.OrderBy(c => c.StateName);
                            if (StateList.Count > 0)
                            {
                                isInit = true;
                                if ((cboRoles.Items[0] as StateType).StateName == _activity.Title)
                                {
                                    cboRoles.SelectedIndex = 0;
                                }
                                if (_activity.Title.IndexOf("新建") > -1)
                                {
                                    cboRoles.SelectedIndex = 0;
                                }
                            }
                            cboUserType.ItemsSource = WfUtils.GetUserTypeList();
                            cboUserType.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    _activityObject = obj;
                }               
            }          
            #endregion
            isInit = false;
        }
        /// <summary>
        /// 是否第一次绑定
        /// </summary>
        int firstBind = 0;

        /// <summary>
        /// 活动设置属性
        /// </summary>
        public void LoadProperty()
        {
            var e = (from ent in ActivityObjects
                    where ent.ActivityId == ActivityObjectData.ActivityId
                    select ent).FirstOrDefault();
            if (e != null)
            {
                #region 已存在的节点
                _activity.Title = e.Remark;
                if (e.IsCounterSign)
                {
                    #region 会签

                    this.cbOtherCompany.IsChecked = false;
                    this.cboOtherCompany.Visibility = System.Windows.Visibility.Collapsed;
                    this.boTxt.Visibility = System.Windows.Visibility.Visible;
                    this.boBox.Visibility = System.Windows.Visibility.Collapsed;
                    counterSignRoleList = e.CounterSignRoleList;
                    dgCountersign.ItemsSource = null;
                    dgCountersign.ItemsSource = counterSignRoleList;
                    chkGroupAudit.IsChecked = true;
                    this.txtActivityName.Text = "";
                    this.txtActivityName.Text = e.Remark;
                    if (cboRule.Items.Count > 0)
                    {
                        if (_activityObject.CounterType != null)
                        {
                            cboRule.SelectedIndex = int.Parse(_activityObject.CounterType);
                        }
                    }
                    stateList = WfUtils.StateList;
                    List<StateType> StateList = WfUtils.GetRoleListByCompanyID(WfUtils.StateList, Utility.CurrentUser.OWNERCOMPANYID);
                    if (cboUserType.SelectedIndex > -1)
                    {
                        cboUserType.SelectedIndex = 0;
                    }
                    cboCountersignRoles.DisplayMemberPath = "StateName";
                    cboCountersignRoles.ItemsSource = StateList;
                    if (StateList.Count > 0) cboCountersignRoles.SelectedIndex = 0;
                    _activity.Title = e.Remark;
                    #endregion
                }
                else
                {
                    counterSignRoleList = e.CounterSignRoleList;

                    if (stateList != null)
                    {
                        #region 非会签
                        chkGroupAudit.IsChecked = false;
                        this.boTxt.Visibility = System.Windows.Visibility.Collapsed;
                        this.boBox.Visibility = System.Windows.Visibility.Visible;
                        this.cbOtherCompany.IsChecked = false;

                        StateType stateType = stateList.Where(p => p.StateCode.Equals(_activityObject.RoleId)).SingleOrDefault();
                        if (stateType != null)
                        {

                            if (!string.IsNullOrEmpty(_activityObject.OtherCompanyId))
                            {
                                SMT.Workflow.Platform.Designer.Utils.SLCache.ComboBoxBindAllCompany(cboOtherCompany, null);
                                this.cbOtherCompany.IsChecked = true;
                                this.cboOtherCompany.Visibility = System.Windows.Visibility.Visible;
                                #region 绑定已选中指定的公司,指定公司没有,直接上级,隔级上级,部门
                                var ItemsSource = stateList.Where(p => p.CompanyID == _activityObject.OtherCompanyId).ToList();
                                if (!string.IsNullOrEmpty(stateType.CompanyID))
                                {
                                    #region 如果指定的公司没有角色，只好绑定当前角色所在的公司
                                    if (ItemsSource != null && ItemsSource.Count < 1)
                                    {
                                        cboOtherCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", stateType.CompanyID);
                                        ItemsSource = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == stateType.CompanyID).ToList();
                                    }
                                    #endregion
                                }
                                if (!ItemsSource.Contains(stateType))
                                {
                                    ItemsSource.Add(stateType);
                                }
                                cboRoles.ItemsSource = ItemsSource.OrderBy(c => c.StateName);
                                cboRoles.SelectedByObject<StateType>("StateName", _activityObject.RoleName);
                                for (int i = 0; i < cboOtherCompany.Items.Count; i++)
                                {
                                    if (e.OtherCompanyId == (cboOtherCompany.Items[i] as V_COMPANY).COMPANYID && e.OtherCompanyId == stateType.CompanyID)//角色的公司ID=指定公司ID
                                    {
                                        cboOtherCompany.SelectedIndex = i;
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region 本公司
                                this.cbOtherCompany.IsChecked = false;
                                this.cboOtherCompany.Visibility = System.Windows.Visibility.Collapsed;
                                var ItemsSource = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == Utility.CurrentUser.OWNERCOMPANYID).ToList();
                                if (!ItemsSource.Contains(stateType))
                                {
                                    ItemsSource.Add(stateType);
                                }
                                cboRoles.ItemsSource = ItemsSource.OrderBy(c => c.StateName);
                                cboRoles.SelectedByObject<StateType>("StateName", _activityObject.RoleName);
                                #endregion
                            }
                        }
                        else if (!string.IsNullOrEmpty(_activityObject.RoleId))
                        {//已绑定角色，但又不是特定公司，又不是当前登录用户所属的公司
                            if (!stateList.Contains(new StateType { StateCode = _activityObject.RoleId, StateName = _activityObject.RoleName, CompanyID = _activityObject.OtherCompanyId }))
                            {
                                stateList.Add(new StateType { StateCode = _activityObject.RoleId, StateName = _activityObject.RoleName, CompanyID = _activityObject.OtherCompanyId });
                            }
                            cboUserType.ItemsSource = stateList;
                            if (stateList.Count > 0)
                            {
                                cboRoles.SelectedByObject<StateType>("StateName", _activityObject.RoleName);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(_activityObject.RoleId))
                        {
                            for (int i = 0; i < cboRoles.Items.Count; i++)
                            {
                                if (_activityObject.RoleId == (cboRoles.Items[i] as StateType).StateCode)
                                {
                                    cboRoles.SelectedIndex = i;
                                }
                            }
                        }
                        if (cboUserType.Items.Count > 0)
                        {
                            if (_activityObject.UserType != null)
                            {
                                cboUserType.SelectedIndex = _activityObject.UserType == "CREATEUSER" ? 0 : 1;
                            }
                            else
                            {//如果是新拖进来的节点,默认是选中第一个
                                cboUserType.SelectedIndex = 0;
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
            else
            {
                #region 新建节点
                counterSignRoleList = _activityObject.CounterSignRoleList;//解决新建 一个节点时，会签时带上，上一点节点的角色
                #endregion
            }
            HideRows();
            UpdateActivityObject(_activityObject);
            isInit = true;
        }
        public void ClearActivity()
        {
            if (_activityObject != null)
            {
                _activityObject.CounterSignRoleList = null;
            }
            this.txtActivityName.Text = "";           
        }
        private void BindActivityProperty()
        {
            if (stateList != null)
            {
                StateType stateType = stateList.Where(p => p.StateCode.Equals(_activityObject.RoleId)).SingleOrDefault();
                int index = 0;
                if (stateType != null)
                {

                    if (!string.IsNullOrEmpty(_activityObject.OtherCompanyId))
                    {
                        #region 绑定已选中指定的公司,指定公司没有,直接上级,隔级上级,部门
                        var ItemsSource = stateList.Where(p => p.CompanyID == _activityObject.OtherCompanyId).ToList();
                        if (!string.IsNullOrEmpty(stateType.CompanyID))
                        {
                            ItemsSource = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == stateType.CompanyID).ToList();
                        }
                        if (!ItemsSource.Contains(stateType))
                        {
                            ItemsSource.Add(stateType);
                        }
                        cboRoles.ItemsSource = ItemsSource.OrderBy(c => c.StateName);
                        cboRoles.SelectedByObject<StateType>("StateName", _activityObject.RoleName);
                        #endregion
                    }
                    else
                    {                   
                        #region 本公司
                        var ItemsSource = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == Utility.CurrentUser.OWNERCOMPANYID).ToList();
                        if (!ItemsSource.Contains(stateType))
                        {
                            ItemsSource.Add(stateType);
                        }
                        cboRoles.ItemsSource = ItemsSource.OrderBy(c => c.StateName);
                        cboRoles.SelectedByObject<StateType>("StateName", _activityObject.RoleName);
                        #endregion
                    }

                }

                else if (!string.IsNullOrEmpty(_activityObject.RoleId))
                {//已绑定角色，但又不是特定公司，又不是当前登录用户所属的公司
                    if (!stateList.Contains(new StateType { StateCode = _activityObject.RoleId, StateName = _activityObject.RoleName, CompanyID = _activityObject.OtherCompanyId }))
                    {
                        stateList.Add(new StateType { StateCode = _activityObject.RoleId, StateName = _activityObject.RoleName, CompanyID = _activityObject.OtherCompanyId });
                    }
                    cboUserType.ItemsSource = stateList;
                    if (stateList.Count > 0)
                    {
                        cboRoles.SelectedByObject<StateType>("StateName", _activityObject.RoleName);
                    }
                }
                else
                {//新拖动进入面板的一个活动
                    if (cboRoles.Items.Count > 0)
                    {
                        cboRoles.SelectedIndex = index;

                    }
                }
            }
        }
        private void txtActivityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _activity.Title = this.txtActivityName.Text;
            _activityObject.Remark = this.txtActivityName.Text;
        }
        #region 角色选择
        private bool State = true;

        private void cboRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_activityObject != null)
            {
                if (isInit)
                {
                    if (cboRoles.SelectedItem != null)
                    {                  
                        _activityObject.RoleId = ((cboRoles.SelectedItem) as StateType).StateCode;
                        _activityObject.RoleName = ((cboRoles.SelectedItem) as StateType).StateName;
                        _activityObject.Remark = ((cboRoles.SelectedItem) as StateType).StateName;
                        _activity.Title = ((cboRoles.SelectedItem) as StateType).StateName;
                        _activityObject.OtherCompanyId = CompanyID;
                        _activityObject.OtherCompanyName = CompanyName;
                    }
                    if (cboUserType.SelectedItem != null)
                    {
                        _activityObject.UserType = ((cboUserType.SelectedItem) as UserType).TypeCode;
                        _activityObject.UserTypeName = (cboUserType.SelectedItem as UserType).TypeName;
                    }
                }
            }
                    
        }
        #endregion
        #region 用户类型选择
        private void cboUserType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_activityObject != null)
            {
                if (isInit)
                {
                    if (cboRoles.SelectedItem != null)
                    {                     
                        _activityObject.RoleId = ((cboRoles.SelectedItem) as StateType).StateCode;
                        _activityObject.RoleName = ((cboRoles.SelectedItem) as StateType).StateName;
                        _activityObject.Remark = ((cboRoles.SelectedItem) as StateType).StateName;
                        _activity.Title = ((cboRoles.SelectedItem) as StateType).StateName;
                        _activityObject.OtherCompanyId = CompanyID;
                        _activityObject.OtherCompanyName = CompanyName;

                    }
                    if (cboUserType.SelectedItem != null)
                    {
                        _activityObject.UserType = ((cboUserType.SelectedItem) as UserType).TypeCode;
                        _activityObject.UserTypeName = (cboUserType.SelectedItem as UserType).TypeName;
                    }
                }
            }
        }
        #endregion      
        #region 是否会签

        private void chkGroupAudit_Click(object sender, RoutedEventArgs e)
        {
            HideRows();
            if (_activityObject != null)
            {
                _activityObject.ActivityId = "State" + _activity.UniqueID;         
                _activityObject.IsCounterSign = chkGroupAudit.IsChecked == true ? true : false;
                _activityObject.CounterType = cboRule.SelectedIndex.ToString();
            }
            if (((CheckBox)sender).IsChecked == false)
            {//非会签
                if (_activityObject != null)
                {
                    this.boTxt.Visibility = System.Windows.Visibility.Collapsed;
                    this.boBox.Visibility = System.Windows.Visibility.Visible;
                    _activityObject.IsCounterSign = false;               
                    _activityObject.CounterSignRoleList = null;
                   
                    dgCountersign.ItemsSource = null;
                    CompanyName = "";
                    CompanyID = "";
                    if (cboRoles.Items.Count > 0)
                    {
                        cboRoles.SelectedIndex = 0;
                        var role = cboRoles.SelectedItem as StateType;
                        if (role != null)
                        {
                            this.txtActivityName.Text = role.StateName;
                        }
                    }                    
                }
            }
            else
            {//会签
                if (_activityObject != null)
                {                 
                    this.txtActivityName.Text = "";
                    this.boTxt.Visibility = System.Windows.Visibility.Visible;
                    this.boBox.Visibility = System.Windows.Visibility.Collapsed;
                    _activityObject.IsCounterSign = true;
                    this.txtActivityName.Text = "会签节点";           
                    CompanyName = "";
                    CompanyID = "";
                    stateList = WfUtils.StateList;
                    if (_activityObject.CounterSignRoleList == null || _activityObject.CounterSignRoleList.Count < 1)
                    {
                        _activityObject.CounterSignRoleList = null;
                        dgCountersign.ItemsSource = null;
                       
                    }
                    if (Utility.CurrentUser != null)
                    {
                        List<StateType> StateList = WfUtils.GetRoleListByCompanyID(WfUtils.StateList, Utility.CurrentUser.OWNERCOMPANYID);
                        cboCountersignRoles.ItemsSource = StateList.OrderBy(c => c.StateName);

                        if (StateList.Count > 0)
                        {
                            State = false;
                            cboCountersignRoles.SelectedIndex = 0;
                        }
                        cboUserType.ItemsSource = WfUtils.GetUserTypeList();
                        cboUserType.SelectedIndex = 0;

                    }                   
                }
            }
        }
        /// <summary>
        /// 选中会签时,显示条件
        /// </summary>
        private void HideRows()
        {
            GridLength minLength = new GridLength(0d);
            GridLength regLength = new GridLength(25d);
            //grdBaseProperty.RowDefinitions[2].Height = (bool)chkGroupAudit.IsChecked ? regLength : minLength;
            //grdBaseProperty.RowDefinitions[3].Height = (bool)chkGroupAudit.IsChecked ? regLength : minLength;
            grdBaseProperty.RowDefinitions[4].Height = (bool)chkGroupAudit.IsChecked ? regLength : minLength;
            grdBaseProperty.RowDefinitions[5].Height = (bool)chkGroupAudit.IsChecked ? regLength : minLength;
            grdBaseProperty.RowDefinitions[6].Height = (bool)chkGroupAudit.IsChecked ? new GridLength(25d, GridUnitType.Auto) : minLength;
            grdBaseProperty.RowDefinitions[7].Height = (bool)chkGroupAudit.IsChecked ? new GridLength(25d, GridUnitType.Auto) : minLength;
        }
        #endregion

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (dgCountersign.SelectedItems.Count == 1)
            {
                CounterSignRole ent = dgCountersign.SelectedItem as CounterSignRole;
                counterSignRoleList.Remove(ent);
                dgCountersign.ItemsSource = null;
                dgCountersign.ItemsSource = counterSignRoleList;
                if (counterSignRoleList.Count < 1)
                {
                    counterSignRoleList = null;
                }
                _activityObject.CounterSignRoleList = counterSignRoleList;
                UpdateActivityObject(_activityObject);
            }
            else
            {

                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录", "确定");
            }       
           
        }      
        #region 定义角色

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (_activityObject == null)
            {
                _activityObject = new ActivityObject();
                _activityObject.ActivityId = _activity.UniqueID;              
                _activityObject.CounterType = cboRule.SelectedIndex.ToString();
            }
            _activityObject.Remark = this.txtActivityName.Text;
            if (counterSignRoleList == null) counterSignRoleList = new List<CounterSignRole>();
            CounterSignRole newCondition = new CounterSignRole();
            if (cboUserType.SelectedItem != null)
            {
                newCondition.TypeCode = (cboUserType.SelectedItem as UserType).TypeCode;
                newCondition.TypeName = (cboUserType.SelectedItem as UserType).TypeName;
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "用户类型没有出来，请重新选择！", "确定");
                return;
            }
            _activityObject.IsCounterSign = true;
            newCondition.StateCode = (cboCountersignRoles.SelectedItem as StateType).StateCode;
            if (cbOtherCompany.IsChecked == true)
            {
                newCondition.IsOtherCompany = true;
                newCondition.OtherCompanyId = ((cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.V_COMPANY).COMPANYID;
                newCondition.OtherCompanyName = ((cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.V_COMPANY).CNAME;
            }
            else
            {
                newCondition.IsOtherCompany = false;
                newCondition.OtherCompanyId = "";
                newCondition.OtherCompanyName = "";
            }
            newCondition.StateName = (cboCountersignRoles.SelectedItem as StateType).StateName;

            _activityObject.IsSpecifyCompany = false;
            _activityObject.OtherCompanyId = "";
            _activityObject.OtherCompanyName = "";
            _activityObject.RoleId = "";
            _activityObject.RoleName = "";
            _activityObject.UserType = "";
            _activityObject.ActivityId = "State" + _activity.UniqueID;         
            _activityObject.CounterType = cboRule.SelectedIndex.ToString();
            var item = counterSignRoleList.Where(p => p.StateCode == (cboCountersignRoles.SelectedItem as StateType).StateCode).FirstOrDefault();
            if (item == null)
            {
                counterSignRoleList.Add(newCondition);
                dgCountersign.ItemsSource = null;
                dgCountersign.ItemsSource = counterSignRoleList;
            }
            _activityObject.CounterSignRoleList = counterSignRoleList;
            //if (_activityObject.CounterSignRoleList != null && _activityObject.CounterSignRoleList.Count > 0)
            //{//已有角色
            //    if (item == null)
            //    {
            //        counterSignRoleList.Add(newCondition);
            //        dgCountersign.ItemsSource = null;
            //        dgCountersign.ItemsSource = counterSignRoleList;
            //    }
            //}
            //else
            //{//没有角色
            //    counterSignRoleList.Add(newCondition);
            //    _activityObject.CounterSignRoleList = counterSignRoleList;
            //}
            UpdateActivityObject(_activityObject);
            if (this.chkGroupAudit.IsChecked == true)
            {
                this.txtActivityName.Text = "";
                this.txtActivityName.Text = "会签节点";
            }   
        }
     
        #endregion

        #region 如果活动不存在,就加入到集合里
        /// <summary>
        /// 更新活动的集合(如果活动不存在,就加入到集合里)
        /// </summary>
        /// <param name="activity">活动</param>
        /// <returns></returns>
        public void UpdateActivityObject(ActivityObject activity)
        {
            var e = from a in ActivityObjects
                    where a.ActivityId == activity.ActivityId
                    select a;
            var ent = e.FirstOrDefault();
            if (ent != null)
            {
                //先删除后增加,保证数据是最新的
                ActivityObjects.Remove(ent);
                ActivityObjects.Add(ent);
            }
            else
            {
                ActivityObjects.Add(activity);
            }

        }
        #endregion     

        #region 选择公司
        List<StateType> stateList = new List<StateType>();
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
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup(Utility.CurrentUser.EMPLOYEEID, "3", "");
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                if (lookup.SelectedObj != null)
                {
                    foreach (var item in lookup.SelectedObj)
                    {
                        string selectcompanyID = ((SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance)).COMPANYID;                     
                        cboOtherCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", selectcompanyID);
                        if (this.chkGroupAudit.IsChecked == false)
                        {
                            if (_activityObject.IsSpecifyCompany)
                            {//指定公司
                                var stateType = stateList.Where(p => p.CompanyID == selectcompanyID).ToList();
                                cboRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                                if (stateType.Count > 0)
                                {
                                    cboRoles.SelectedIndex = 0;
                                }
                            }
                            else
                            {
                                var stateType = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == selectcompanyID).ToList();
                                cboRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                                if (stateType.Count > 0)
                                {
                                    cboRoles.SelectedIndex = 0;
                                }
                            }
                        }
                        else
                        {
                            if (_activityObject.IsSpecifyCompany)
                            {//指定公司
                                var stateType = stateList.Where(p => p.CompanyID == selectcompanyID).ToList();
                                cboCountersignRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                                if (stateType.Count > 0)
                                {
                                    cboCountersignRoles.SelectedIndex = 0;
                                }
                            }
                            else
                            {
                                var stateType = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == selectcompanyID).ToList();
                                cboCountersignRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                                if (stateType.Count > 0)
                                {
                                    cboCountersignRoles.SelectedIndex = 0;
                                }
                            }
                            this.txtActivityName.Text = "";
                            this.txtActivityName.Text = "会签节点";    
                        }
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

        #region 是否选择特定公司
        private void cbOtherCompany_Click(object sender, RoutedEventArgs e)
        {
            _activityObject.IsSpecifyCompany = ((CheckBox)sender).IsChecked == true ? true : false;
            SMT.Workflow.Platform.Designer.Utils.SLCache.ComboBoxBindAllCompany(cboOtherCompany, null);
            if (((CheckBox)sender).IsChecked == false)
            {
                cboOtherCompany.Visibility = Visibility.Collapsed;             
                CompanyName = "";
                CompanyID = "";
                _activityObject.OtherCompanyId = "";
                _activityObject.OtherCompanyName = "";
                #region 绑定当前登录用户所属公司的角色
                var stateType = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == Utility.CurrentUser.OWNERCOMPANYID).ToList();
                //cboRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                //if (stateType.Count > 0 && this.chkGroupAudit.IsChecked == false)
                //{
                //    cboRoles.SelectedIndex = 0;
                //}
                if (this.chkGroupAudit.IsChecked == true)
                {
                    cboCountersignRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                    if (stateType.Count > 0)
                    {
                        cboCountersignRoles.SelectedIndex = 0;
                    }
                    this.txtActivityName.Text = "";
                    this.txtActivityName.Text = "会签节点";
                }
                else
                {
                    cboRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                    if (stateType.Count > 0)
                    {
                        cboRoles.SelectedIndex = 0;
                    }
                }
                cboUserType.ItemsSource = WfUtils.GetUserTypeList();
                cboUserType.SelectedIndex = 0;
                #endregion
            }
            else
            {
                //btnSearch.Visibility = Visibility.Visible;顶顶顶顶
                cboOtherCompany.Visibility = Visibility.Visible;
                #region 绑定特定公司的角色
                var t_hr_company = (cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.V_COMPANY;
                if (t_hr_company != null)
                {
                    CompanyName = t_hr_company.CNAME;
                    CompanyID = t_hr_company.COMPANYID;
                    _activityObject.OtherCompanyId = t_hr_company.COMPANYID;
                    _activityObject.OtherCompanyName = t_hr_company.CNAME;
                    //指定公司没有,直接上级,隔级上级,部门
                    //var stateType = stateList.Where(p => p.CompanyID == "11111" || p.CompanyID == t_hr_company.COMPANYID).ToList();
                    var stateType = stateList.Where(p => p.CompanyID == t_hr_company.COMPANYID).ToList();
                    if (this.chkGroupAudit.IsChecked == true)
                    {
                        cboCountersignRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                        if (stateType.Count > 0)
                        {
                            cboCountersignRoles.SelectedIndex = 0;
                        }
                        this.txtActivityName.Text = "";
                        this.txtActivityName.Text = "会签节点";                       
                    }
                    else
                    {
                        cboRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                        if (stateType.Count > 0)
                        {
                            cboRoles.SelectedIndex = 0;
                        }
                    }
                    cboUserType.ItemsSource = WfUtils.GetUserTypeList();
                    cboUserType.SelectedIndex = 0;
                }
                #endregion
            }
        }

        #endregion

        #region 选择特定公司
        private void cboOtherCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_activityObject != null && isInit)
            {           
                if (cboOtherCompany.SelectedItem != null)
                {
                    var t_hr_company = (cboOtherCompany.SelectedItem) as SMT.Saas.Tools.OrganizationWS.V_COMPANY;
                    CompanyName = t_hr_company.CNAME;
                    CompanyID = t_hr_company.COMPANYID;
                    _activityObject.OtherCompanyId = t_hr_company.COMPANYID;
                    _activityObject.OtherCompanyName = t_hr_company.CNAME;
                    #region 绑定特定公司的角色 ,指定公司没有,直接上级,隔级上级,部门    
                    if (this.chkGroupAudit.IsChecked == false)
                    {
                        #region  非会签  
                        var stateType = stateList.Where(p => p.CompanyID == CompanyID).ToList();
                        cboRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                        if (stateType.Count > 0)
                        {
                            State = false;
                            cboRoles.SelectedIndex = 0;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 会签 
                        var stateType = stateList.Where(p => p.CompanyID == t_hr_company.COMPANYID).ToList();
                        cboCountersignRoles.ItemsSource = stateType.OrderBy(c => c.StateName);
                        if (stateType.Count > 0)
                        {
                            cboCountersignRoles.SelectedIndex = 0;
                        }
                        cboUserType.ItemsSource = WfUtils.GetUserTypeList();
                        cboUserType.SelectedIndex = 0;
                       
                        #endregion
                        this.txtActivityName.Text = "";
                        this.txtActivityName.Text = "会签节点";    
                    }
                    #endregion
                }
            }

        }
        #endregion

        #region 选择会签规则
        private void cboRule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_activityObject != null)
            {
                _activityObject.CounterType = cboRule.SelectedIndex.ToString();
            }
        }
        #endregion


    }
}
