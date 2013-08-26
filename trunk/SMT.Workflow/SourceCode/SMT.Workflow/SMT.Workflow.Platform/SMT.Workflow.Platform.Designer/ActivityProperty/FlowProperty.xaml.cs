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
using System.Collections.ObjectModel;

using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.Workflow.Platform.Designer.Utils;
using System.Xml;
using System.Xml.Linq;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.Workflow.Platform.Designer.ActivityProperty
{
    public partial class FlowProperty : UserControl, IProperty
    {
        #region 公共定义
 
        /// <summary>
        /// 当前流程
        /// </summary>
        private FLOW_FLOWDEFINE_T _currentFlow = null;
   
        #endregion

        #region 属性定义

        /// <summary>
        /// 从Layout取出的缺省系统名
        /// </summary>
        public string SystemNameFromLayout { get; set; }

        /// <summary>
        /// 从Layout取出的缺省业务对象名
        /// </summary>
        public string ObjectNameFromLayout { get; set; }

        /// <summary>
        /// 当前流程视图（包括所有关系表）
        /// </summary>
        public V_FLOWDEFINITION CurrentFlowView
        {
            get;
            set;
        }
        /// <summary>
        /// 当前流程
        /// </summary>
        public FLOW_FLOWDEFINE_T CurrentFlow
        {
            get { return _currentFlow; }
            set { _currentFlow = value; }
        }
     
         #endregion

        #region 构造函数
        public FlowProperty()
        {
            InitializeComponent();        
            
        }       
        #endregion
     

        #region 绑定系统下拉框
        /// <summary>
        ///绑定下拉列表
        /// </summary>
        private void BindSystem()
        {    
            cbSystemCode.ItemsSource = FlowSystemModel.appSystem;
            cbSystemCode.DisplayMemberPath = "SYSTEMNAME";
            cbSystemCode.SelectedIndex = 0;
            cbModelCode.ItemsSource = FlowSystemModel.appModel;
            cbModelCode.DisplayMemberPath = "DESCRIPTION";
            if (CurrentFlowView.ModelRelation != null)
            {
                string systemCode = CurrentFlowView.ModelRelation.SYSTEMCODE;
                var appSystem = FlowSystemModel.appSystem.Where(p => p.SYSTEMCODE.Equals(systemCode)).FirstOrDefault();
                cboFlowType.SelectedIndex = (_currentFlow.FLOWTYPE == "0") ? 0 : 1; //绑定流程类型
                if (appSystem != null)
                {
                    cbSystemCode.SelectedIndex = FlowSystemModel.appSystem.IndexOf(appSystem);
                }
                else
                {
                    cbSystemCode.SelectedIndex = 0;
                }
                string modelCode = CurrentFlowView.ModelRelation.MODELCODE;
                var appModel = FlowSystemModel.appModel.Where(p => p.MODELCODE.Equals(modelCode)).FirstOrDefault();
                if (appModel != null)
                {
                    this.cbModelCode.SelectedIndex = FlowSystemModel.appModel.Where(p => p.SYSTEMCODE.Equals(systemCode)).ToList().IndexOf(appModel)+1;
                }
                else
                {
                    this.cbModelCode.SelectedIndex = 0;
                }
                txtCompanyName.Text = CurrentFlowView.ModelRelation.COMPANYNAME;
                txtDepartmentName.Text = CurrentFlowView.ModelRelation.DEPARTMENTNAME != null ? CurrentFlowView.ModelRelation.DEPARTMENTNAME : "";
            }          
        }
        #endregion       

    


        #region 显示流程属性
        /// <summary>
        /// 显示流程属性
        /// </summary>
        /// <param name="element"></param>
        public void ShowPropertyWindow(UIElement element)
        {
            //
        }
          #endregion

        #region 加载属性
       
        /// 加载属性
        /// </summary>
        public void LoadProperty()
        {
            btnGetDepartment.IsEnabled = true;      
            BindSystem();        
            if (CurrentFlow == null)
            {
                return;
            }

            txtFlowName.Text = CurrentFlow.DESCRIPTION;//流程名称
            txtCreateUser.Text = CurrentFlow.CREATEUSERNAME;
            txtCreateDate.Text =CurrentFlow.CREATEDATE==null?"":DateTime.Parse(CurrentFlow.CREATEDATE.ToString()).ToLongDateString();

            if (!string.IsNullOrEmpty(CurrentFlow.EDITUSERNAME))
            {
                txtUpdateUser.Text = CurrentFlow.EDITUSERNAME;
                txtUpdateDate.Text = CurrentFlow.EDITDATE == null ? "" : DateTime.Parse(CurrentFlow.EDITDATE.ToString()).ToLongDateString();
            }
            #region         
          
         #endregion        
        }
        #endregion
      
       

        #region   选择业务系统（基本设置）触发事件
        private void cboSystem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null)
            {
                if (FlowSystemModel.appModel != null && cbSystemCode.SelectedIndex>0)
                {
                    var models = from ent in FlowSystemModel.appModel
                                 where ent.SYSTEMCODE == item.SYSTEMCODE || ent.MODELCODE == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cbModelCode.ItemsSource = models;
                        this.cbModelCode.DisplayMemberPath = "DESCRIPTION";
                        this.cbModelCode.SelectedIndex = 0;
                    }
                }
                else
                {
                    ObservableCollection<FLOW_MODELDEFINE_T> list = new ObservableCollection<FLOW_MODELDEFINE_T>();
                    list.Insert(0, new FLOW_MODELDEFINE_T() { MODELCODE = "0", DESCRIPTION = "请选择......" });
                    this.cbModelCode.ItemsSource = list;
                    this.cbModelCode.DisplayMemberPath = "DESCRIPTION";
                    this.cbModelCode.SelectedIndex = 0;
                }
            }
        }

        private void cbModelCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (cbModelCode.SelectedIndex > 0)
            {
                this.txtFlowName.Text = item.DESCRIPTION;
            }
        }
        #endregion
     
        // 多个公司,多个部门(不合理)
        // 多个公司,一个部门(不合理)
        // 一个公司,多个部门,如果不属同一公司(不合理)
        
        // 选择多个公司,不能选择部门(提示后,清除部门)
        // 选择多个部门,只能选择一间公司(提示后,清除部门)

        #region 选取公司
        /// <summary>
        /// 是否有选择多个公司
        /// </summary>
        bool isManyCompany = false;//是否有选择多个公司
        private void btnGetCompany_Click(object sender, RoutedEventArgs e)
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
            lookup.MultiSelected = true;
            lookup.SelectedClick += (obj, ev) =>
            {
                if (lookup.SelectedObj != null)
                {
                    #region 判断是否可以选择部门
                    if (lookup.SelectedObj.Count > 1)
                    {
                        if (!string.IsNullOrEmpty(CurrentFlowView.ModelRelation.DEPARTMENTID))
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "选择多个公司不能选择部门", "确定");
                        }
                        isManyCompany = true;
                        txtDepartmentName.Text = "";
                        CurrentFlowView.ModelRelation.DEPARTMENTID = null;//部门ID
                        CurrentFlowView.ModelRelation.DEPARTMENTNAME = null;//部门名称
                        btnGetDepartment.IsEnabled = false;
                    }
                    else
                    {
                        var companyObj = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(lookup.SelectedObj[0].ObjectInstance);
                     
                        string companyid = companyObj.COMPANYID;
                        if (CurrentFlowView.ModelRelation.COMPANYID.TrimEnd('|') != companyid)
                        {//选择的不是同一个公司
                            txtDepartmentName.Text = "";
                            CurrentFlowView.ModelRelation.DEPARTMENTID = null;//部门ID
                            CurrentFlowView.ModelRelation.DEPARTMENTNAME = null;//部门名称 
                            isManyCompany = false;
                            btnGetDepartment.IsEnabled = true;
                        }
                        else
                        {
                            isManyCompany = false;
                            btnGetDepartment.IsEnabled = true;
                            return;
                        }                    
                    }
                    #endregion
                    txtCompanyName.Text = "";
                    CurrentFlowView.ModelRelation.COMPANYID = "";
                    CurrentFlowView.ModelRelation.COMPANYNAME = "";
                   
                    foreach (var item in lookup.SelectedObj)
                    {
                        var companyObj = ((SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance));
                        string companyname = companyObj.CNAME;
                        string briefname = companyObj.BRIEFNAME;//简称
                        string companyid = companyObj.COMPANYID;
                        if (string.IsNullOrEmpty(briefname))
                        {
                            briefname = companyname;
                        }
                        txtCompanyName.Text += briefname + "\r\n";
                        CurrentFlowView.ModelRelation.COMPANYID += companyid + "|";//公司ID
                        CurrentFlowView.ModelRelation.COMPANYNAME += briefname + "|";//公司名称
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

        #region 选取部门
        private void btnGetDepartment_Click(object sender, RoutedEventArgs e)
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
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.MultiSelected = true;
            lookup.SelectedClick += (obj, ev) =>
            {
                if (lookup.SelectedObj != null)
                {
                    #region 判断是否是属于同一个公司,如果不是提示,返回
                    foreach (var item in lookup.SelectedObj)
                    {
                        var deparement = ((SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)(item.ObjectInstance));
                        string companyid = deparement.T_HR_COMPANY.COMPANYID;            
                        string departmentname = deparement.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                       
                        if (CurrentFlowView.ModelRelation.COMPANYID.TrimEnd('|') != companyid)
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", departmentname + " 不属于 " + txtCompanyName.Text + "公司,请重新选择!", "确定");
                            return;
                        }
                    }
                    #endregion
                    txtDepartmentName.Text = "";
                    CurrentFlowView.ModelRelation.DEPARTMENTID = "";//部门ID
                    CurrentFlowView.ModelRelation.DEPARTMENTNAME = "";//部门名称
                    foreach (var item in lookup.SelectedObj)
                    {
                        
                        var deparement = ((SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)(item.ObjectInstance));
                     
                        string departmentcode = deparement.DEPARTMENTID;
                        string departmentname = deparement.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                     
                        txtDepartmentName.Text += departmentname + "\r\n";
                        CurrentFlowView.ModelRelation.DEPARTMENTID += departmentcode + "|";//部门ID
                        CurrentFlowView.ModelRelation.DEPARTMENTNAME += departmentname + "|";//部门名称
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

        #region 清除部门
      
        private void btnClearDepartment_Click(object sender, RoutedEventArgs e)
        {
            txtDepartmentName.Text = "";
            CurrentFlowView.ModelRelation.DEPARTMENTID = null;//部门ID
            CurrentFlowView.ModelRelation.DEPARTMENTNAME = null;//部门名称
        }
        #endregion

    
    }
}
