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
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.Workflow.Platform.Designer.Views.Engine;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;

namespace SMT.Workflow.Platform.Designer.Dialogs
{
    public partial class FlowRuleDefineDialog : ChildWindow
    {
        private List<CompareCondition> _conditionList;
        private string _sysName = string.Empty;
        private string _objectName = string.Empty;

        //XML Service 定义
        private FlowXmlDefineClient _xmlClient = new FlowXmlDefineClient();

        public FlowRuleDefineDialog()
        {
            InitializeComponent();

            //注册业务对象事件
            _xmlClient.GetSystemBOAttributeListCompleted += new EventHandler<GetSystemBOAttributeListCompletedEventArgs>(_xmlClient_GetSystemBOAttributeListCompleted);
        }
        private void _xmlClient_GetSystemBOAttributeListCompleted(object sender, GetSystemBOAttributeListCompletedEventArgs e)
        {
            //try
            //{
            if (e.Result != null)
            {
                cboField.ItemsSource = e.Result;
                cboField.DisplayMemberPath = "Description";

                if (e.Result.Count > 0)
                {
                    cboField.SelectedIndex = 0;
                }
            }
            //}
            //catch (Exception ex)
            //{
            //}
        }

        public List<CompareCondition> ConditionList
        {
            get { return _conditionList; }
            set { _conditionList = value; }
        }

        public string SysName
        {
            get { return _sysName; }
            set { _sysName = value; }
        }

        public string ObjectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }
        /// <summary>
        /// 加载规则
        /// </summary>
        public void LoadConditions()
        {
            btnLookUp.Visibility = Visibility.Collapsed;
            cboPostLevel.Visibility = Visibility.Collapsed; 
            OrganizationCondition();
            GetPostLevel();
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _conditionList;
            cboOperate.SelectedIndex = 0;
            _xmlClient.GetSystemBOAttributeListAsync(_sysName, _objectName);
        }
        //public void LoadConditions(List<FLOW_RULES> ruleslist)
        //{
        //    btnLookUp.Visibility = Visibility.Collapsed;
        //    cboPostLevel.Visibility = Visibility.Collapsed; 
        //    OrganizationCondition();
        //    GetPostLevel();
        //    dgCodition.ItemsSource = null;
        //    dgCodition.ItemsSource = ruleslist;
        //    cboOperate.SelectedIndex = 0;
        //}

        #region 增加条件

