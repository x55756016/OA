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
using SMT.Workflow.Platform.Designer.DesignerControl;

using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.Dialogs;
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.Views.Engine;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.Workflow.Platform.Designer.ActivityProperty
{
    public partial class LineProperty : UserControl
    {

        private FlowXmlDefineClient xmlClient = new FlowXmlDefineClient();
        /// <summary>
        /// 连线属性对性
        /// </summary>
        private LineObject _lineObject;

        /// <summary>
        /// 系统名
        /// </summary>
        private string _sysName = string.Empty;

        /// <summary>
        /// 业务对象名
        /// </summary>
        private string _objectName = string.Empty;

        /// <summary>
        /// 连线控件
        /// </summary>
       private LineControl _lineControl;//旧的
       private IList<LineObject> _lineObjects = new List<LineObject>();

       private List<CompareCondition> _conditionList;


     
       /// <summary>
       /// 当前连线的集合
       /// </summary>
       public IList<LineObject> LineObjects
       {
           get { return _lineObjects; }
           set { _lineObjects = value; }
       }
        /// <summary>
        /// 连线控件
        /// </summary>    
        public LineProperty()
        {
            InitializeComponent();
            //注册业务对象事件
            xmlClient.GetSystemBOAttributeListCompleted += new EventHandler<GetSystemBOAttributeListCompletedEventArgs>(xmlClient_GetSystemBOAttributeListCompleted);
           
        }

        void xmlClient_GetSystemBOAttributeListCompleted(object sender, GetSystemBOAttributeListCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                cboField.ItemsSource = null;               
                cboField.ItemsSource = e.Result;
                cboField.DisplayMemberPath = "Description";
                if (e.Result.Count > 0)
                {
                    cboField.SelectedIndex = 0;
                }
            }
        }

        public string systemCode
        {
            get { return _sysName; }
            set { _sysName = value; }
        }

        public string modelCode
        {
            get { return _objectName; }
            set { _objectName = value; }
        }
        /// <summary>
        /// 设计器中每条连线对应的属性对象(包括规则)
        /// </summary>
        public LineObject LineObjectData
        {
            get { return _lineObject; }
            set { _lineObject = value; }
        }

        public void ShowPropertyWindow(UIElement element)
        {
            _lineControl = element as LineControl;//旧的 
            if (_lineControl == null)
            {
                return;
            }
            #region 如果是新建连线就创对_lineObject对象
            if (_lineObject == null)
            {
                LineObject obj = LineObjects.Where(p => p.LineId.Equals( _lineControl.UniqueID)).SingleOrDefault();
                if (obj == null)
                {
                    _lineObject = new LineObject();
                    _lineObject.LineId = _lineControl.UniqueID;
                }
                else
                {
                    _lineObject = obj;
                }           
             
                UpdateLineObject(_lineObject); 
            }
            #endregion
            
        }
        /// <summary>
        /// 连线设置属性
        /// </summary>
        public void LoadProperty()
        {
            cboField.ItemsSource = null;   
            ConditionBind(_lineObject);
            UpdateLineObject(_lineObject);
            txtCompareValue.Text = "";
            OrganizationCondition();
            GetPostLevel();            
            if (cboField.ItemsSource == null && !string.IsNullOrWhiteSpace(_sysName) && !string.IsNullOrWhiteSpace(_objectName))
            {
                xmlClient.GetSystemBOAttributeListAsync(_sysName, _objectName);
            }
        }

        public void ConditionBind(LineObject activity)
        {
            var e = from a in LineObjects
                    where a.LineId == activity.LineId
                    select a;
            _conditionList = e.FirstOrDefault().ConditionList;
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = e.FirstOrDefault().ConditionList;
        }

        private void txtLineName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //_lineControl.Title = txtLineName.Text;
        }
        #region 定义规则      
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            FlowRuleDefineDialog dialog = new FlowRuleDefineDialog();
            dialog.Closed += new EventHandler(dialog_Closed);
            dialog.SysName = _sysName;
            dialog.ObjectName = _objectName;
            dialog.ConditionList = _lineObject == null ? null : _lineObject.ConditionList;
            dialog.LoadConditions();
            dialog.Show();
        }
        /// <summary>
        /// 定义规则返回事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dialog_Closed(object sender, EventArgs e)
        {
            FlowRuleDefineDialog dialog = sender as FlowRuleDefineDialog;

            if (dialog.DialogResult == true)
            {
                if (_lineObject == null)
                {
                    _lineObject = new LineObject();
                }

                _lineObject.ConditionList = dialog.ConditionList;
                UpdateLineObject(_lineObject);
                //SetRulesText();
            }
        }
        #endregion
       
        #region 清除规则       
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _lineObject.ConditionList = null;
            //SetRulesText();
        }
        #endregion

        /// <summary>
        /// 清除规则 供外部调用
        /// </summary>
        public void ClearCondition()
        {
            _lineObject.ConditionList = null;
            dgCodition.ItemsSource = null;
        }
        #region 如果活动不存在,就加入到集合里
        /// <summary>
        /// 更新连线的集合(如果连线不存在,就加入到集合里)
        /// </summary>
        /// <param name="activity">连线</param>
        /// <returns></returns>
        public void UpdateLineObject(LineObject activity)
        {
            var e = from a in LineObjects
                    where a.LineId == activity.LineId
                    select a;
            var ent = e.FirstOrDefault();
            if (ent != null)
            {
                //先删除后增加,保证数据是最新的
                LineObjects.Remove(ent);
                LineObjects.Add(activity);
            }
            else
            {
                LineObjects.Add(activity);
            }

        }
        #endregion

        private void but_Add_Click(object sender, RoutedEventArgs e)
        {
            if (_conditionList == null) _conditionList = new List<CompareCondition>();
            if (cboField.SelectedItem != null )
            {
                if (!string.IsNullOrEmpty(txtCompareValue.Text.Trim()))
                {
                    //if (_conditionList.Count > 0)
                    //{
                    //    ComfirmWindow.ConfirmationBox("提示信息", "只能设置一个条件！", "确定");
                    //}
                    //else
                    //{
                        CompareCondition newCondition = new CompareCondition()
                        {
                            Name = System.Guid.NewGuid().ToString(),
                            Description = (cboField.SelectedItem as WFBOAttribute).Description,
                            CompAttr = (cboField.SelectedItem as WFBOAttribute).Name,
                            DataType = (cboField.SelectedItem as WFBOAttribute).DataType,
                            Operate = ((System.Windows.Controls.ContentControl)(cboOperate.SelectedItem)).Tag.ToString(),
                            CompareValue = txtCompareValue.Text.Trim().ToString(),
                        };
                        var item = _conditionList.Where(p => p.Description == (cboField.SelectedItem as WFBOAttribute).Description).FirstOrDefault();
                        if (item == null || (item.CompareValue != newCondition.CompareValue && item.Operate != newCondition.Operate))
                        {
                            _conditionList.Add(newCondition);
                        }
                    //}
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "比较值不能为空！", "确定");
                }
            }
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = _conditionList;
            _lineObject.ConditionList = _conditionList;
            UpdateLineObject(_lineObject);
        }

        private void but_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCodition.SelectedItems.Count == 1)
            {
                CompareCondition ent = dgCodition.SelectedItem as CompareCondition;
                _conditionList.Remove(ent);
                dgCodition.ItemsSource = null;
                dgCodition.ItemsSource = _conditionList;
            }
            else
            {

                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录", "确定");
            }
        }

        #region 条件选择
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
            up.Width = 700;
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
                            || nameType == "EMPLOYEENAME"
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
            };
            up.Show();
            return;
        }
        private void IsStringDataType(string dataType)
        {          
            if (dataType.ToLower() == "string")
            {//如果是字符串类型，只能是等于==
                cboOperate.SelectedIndex = 1; 
                cboOperate.IsEnabled = false;
            }
            else
            {
                cboOperate.IsEnabled = true;
            }
        }
    

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

     
        #endregion

    }
}