        private void btnAddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (_conditionList == null) _conditionList = new List<CompareCondition>();
            if (cboField.SelectedItem != null)
            {
                if (!string.IsNullOrEmpty(txtCompareValue.Text.Trim()))
                {
                    CompareCondition newCondition = new CompareCondition()
                    {
                        Name = System.Guid.NewGuid().ToString(),
                        Description = (cboField.SelectedItem as WFBOAttribute).Description,
                        CompAttr = (cboField.SelectedItem as WFBOAttribute).Name,
                        DataType = (cboField.SelectedItem as WFBOAttribute).DataType,
                        Operate = ((System.Windows.Controls.ContentControl)(cboOperate.SelectedItem)).Content.ToString(),
                        CompareValue = txtCompareValue.Text.Trim().ToString(),
                    };
                    _conditionList.Add(newCondition);
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "比较值不能为空！", "确定");
                }
            }
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _conditionList;
        }
        #endregion
        #region 删除条件
        private void btnRemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            string itemName = ((Button)e.OriginalSource).Tag.ToString();

            foreach (CompareCondition cpitem in _conditionList)
            {
                if (cpitem.Name == itemName)
                {
                    _conditionList.Remove(cpitem);
                    break;
                }
            }
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _conditionList;
        }
        #endregion
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        #region 条件选择时判断,哪些是选择的，哪些是填写的
        Dictionary<string, object> list = null;
        /// <summary>
        /// 组织架构的条件
        /// </summary>
        private void OrganizationCondition()
        {
            list = new Dictionary<string, object>();
            list.Add("OWNERNAME", "所属用户名称");
            list.Add("CURRENTEMPLOYEENAME", "提交者");
            list.Add("CLAIMSWERENAME", "报销人姓名");
            list.Add("OWNER", "所属人");
            list.Add("CREATEUSERNAME", "创建人");
            list.Add("DEPARTMENTNAME", "部门名称");
            list.Add("POSTNAME", "岗位名称");
            list.Add("UPDATEUSERID", "修改人");
            list.Add("UPDATEUSERNAME", "修改人名");
            list.Add("POSTID", "岗位ID");

            list.Add("OWNERCOMPANYID", "所属公司ID");
            list.Add("CREATECOMPANYID", "创建人公司ID");
            list.Add("COMPANYID", "公司ID");

            list.Add("OWNERDEPARTMENTID", "所属部门ID");
            list.Add("CREATEDEPARTMENTID", "创建人部门ID");

            list.Add("OWNERPOSTID", "所属岗位ID");
            list.Add("CREATEPOSTID", "创建人岗位ID");

            list.Add("OWNERID", "所属员工ID");
            list.Add("EMPLOYEEID", "员工ID");
            list.Add("EMPLOYEENAME", "员工姓名");
            list.Add("CREATEUSERID", "创建人");
          
        }
        string fieldName = "";
        private void cboField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboField.SelectedItem != null)
            {
                fieldName = "";
                txtCompareValue.Text = "";
                bool isSelected = (cboField.SelectedItem as WFBOAttribute).IsSelected;
                string dataType = (cboField.SelectedItem as WFBOAttribute).DataType;
                string name = (cboField.SelectedItem as WFBOAttribute).Name;
                fieldName = name;
                #region 是否显示组织架构
                if (IsOrganization(name))
                {
                    txtCompareValue.IsReadOnly = true;
                    btnLookUp.Visibility = Visibility.Visible;
                    //ShowOrganization(GetOrgTreeItemTypes(name), name);
                }
                else
                {
                    btnLookUp.Visibility = Visibility.Collapsed;
                    txtCompareValue.IsReadOnly = false;
                }
                #endregion
                #region 是否显示岗位级别
                if (name.ToUpper() == "POSTLEVEL")
                {
                    cboPostLevel.Visibility = Visibility.Visible;
                    txtCompareValue.IsReadOnly = true;
                }
                else
                {
                    cboPostLevel.Visibility = Visibility.Collapsed;
                    txtCompareValue.IsReadOnly = false;
                }
                #endregion
                IsStringDataType(dataType);
            }

        }
        /// <summary>
        /// 是否显示组织架构
        /// </summary>
        private bool IsOrganization(string name)
        {
            lookTitle = "";
            #region 是否显示组织架构
            if (list.ContainsKey(name.ToUpper()))
            {
                lookTitle = list[name].ToString();
                return true;
            }
            else
            {
                return false;
            }
            #endregion
        }

        /// <summary>
        /// 得到组织架构的类型
        /// </summary>
        private SMT.SaaS.FrameworkUI.OrgTreeItemTypes GetOrgTreeItemTypes(string name)
        {
            #region 得到组织架构的类型
            SMT.SaaS.FrameworkUI.OrgTreeItemTypes type = new SaaS.FrameworkUI.OrgTreeItemTypes();
            switch (name)
            {
                case "OWNER":
                case "CREATEUSERNAME":
                case "OWNERNAME":
                case "CURRENTEMPLOYEENAME":
                case "CLAIMSWERENAME":

                case "OWNERID":
                case "EMPLOYEEID":
                case "EMPLOYEENAME":
                case "CREATEUSERID":
                case "UPDATEUSERID":
                case "UPDATEUSERNAME":
                    type = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
                    break;
                case "POSTNAME":
                case "OWNERPOSTID":
                case "CREATEPOSTID":
                case "POSTID":
                    type = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post;
                    break;
                case "DEPARTMENTNAME":
                case "OWNERDEPARTMENTID":
                case "CREATEDEPARTMENTID":
                    type = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
                    break;
                case "OWNERCOMPANYID":
                case "CREATECOMPANYID":
                case "COMPANYID":
                    type = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                    break;
                default:
                    type = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                    break;
            }
            return type;
            #endregion

        }
        string lookTitle = "";
        private void ShowOrganization(SMT.SaaS.FrameworkUI.OrgTreeItemTypes orgTreeItemType, string nameType)
        {
            LooKUP up = new LooKUP(orgTreeItemType, "你选择的条件是:" + lookTitle);
            up.SelectedClick += (obj, ev) =>
            {
                if (up.SelectList != null)
                {
                    string selectid = "";
                    if (SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company == orgTreeItemType)
                    {
                        if (nameType == "COMPANYNAME")
                        {
                            selectid = ((SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(up.SelectList.FirstOrDefault().ObjectInstance)).CNAME;
                        }
                        else
                        {
                            selectid = ((SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(up.SelectList.FirstOrDefault().ObjectInstance)).COMPANYID;
                        }
                    }
                    if (SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department == orgTreeItemType)
                    {
                        if (nameType == "DEPARTMENTNAME")
                        {
                            selectid = ((SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)(up.SelectList.FirstOrDefault().ObjectInstance)).T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        }
                        else
                        {
                            selectid = ((SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)(up.SelectList.FirstOrDefault().ObjectInstance)).DEPARTMENTID;
                        }
                    }
                    if (SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post == orgTreeItemType)
                    {
                        if (nameType == "POSTNAME")
                        {
                            selectid = ((SMT.Saas.Tools.OrganizationWS.T_HR_POST)(up.SelectList.FirstOrDefault().ObjectInstance)).T_HR_POSTDICTIONARY.POSTNAME;
                        }
                        else
                        {
                            selectid = ((SMT.Saas.Tools.OrganizationWS.T_HR_POST)(up.SelectList.FirstOrDefault().ObjectInstance)).POSTID;
                        }

                    }
                    if (SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel == orgTreeItemType)
                    {
                        if (nameType == "OWNER"
                            || nameType == "CREATEUSERNAME"
                            || nameType == "OWNERNAME"
                            || nameType == "CURRENTEMPLOYEENAME"
                            || nameType == "CLAIMSWERENAME"
                             || nameType == "UPDATEUSERNAME"
                            )
                        {
                            selectid = ((SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)(up.SelectList.FirstOrDefault().ObjectInstance)).EMPLOYEECNAME;
                        }
                        else
                        {
                            selectid = ((SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)(up.SelectList.FirstOrDefault().ObjectInstance)).EMPLOYEEID;
                        }

                    }
                    txtCompareValue.Text = selectid;
                }
                //if (up.SelectList != null)
                //{
                //    OrgObj = up.SelectList.FirstOrDefault();
                //    this.txtReceiveUser.Text = up.SelectList.FirstOrDefault().ObjectName;
                //}
            };
            up.Show();
            return;
        }
        private void IsStringDataType(string dataType)
        {          
            if (dataType.ToLower() == "string")
            {//如果是字符串类型，只能是等于==
                cboOperate.SelectedByText("==");
                cboOperate.IsEnabled = false;
            }
            else
            {
                cboOperate.IsEnabled = true;
            }
        }
        #endregion

        #region 打开组织架构
        private void btnLookUp_Click(object sender, RoutedEventArgs e)
        {
            ShowOrganization(GetOrgTreeItemTypes(fieldName), fieldName);
        }
        
        #endregion

        #region 获取岗位级别
        private void GetPostLevel()
        {
            string path = "silverlightcache\\POSTLEVEL.txt";
            try
            {
              
                var postlevel =SLCache.GetCache<List<V_Dictionary>>(path, 10);
                if (postlevel != null)
                {
                    var ents = from a in postlevel
                               orderby a.DICTIONARYNAME ascending
                               select a;
                    cboPostLevel.BindData(ents.ToList(), "DICTIONARYNAME", "DICTIONARYVALUE");
                }
                else
                {
                    PermissionServiceClient psc = new PermissionServiceClient();
                    System.Collections.ObjectModel.ObservableCollection<string> strs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    strs.Add("POSTLEVEL");
                    //   strs.Add("TYPEAPPROVAL");
                    psc.GetDictionaryByCategoryArrayAsync(strs);
                    psc.GetDictionaryByCategoryArrayCompleted += (o, e) =>
                    {
                        if (e.Error == null)
                        {
                            if (e.Result != null)
                            {
                                //System.Collections.ObjectModel.ObservableCollection<V_Dictionary> dics =e.Result;
                                var ents = from a in e.Result
                                           orderby a.DICTIONARYNAME ascending
                                           select a;
                                cboPostLevel.BindData(ents.ToList(), "DICTIONARYNAME", "DICTIONARYVALUE");
                                SLCache.SaveData<List<V_Dictionary>>(e.Result.ToList(), path);
                            }
                        }
                    }; 
                }
            }
            catch
            {
                PermissionServiceClient psc = new PermissionServiceClient();
                System.Collections.ObjectModel.ObservableCollection<string> strs = new System.Collections.ObjectModel.ObservableCollection<string>();
                strs.Add("POSTLEVEL");
                //   strs.Add("TYPEAPPROVAL");
                psc.GetDictionaryByCategoryArrayAsync(strs);
                psc.GetDictionaryByCategoryArrayCompleted += (o, e) =>
                {
                    if (e.Error == null)
                    {
                        if (e.Result != null)
                        {
                            //System.Collections.ObjectModel.ObservableCollection<V_Dictionary> dics =e.Result;
                            var ents = from a in e.Result
                                       orderby a.DICTIONARYNAME ascending
                                       select a;
                            cboPostLevel.BindData(ents.ToList(), "DICTIONARYNAME", "DICTIONARYVALUE");
                            SLCache.SaveData<List<V_Dictionary>>(e.Result.ToList(), path);
                        }
                    }
                }; 
            }
           

        }
        #endregion

        #region 岗位级别选择
        private void cboPostLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPostLevel.SelectedItem != null)
            {
                txtCompareValue.Text = "";
                string value = (cboPostLevel.SelectedItem as V_Dictionary).DICTIONARYVALUE.ToString();
                txtCompareValue.Text = value;
            }
        }
        #endregion
    }
}

