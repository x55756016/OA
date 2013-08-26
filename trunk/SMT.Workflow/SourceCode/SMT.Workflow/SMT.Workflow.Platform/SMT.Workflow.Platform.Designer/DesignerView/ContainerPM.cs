/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：ContainerPM.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/17 9:27:27   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerView 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/

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
using SMT.Workflow.Platform.Designer.Utils;
using System.Xml.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.PlatformService;

using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.DesignerControl;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.DesignerView
{
    public partial class Container : UserControl, IContainer
    {
        /// <summary>
        /// 提示信息
        /// </summary>
        string alertMsg = "修改";
        #region 事件的定义（龙康才）
        /// <summary>
        ///  改变树控件的流程数据事件（新增、更新、删除）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="option">操作选项：add,update,delete</param>
        public delegate void OnClicked(object sender, TreeItem args, string option);
        /// <summary>
        ///  改变树控件的流程数据完成事件（新增、更新、删除）
        /// </summary>
        //public event OnClicked OnSubmitComplated; //注册事件
        #endregion
        #region 变量定义
        /// <summary>
        /// 流程服务定义
        /// </summary>
        FlowDefineClient clientFlow = new FlowDefineClient();

        //XML Service 定义
        private FlowXmlDefineClient _xmlClient = new FlowXmlDefineClient();

        /// <summary>
        /// 当前流程定义及关联
        /// </summary>
        V_FLOWDEFINITION _currentFlow = null;
        /// <summary>
        /// 连线属性集合
        /// </summary>
        private IList<LineObject> _lineObjects = new List<LineObject>();

        /// <summary>
        /// 活动属性集合
        /// </summary>
        private IList<ActivityObject> _activityObjects = new List<ActivityObject>();
     
      

        //==added by jason, 02/24/2012
        //==以下2个变量为了兼容导入的流程，缺省从Layout中取出系统名、业务对象名
        private string _systemNameFromLayout = string.Empty;
        private string _objectNameFromLayout = string.Empty;
        //==end added by jason

        #endregion
        #region 属性定义

       
        /// <summary>
        /// 当前流程
        /// </summary>
        public V_FLOWDEFINITION CurrentFlow
        {
            get { return _currentFlow; }
            set { _currentFlow = value; }
        }
        #endregion
        /// <summary>
        /// 全局提示信息
        /// </summary>
        string tipMsg = "";
        /// <summary>
        /// 保存后返回来的FLOWCODE
        /// </summary>
        string refFlowCode = "";
        /// <summary>
        /// 记录当前保存的FLOWCODE
        /// </summary>
        string currFlowCode = "";
        /// <summary>
        /// 记录当前保存的FLOWCODE<旧,新>
        /// </summary>
        Dictionary<string, string> saveFlowCode = new Dictionary<string, string>();
        /// <summary>
        /// 记录旧的flowcode与新的 flowcode,通过旧flowcode,返回对应新的flowcode
        /// </summary>
        /// <param name="saveFlowCode"></param>
        /// <param name="newFlowCode"></param>
        /// <returns></returns>
        private string GetFlowCode(Dictionary<string, string> saveFlowCode, string currFlowCode)
        {
            string flowcode = "";
            foreach (KeyValuePair<string, string> code in saveFlowCode)
            {
                if (code.Key == currFlowCode)
                {
                   flowcode= code.Value;
                   return flowcode;
                }
            }
            return currFlowCode;
        }
        #region 注册服务
        /// <summary>
        /// 注册服务
        /// </summary>
        private void RegisterServices()
        {
            //注册流程服务事件
            clientFlow.AddFlowDefineCompleted += new EventHandler<AddFlowDefineCompletedEventArgs>(clientFlow_AddFlowDefineCompleted);
            clientFlow.GetFlowEntityCompleted += new EventHandler<GetFlowEntityCompletedEventArgs>(clientFlow_GetFlowEntityCompleted);
            //_flowClient.CreateFlowDefineCompleted += new EventHandler<CreateFlowDefineCompletedEventArgs>(_flowClient_CreateFlowDefineCompleted);
            //_flowClient.AddFlowDefineCompleted += new EventHandler<AddFlowDefineCompletedEventArgs>(_flowClient_AddFlowDefineCompleted);
            //_flowClient.RetrieveFlowDefinitionCompleted += new EventHandler<RetrieveFlowDefinitionCompletedEventArgs>(_flowClient_RetrieveFlowDefinitionCompleted);

            ////注册业务对象事件
            //_xmlClient.ListSystemCompleted += new EventHandler<ListSystemCompletedEventArgs>(_xmlClient_ListSystemCompleted);
        }

    

     
        #endregion

        #region 新建流程
        int newtag = 1;//0是新退,其它是修改
        /// <summary>
        /// 新建流程
        /// </summary>
        public void AddFlow()
        {
            newtag = 0;
            _uniqueID = Guid.NewGuid().ToString().Replace("-", "");
            ClearData();
            alertMsg = "新增";
        }
        /// <summary>
        /// 清除容器
        /// </summary>
        public void ClearFlow()
        {
          
            ClearData();
            this.CopyedElements.Clear();
            this.NeedMoveUnFocusLines.Clear();
            this.SelectedElementRemoveAll();
            this.Elements.Clear();
            cnsDesignerContainer.Children.Clear();
            this.ucFlowSetting.lineProperty.ClearCondition();//清除规则
            this.ucFlowSetting.activityProperty.ClearActivity();
            this.SetGridLines();
        }
        private void ClearData()
        {
            _lastSelectedElement = null;
            _lineObjects = new List<LineObject>();
            _activityObjects = new List<ActivityObject>();
            if (this.CurrentFlow == null)
            {
                this.CurrentFlow = new V_FLOWDEFINITION();
                this._currentFlow = new V_FLOWDEFINITION();
                this.ucFlowSetting.flowProperty.CurrentFlow = new FLOW_FLOWDEFINE_T();
                this.ucFlowSetting.flowProperty.CurrentFlowView = new V_FLOWDEFINITION();
                this.ucFlowSetting.flowProperty.CurrentFlowView.FlowDefinition = new FLOW_FLOWDEFINE_T();
                this.ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation = new FLOW_MODELFLOWRELATION_T();
            }
            this.CleareContainer();
            this.ucFlowSetting.activityProperty.ActivityObjects.Clear();//新建时清空活动数据
            this.ucFlowSetting.lineProperty.LineObjects.Clear();//新建时清空连线数据



            this.ucFlowSetting.flowProperty.CurrentFlow.FLOWCODE = _uniqueID;
            this.ucFlowSetting.flowProperty.CurrentFlowView.FlowDefinition.FLOWCODE = _uniqueID;


            //关联设置
            this.ucFlowSetting.flowProperty.txtCompanyName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            this.ucFlowSetting.flowProperty.txtDepartmentName.Text = "";
            this.ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//公司ID
            this.ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;//公司名称
            this.ucFlowSetting.flowProperty.txtCreateDate.Text = DateTime.Now.ToString();
            FLOW_MODELDEFINE_T modelCode = ucFlowSetting.flowProperty.cbModelCode.SelectedItem as FLOW_MODELDEFINE_T;
            if (modelCode != null)
            {
                this.ucFlowSetting.flowProperty.txtFlowName.Text = modelCode.DESCRIPTION;
            }
            else
            {
                this.ucFlowSetting.flowProperty.txtFlowName.Text = "";
            }
            this.ucFlowSetting.flowProperty.txtUpdateDate.Text = DateTime.Now.ToString();
            this.ucFlowSetting.flowProperty.txtCreateUser.Text = Utility.CurrentUser.USERNAME;
            this.ucFlowSetting.flowProperty.txtUpdateUser.Text = Utility.CurrentUser.USERNAME;

            this.ucFlowSetting.flowProperty.CurrentFlow.DESCRIPTION = this.ucFlowSetting.flowProperty.txtFlowName.Text; //流程名称
            this.ucFlowSetting.flowProperty.CurrentFlow.CREATEUSERNAME = Utility.CurrentUser.USERNAME;
            this.ucFlowSetting.flowProperty.CurrentFlow.CREATEDATE = DateTime.Now;

            this.ucFlowSetting.flowProperty.LoadProperty();
        }
        #endregion
        #region 保存流程
        /// <summary>
        /// 保存流程
        /// </summary>
        public void SaveFlow()
        {

            if (Elements.Count <= 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "不能保存不完整的流程", "确定");
                return;
            }

            string errMsg = CheckFlow().Trim();
            if (errMsg.Trim() != "")
            {
                ComfirmWindow.ConfirmationBox("提示信息", errMsg, "确定");
                return;
            }
            #region 是否选择了多个公司或多个部门
            if (ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation == null)
            {
                //MessageBox.Show("关联设置－〉所属公司：不能为空。", "警告：", MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBox("提示信息", "所属公司：不能为空!", "确定");
                return;
            }
            else
            {
                SaveDataToDataBase();
                #region 注释掉以前的判断
                //string[] companys = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID.TrimEnd('|').Split('|');
                //string[] companyNames = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYNAME.TrimEnd('|').Split('|');
                //string[] deparmentids = { };
                //string[] deparmentidNames = { };
                //if (!string.IsNullOrEmpty(ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTID))
                //{
                //    deparmentids = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTID.TrimEnd('|').Split('|');
                //    deparmentidNames = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTNAME.TrimEnd('|').Split('|');
                //}
                //if (companys.Length > 1)
                //{//如果是多个公司
                //    for (int i = 0; i < companyNames.Length; i++)
                //    {
                //        if (ucFlowSetting.flowProperty.txtFlowName.Text.Trim().IndexOf(companyNames[i]) > 0)
                //        {
                //            ComfirmWindow.ConfirmationBox("提示信息", "请删除流程名称中的 \"" + companyNames[i] + "\" 再保存,系统会自动在流程名称后面加上", "确定");
                //            return;
                //        }
                //    }
                //}
                //else
                //{//一个公司
                //    if (deparmentidNames.Length > 0)
                //    {//有部门
                //        for (int i = 0; i < companyNames.Length; i++)
                //        {
                //            if (ucFlowSetting.flowProperty.txtFlowName.Text.Trim().IndexOf(companyNames[i]) > 0)
                //            {
                //                ComfirmWindow.ConfirmationBox("警告,确定流程名称中没有公司,部门名称", "请删除流程名称中的 \"" + companyNames[i] + "\" 再保存,系统会自动在流程名称后面加上", "确定");
                //                return;
                //            }
                //        }
                //        for (int i = 0; i < deparmentidNames.Length; i++)
                //        {
                //            if (ucFlowSetting.flowProperty.txtFlowName.Text.Trim().IndexOf(deparmentidNames[i]) > 0)
                //            {
                //                ComfirmWindow.ConfirmationBox("警告,确定流程名称中没有公司,部门名称", "请删除流程名称中的 \"" + deparmentidNames[i] + "\" 再保存,系统会自动在流程名称后面加上", "确定");
                //                return;
                //            }
                //        }
                //    }
                //}
                //if (companys.Length > 1 || deparmentids.Length > 1)
                //{
                //    ComfirmWindow com = new ComfirmWindow();
                //    com.OnSelectionBoxClosed += (obj, result) =>
                //    {
                //        SaveDataToDataBase();
                //    };
                //    com.SelectionBox("提示信息", "选择多公司或多部门,系统会自动加上(公司->部门).例如:事项审批(公司->部门)", ComfirmWindow.titlename, "");
                //}
                //else
                //{

                //}
                #endregion
            }
            #endregion
        }
        /// <summary>
        /// 保存数据到数据库
        /// </summary>
        private void SaveDataToDataBase()
        {
            try
            {
                _activityObjects = this.ucFlowSetting.activityProperty.ActivityObjects;//新建时活动数据
                _lineObjects = this.ucFlowSetting.lineProperty.LineObjects;//新建时连线数据
                //刷新上次选中元素的属性数据
                RefreshFlowData();
                #region 判断完成整性
                foreach (var role in _activityObjects)
                {
                    if (role.IsCounterSign)
                    {
                        if (role.CounterSignRoleList==null || role.CounterSignRoleList.Count < 1)
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "【会签节点】的角色不能为空，请添加角色！", "确定");
                            return;
                        }
                    }
                    if ((role.IsSpecifyCompany == true && role.OtherCompanyId.Trim() == "") || (role.IsSpecifyCompany == false && role.OtherCompanyId.Trim() != ""))
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "【" + role.Remark + "】指定了公司【" + role .OtherCompanyName+ "】，但没有钩选中，请重新钩选！", "确定");
                        return;
                    }                   
                    #region 会签角色判断是否有指定公司
                    if (role.CounterSignRoleList != null && role.CounterSignRoleList.Count > 0)
                    {
                        string warlName = "";
                        foreach (var roleSingn in role.CounterSignRoleList)
                        {
                            if ((roleSingn.IsOtherCompany == true && roleSingn.OtherCompanyId.Trim() == "") || (roleSingn.IsOtherCompany == false && roleSingn.OtherCompanyId.Trim() != ""))
                            {
                                warlName += "【" + roleSingn.StateName + "】指定了公司【" + roleSingn.OtherCompanyName + "】，但没有钩选中，请重新钩选！\r\n";
                            }
                        }
                        if (warlName.Trim() != "")
                        {
                            ComfirmWindow.ConfirmationBox("会签角色提示信息", warlName, "确定");
                            return;
                        }
                    }
        //                    IsOtherCompany	false	bool
        //OtherCompanyId	"1a1f745c-e0df-4d21-b000-8d80d91b1743"	string
        //OtherCompanyName	null	string
        //StateCode	"da662417-baf1-4dab-ba34-5ae648c2047b"	string
        //StateName	"副总经理"	string
        //TypeCode	"CREATEUSER"	string
        //TypeName	"单据所有者"	string

                    #endregion
                }
                if (ucFlowSetting.flowProperty.txtFlowName.Text.Trim() == "")
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "流程名称不能为空!", "确定");
                    return;
                }
                if (ucFlowSetting.flowProperty.cbSystemCode.SelectedIndex < 1)
                {
                    //MessageBox.Show("关联设置－〉业务系统：不能为空。", "警告：", MessageBoxButton.OK);
                    ComfirmWindow.ConfirmationBox("提示信息", "所属系统：不能为空!", "确定");
                    return;
                }
                if (ucFlowSetting.flowProperty.cbModelCode.SelectedIndex < 1)
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "所属模块：不能为空!", "确定");
                    return;
                }

                if (ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation == null)
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "所属公司：不能为空!", "确定");
                    return;
                }
                if (ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation != null && string.IsNullOrEmpty(ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID))
                {
                    //MessageBox.Show("关联设置－〉所属公司：不能为空。", "警告：", MessageBoxButton.OK);
                    ComfirmWindow.ConfirmationBox("提示信息", "所属公司：不能为空!", "确定");
                    return;
                }
                #endregion

                #region 系统与模块的选择             
                FLOW_MODELDEFINE_T systemCode = ucFlowSetting.flowProperty.cbSystemCode.SelectedItem as FLOW_MODELDEFINE_T;
              
                FLOW_MODELDEFINE_T modelCode = ucFlowSetting.flowProperty.cbModelCode.SelectedItem as FLOW_MODELDEFINE_T;
              
                #endregion
                string strWFLayout = ToXmlString();
                string strXOML = GetXoml(_uniqueID);// ToXoml(_uniqueID);
                string strRules = ToRule(systemCode.SYSTEMCODE, modelCode.MODELCODE);
                string strLayout = GetLayout();// ToLayout();
                //工作流程定义
                #region 工作流程定义
                FLOW_FLOWDEFINE_T flowDefine = new FLOW_FLOWDEFINE_T();
                flowDefine.FLOWDEFINEID = Guid.NewGuid().ToString().Replace("-", ""); ;//暂时存在数据库,但没有用到
                flowDefine.FLOWCODE = _uniqueID;               
                flowDefine.DESCRIPTION = ucFlowSetting.flowProperty.txtFlowName.Text;
                flowDefine.XOML = strXOML;
                flowDefine.RULES = strRules;
                flowDefine.LAYOUT = strLayout;
                flowDefine.WFLAYOUT = strWFLayout;
                flowDefine.FLOWTYPE = ucFlowSetting.flowProperty.cboFlowType.SelectedIndex.ToString();
                flowDefine.CREATEUSERID = Utility.CurrentUser.OWNERID;
                flowDefine.CREATEUSERNAME = Utility.CurrentUser.USERNAME;
                flowDefine.CREATECOMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
                flowDefine.CREATEDEPARTMENTID = Utility.CurrentUser.OWNERDEPARTMENTID;
                flowDefine.CREATEPOSTID = Utility.CurrentUser.OWNERPOSTID;
                flowDefine.CREATEDATE = newtag == 0 ? DateTime.Now : ucFlowSetting.flowProperty.CurrentFlowView.FlowDefinition.CREATEDATE;
                flowDefine.EDITUSERID = Utility.CurrentUser.UPDATEUSER;
                flowDefine.EDITUSERNAME = Utility.CurrentUser.USERNAME;
                flowDefine.EDITDATE = DateTime.Now;
                flowDefine.SYSTEMCODE = systemCode.SYSTEMCODE;
                flowDefine.BUSINESSOBJECT = modelCode.MODELCODE;
                #endregion
                //模块定义
                #region 模块定义
                FLOW_MODELFLOWRELATION_T flowRealtion = new FLOW_MODELFLOWRELATION_T();
                flowRealtion.MODELFLOWRELATIONID = newtag == 0 ? Guid.NewGuid().ToString().Replace("-", "") : ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.MODELFLOWRELATIONID;
                flowRealtion.COMPANYID = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID;//公司ID
                flowRealtion.DEPARTMENTID = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTID; //部门Id
                flowRealtion.COMPANYNAME = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYNAME;//公司名称 
                flowRealtion.DEPARTMENTNAME = ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTNAME;//部门名称 
                flowRealtion.SYSTEMCODE = systemCode.SYSTEMCODE; //系统代码 
                flowRealtion.MODELCODE = modelCode.MODELCODE;              
                flowRealtion.FLOWCODE = _uniqueID;
                flowRealtion.FLAG = "1";
                flowRealtion.FLOWTYPE = ucFlowSetting.flowProperty.cboFlowType.SelectedIndex.ToString();
                flowRealtion.CREATEUSERID = Utility.CurrentUser.OWNERID;
                flowRealtion.CREATEUSERNAME = Utility.CurrentUser.USERNAME;
                flowRealtion.CREATECOMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
                flowRealtion.CREATEDEPARTMENTID = Utility.CurrentUser.OWNERDEPARTMENTID;
                flowRealtion.CREATEPOSTID = Utility.CurrentUser.OWNERPOSTID;
                flowRealtion.CREATEDATE = DateTime.Now;
                flowRealtion.EDITUSERID = Utility.CurrentUser.UPDATEUSER;
                flowRealtion.EDITUSERNAME = Utility.CurrentUser.USERNAME;
                flowRealtion.EDITDATE = DateTime.Now;
                #endregion
                _currentFlow = new V_FLOWDEFINITION()
                {
                    FlowDefinition = flowDefine,
                    ModelRelation = flowRealtion

                };
                ucFlowSetting.flowProperty.CurrentFlowView = _currentFlow;
                pBar.Start();            
                clientFlow.AddFlowDefineAsync(_currentFlow);              
            }
            catch (Exception e)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "出错信息如下：\r\n" + e.ToString(), "确定");
            }
        }
      
        #endregion
        #region 执行WCF服务完成事件

        /// <summary>
        /// 加载业务系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _xmlClient_ListSystemCompleted(object sender, ListSystemCompletedEventArgs e)
        {
            //try
            //{
            if (e.Result != null)
            {
                FlowSystemModel.ListAppSystem = e.Result.ToList();

                //_flowClient.RetrieveFlowDefinitionAsync(_uniqueID);
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message.ToString());
            //}
        }
        #region 新建流程定义完成事件
        void clientFlow_AddFlowDefineCompleted(object sender, AddFlowDefineCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "1")
                {

                    ComfirmWindow.ConfirmationBox("提示信息", alertMsg + "成功!\r\n", "确定");
                    newtag = 1;//新增成功后变成修改
                    pBar.Stop();
                    alertMsg = "修改";
                }
                else
                {
                    //MessageBox.Show(alertMsg+"失败！"+e.failMessage, "提示！", MessageBoxButton.OK);
                    ComfirmWindow.ConfirmationBox("提示信息", alertMsg + "失败!" + e.Result, "确定");
                    if (alertMsg == "新增")
                    {
                        alertMsg = "新增";
                    }
                    pBar.Stop();
                }
            }
            tipMsg = "";//清空提示信息
           
        }
       
        #endregion

        /// <summary>
        /// 获取指定流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientFlow_GetFlowEntityCompleted(object sender, GetFlowEntityCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _currentFlow = e.Result as V_FLOWDEFINITION;
                ucFlowSetting.flowProperty.CurrentFlow = _currentFlow.FlowDefinition;
                ucFlowSetting.flowProperty.CurrentFlowView = _currentFlow;
                ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation = _currentFlow.ModelRelation;
                if (ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation != null)
                {//绑定初始化           
                    if (!string.IsNullOrEmpty(ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID))
                    {
                        ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID = _currentFlow.ModelRelation.COMPANYID;//公司ID
                        ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYNAME = _currentFlow.ModelRelation.COMPANYNAME;//公司名称     
                    }
                    else
                    {//如果没有绑定公司，则以当前登录人公司进行绑定
                        ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//公司ID
                        ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.COMPANYNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;//公司名称
                    }
                    if (!string.IsNullOrEmpty(_currentFlow.ModelRelation.DEPARTMENTID))
                    {//如果绑定部门不为空
                        ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTID = _currentFlow.ModelRelation.DEPARTMENTID;//部门ID
                        ucFlowSetting.flowProperty.CurrentFlowView.ModelRelation.DEPARTMENTNAME = _currentFlow.ModelRelation.DEPARTMENTNAME;//部门名称
                    }
                }
                InitFlowContainer();
            }
            pBar.Stop();
        }      
        #endregion

        #region 初始化容器
        /// <summary>
        /// 初始化容器
        /// </summary>
        private void InitFlowContainer()
        {

            CleareContainer();
            if (_currentFlow.FlowDefinition.WFLAYOUT.Length > 100)
            {
                LoadFromXmlString(_currentFlow.FlowDefinition.WFLAYOUT, true);
            }
            else
            {
                LoadFromXmlString(_currentFlow.FlowDefinition.LAYOUT, false);
            }

            GetFlowDefine();
            ucFlowSetting.activityProperty.ActivityObjects = _activityObjects;//当前流程活动的集合
            ucFlowSetting.lineProperty.LineObjects = _lineObjects;//当前连线的集合
            ucFlowSetting.ShowProperty(null);
            //==added by jason, 02/24/2012
            ucFlowSetting.flowProperty.SystemNameFromLayout = _systemNameFromLayout;
            ucFlowSetting.flowProperty.ObjectNameFromLayout = _objectNameFromLayout;
            ucFlowSetting.flowProperty.CurrentFlowView = _currentFlow;         
            ucFlowSetting.flowProperty.LoadProperty();
        }


        /// <summary>
        /// 加载流程
        /// </summary>
        /// <param name="flowCode">流程编码</param>
        public void LoadFlow(string flowCode)
        {
            alertMsg = "修改";
            pBar.Start();

            _lastSelectedElement = null;
            _uniqueID = flowCode;
            if (FlowSystemModel.appSystem != null)
            {
                clientFlow.GetFlowEntityAsync(_uniqueID);               
            }           
        }
        #endregion
        #region
       

        /// <summary>
        /// 初始化属性数据
        /// </summary>
        private void InitPropertyData()
        {

            if (SelectedElements.Count <= 0) return;

            UIElement element = SelectedElements[0];
            string id = ((IControlBase)element).UniqueID;

            if (((IControlBase)element).Type == ElementType.Activity)
            {
                ucFlowSetting.activityProperty.ActivityObjectData = _activityObjects.Where(p => p.ActivityId.Equals(id)).SingleOrDefault();
            }
            else if (((IControlBase)element).Type == ElementType.Line)
            {
                ucFlowSetting.lineProperty.LineObjectData = _lineObjects.Where(p => p.LineId.Equals(id)).SingleOrDefault();
            }
        }

        /// <summary>
        /// 刷新流程元素数据
        /// </summary>
        private void RefreshFlowData()
        {
            if (_lastSelectedElement == null) return;

            if (((IControlBase)_lastSelectedElement).Type == ElementType.Activity)
            {
                //刷性活动属性中的数据
                RefreshActivityData(((IControlBase)_lastSelectedElement).UniqueID);
            }
            else if (((IControlBase)_lastSelectedElement).Type == ElementType.Line)
            {
                //刷性连线属性中的数据
                RefreshLineData(((IControlBase)_lastSelectedElement).UniqueID);
            }
        }

        /// <summary>
        /// 刷新活动数据
        /// </summary>
        /// <param name="id"></param>
        private void RefreshActivityData(string id)
        {
            ActivityObject activityObject = _activityObjects.Where(p => p.ActivityId.Equals(id)).SingleOrDefault();

            if (activityObject != null) _activityObjects.Remove(activityObject);

            activityObject = new ActivityObject()
            {
                ActivityId = id,
                IsCounterSign = (bool)ucFlowSetting.activityProperty.chkGroupAudit.IsChecked ? true : false,
                RoleId = ucFlowSetting.activityProperty.cboRoles.SelectedItem == null ? "" : (ucFlowSetting.activityProperty.cboRoles.SelectedItem as StateType).StateCode,
                UserType = ucFlowSetting.activityProperty.cboUserType.SelectedItem == null ? "" : (ucFlowSetting.activityProperty.cboUserType.SelectedItem as UserType).TypeCode,
                IsSpecifyCompany = (bool)ucFlowSetting.activityProperty.cbOtherCompany.IsChecked ? true : false,
                CounterType = ucFlowSetting.activityProperty.cboRule.SelectedIndex.ToString(),
                CounterSignRoleList = ucFlowSetting.activityProperty.ActivityObjectData == null ? null : ucFlowSetting.activityProperty.ActivityObjectData.CounterSignRoleList,
            };

            _activityObjects.Add(activityObject);
        }

        /// <summary>
        /// 取得容器中线段数
        /// </summary>
        /// <returns></returns>
        private void GetLineIdentitys()
        {
            int n = this.cnsDesignerContainer.Children.Count;
            for (int i = 0; i < n; i++)
            {
                var el = this.cnsDesignerContainer.Children[i];

            }
            List<LineControl> lineList = new List<LineControl>();
            List<ActivityControl> activityList = new List<ActivityControl>();
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).Type == ElementType.Line)
                {
                    lineList.Add(((LineControl)(this.Elements[i])));
                }
                if (((IControlBase)this.Elements[i]).Type == ElementType.Activity)
                {
                    activityList.Add(((ActivityControl)(this.Elements[i])));
                }
            }
            var o = ucFlowSetting.lineProperty.LineObjectData.ConditionList.Count;

        }

        /// <summary>
        /// 刷新连线数据
        /// </summary>
        /// <param name="id"></param>
        private void RefreshLineData(string id)
        {
            LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(id)).SingleOrDefault();

            if (lineObject != null) _lineObjects.Remove(lineObject);

            lineObject = new LineObject()
            {
                LineId = id,
                ConditionList = ucFlowSetting.lineProperty.LineObjectData == null ? null : ucFlowSetting.lineProperty.LineObjectData.ConditionList,
            };

            _lineObjects.Add(lineObject);
        }

        #region xml导入

        //xml方法
        private string ToXmlString()
        {
            System.Text.StringBuilder xml = new System.Text.StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes"" ?>");
            xml.Append(Environment.NewLine);
            xml.Append(@"<SMT.Workflow.Platform");
            xml.Append(@" UniqueID=""" + this.UniqueID + @"""");
            xml.Append(@" Name=""" + this.Name + @""">");
            xml.Append(Environment.NewLine);

            xml.Append(@"    <Elements>");
            for (int i = 0; i < this.Elements.Count; i++)
            {
                xml.Append(Environment.NewLine);
                xml.Append(((IControlBase)this.Elements[i]).ToXmlString());
            }
            xml.Append(Environment.NewLine);
            xml.Append(@"    </Elements>");

            xml.Append(Environment.NewLine);
            xml.Append(@"</SMT.Workflow.Platform>");
            return xml.ToString();
        }

        private void LoadFromXmlString(string xmlString, bool istrue)
        {
            svDesigner.ScrollToVerticalOffset(0);//加载新流程滚动条复位
            svDesigner.ScrollToHorizontalOffset(0);//加载新流程滚动条复位
            if (string.IsNullOrEmpty(xmlString)) return;
            Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(xmlString);
            XElement xElement = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
            var lines = from item in xElement.Descendants("Rule") select item;
            string left = string.Empty;
            string top = string.Empty;
            int zIndex = 0;
            #region 测试
            if (istrue)
            {//新设计的流程
                this.UniqueID = xElement.Attribute(XName.Get("UniqueID")).Value;
                var Els = from item in xElement.Descendants("SMTElement") select item;
                foreach (XElement SMTElementes in Els)
                {
                    ElementType SMTType = (ElementType)Enum.Parse(typeof(ElementType), SMTElementes.Attribute(XName.Get("ElementType")).Value, true);
                    left = SMTElementes.Attribute(XName.Get("Left")).Value;
                    top = SMTElementes.Attribute(XName.Get("Top")).Value;
                    int.TryParse(SMTElementes.Attribute(XName.Get("ZIndex")).Value, out zIndex);
                    string UniqueID = SMTElementes.Attribute(XName.Get("UniqueID")).Value;
                    string name = SMTElementes.Attribute(XName.Get("Title")).Value;//元素名称               
                    this.CreateElement(UniqueID, name, SMTType, left, top, zIndex);//龙康才增加
                }
            }
            else
            {//老版的流程 
                AddElements(xElement);

            }
            #endregion
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).Type == ElementType.Line)
                {
                    this.SetLineLinkElement(this.Elements, this.Elements[i]);
                }
            }
            this.SelectedElementRemoveAll();
        }

        #endregion
        private void AddElements(XElement xElement)
        {
            int MaxX = 1000;
            this.CreateElement("StartFlow", "开始元素", ElementType.Begin, "5", "250", 1);//开始元素           
            var lines = from item in xElement.Descendants("Rule")
                        where item.Attribute("StrStartActive").Value == "StartFlow"
                        select item;
            if (lines.Count() > 5)
            {
                MaxX = 2000;
            }
            this.CreateElement("EndFlow", "结束元素", ElementType.Finish, "" + MaxX + "", "250", 300);//开始元素        
            int X1 = 5, X2 = 100, Y1 = 250, Y2 = 20;
            int index = 1;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Dictionary<string, string> Countdic = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var Element = (from item in xElement.Descendants("Activity")
                               where item.Attribute("Name").Value == line.Attribute("StrEndActive").Value
                               select item).FirstOrDefault();
                string UniqueID = Element.Attribute(XName.Get("Name")).Value;
                string name = Element.Attribute(XName.Get("Remark")).Value;
                this.CreateElement(UniqueID.Substring(5), name, ElementType.Activity, (X2).ToString(), (Y2).ToString(), 80);
                this.CreateElement(line.Attribute("Name").Value, "新建连线" + index + "", ElementType.Line, "" + (X1 + 55) + "," + (X2 + 3), "" + (Y1 + 30) + "," + (Y2 + 32), 80);
                dic.Add(UniqueID, "" + (X2) + "," + (Y2));
                Countdic.Add(UniqueID, "" + (X2) + "," + (Y2));
                Y2 += 70;
                index += 1;
            }
            AddActivity(xElement, dic, Countdic, index, X2, MaxX, false);
        }
        private void AddActivity(XElement xElement, Dictionary<string, string> dic, Dictionary<string, string> Countdic, int index, int X2, int MaxX, bool strart)
        {

            Dictionary<string, string> dicitem = new Dictionary<string, string>();
            int  Y2 = 20;
            X2 = X2 + 150;
            for (int i = 0; i < dic.Count; i++)
            {             
                double zx = double.Parse(dic.Values.ElementAt(i).Split(',')[0]);
                double zy = double.Parse(dic.Values.ElementAt(i).Split(',')[1]);
                var lines = from item in xElement.Descendants("Rule")
                            where item.Attribute("StrStartActive").Value == dic.Keys.ElementAt(i)
                            select item;
                for (int j = 0; j < lines.Count(); j++)
                {
                    var Startvalue = (from p in Countdic where p.Key == lines.ToList()[j].Attribute("StrEndActive").Value select p);
                    var value = (from p in dic where p.Key == lines.ToList()[j].Attribute("StrEndActive").Value select p);
                    if (Startvalue.Count() > 0 && value.Count() < 1 && strart)
                    {
                        for (int n = 0; n < Startvalue.Count(); n++)
                        {
                            this.CreateElement(lines.ToList()[j].Attribute("Name").Value, "新建连线" + index + "", ElementType.Line, "" + (zx + 49) + "," + (double.Parse(Startvalue.ElementAt(n).Value.Split(',')[0]) + 49), "" + (zy + 62) + "," + (double.Parse(Startvalue.ElementAt(n).Value.Split(',')[1]) + 2), 80);
                            index += 1;
                        }
                        continue;
                    }

                    if (value.Count() > 0)
                    {
                        for (int n = 0; n < value.Count(); n++)
                        {
                            this.CreateElement(lines.ToList()[j].Attribute("Name").Value, "新建连线" + index + "", ElementType.Line, "" + (zx + 49) + "," + (double.Parse(value.ElementAt(n).Value.Split(',')[0]) + 49), "" + (zy + 62) + "," + (double.Parse(value.ElementAt(n).Value.Split(',')[1]) + 2), 80);
                            index += 1;
                        }
                        continue;
                    }
                    else
                    {
                        if (lines.ToList()[j].Attribute("StrEndActive").Value == "EndFlow")
                        {
                            this.CreateElement(lines.ToList()[j].Attribute("Name").Value, "新建连线" + index + "", ElementType.Line, "" + (zx + 98) + "," + (MaxX + 2), "" + (zy + 32) + "," + (250 + 26), 80);

                        }
                        else
                        {
                            var Element = (from item in xElement.Descendants("Activity")
                                           where item.Attribute("Name").Value == lines.ToList()[j].Attribute("StrEndActive").Value
                                           select item).FirstOrDefault();
                            string UniqueID = Element.Attribute(XName.Get("Name")).Value;
                            string name = Element.Attribute(XName.Get("Remark")).Value;
                            var values = (from pv in dicitem where pv.Key == UniqueID select pv);
                            if (values.Count() < 1)
                            {
                                dicitem.Add(UniqueID, "" + (X2) + "," + (Y2));
                                Countdic.Add(UniqueID, "" + (X2) + "," + (Y2));
                                this.CreateElement(UniqueID.Substring(5), name, ElementType.Activity, (X2).ToString(), (Y2).ToString(), 80);
                                this.CreateElement(lines.ToList()[j].Attribute("Name").Value, "新建连线" + index + "", ElementType.Line, "" + (zx + 98) + "," + (X2 + 3), "" + (zy + 32) + "," + (Y2 + 32), 80);
                                Y2 += 70;
                            }
                            else
                            {
                                this.CreateElement(lines.ToList()[j].Attribute("Name").Value, "新建连线" + index + "", ElementType.Line, "" + (zx + 98) + "," + (double.Parse(values.ElementAt(0).Value.Split(',')[0]) + 2), "" + (zy + 32) + "," + (double.Parse(values.ElementAt(0).Value.Split(',')[1]) + 32), 80);
                            }
                        }
                        index += 1;
                    }
                }
            }
            if (dicitem.Count > 0)
            {
                AddActivity(xElement, dicitem, Countdic, index, X2, MaxX, true);
            }
        }
        private string ToLayout()
        {
            StringBuilder xml = new System.Text.StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""  ?>");
            xml.Append(Environment.NewLine);
            xml.Append(@"<WorkFlow>");

            AppSystem system = ucFlowSetting.flowProperty.cbSystemCode.SelectedItem as AppSystem;
            AppModel BOObject = ucFlowSetting.flowProperty.cbSystemCode.SelectedItem as AppModel;

            if (system != null) xml.Append("<System>" + system.Name + "</System> ");

            #region Activitys
            StringBuilder activityXml = new StringBuilder("<Activitys>");
            #region Activity

            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Activity)
                {
                    ActivityControl activity = element as ActivityControl;
                    ActivityObject activityObject = _activityObjects.Where(p => p.ActivityId.Equals(activity.UniqueID)).SingleOrDefault();

                    activityXml.Append(Environment.NewLine);
                    activityXml.Append("<Activity ");

                    activityXml.Append(@" Name=""State" + activity.UniqueID + @"""");
                    activityXml.Append(@" X=""" + activity.Location.X + @"""");
                    activityXml.Append(@" Y=""" + activity.Location.Y + @"""");
                    activityXml.Append(@" RoleName=""" + (activityObject == null ? "" : activityObject.RoleName) + @"""");
                    activityXml.Append(@" UserType=""" + (activityObject == null ? "" : activityObject.UserTypeName) + @"""");
                    activityXml.Append(@" IsOtherCompany=""" + (activityObject == null ? "False" : activityObject.IsSpecifyCompany.ToString()) + @"""");
                    activityXml.Append(@" OtherCompanyID=""" + (activityObject == null ? "" : activityObject.OtherCompanyId) + @"""");
                    activityXml.Append(@" Remark=""" + activity.Title + @""">");
                    #region Countersigns

                    if (activityObject != null && activityObject.IsCounterSign)
                    {
                        activityXml.Append("<Countersigns CountersignType=\"" + activityObject.CounterType + "\">");
                        foreach (CounterSignRole role in activityObject.CounterSignRoleList)
                        {
                            activityXml.Append("<Countersign ");
                            activityXml.Append(@" StateType=""" + role.StateCode + @"""");
                            activityXml.Append(@" RoleName=""" + role.StateName + @"""");
                            activityXml.Append(@" IsOtherCompany=""" + role.IsOtherCompany.ToString() + @"""");
                            activityXml.Append(@" OtherCompanyID=""" + role.OtherCompanyId + @"""");
                            activityXml.Append(@" UserType=""" + role.TypeCode + @""">");
                            activityXml.Append("</Countersign>");
                        }

                        activityXml.Append("</Countersigns>");
                    }
                    #endregion
                    activityXml.Append(Environment.NewLine);
                    activityXml.Append("</Activity>");
                }

            }
            #endregion
            activityXml.Append(Environment.NewLine);
            activityXml.Append("</Activitys>");
            #endregion

            #region rule
            StringBuilder ruleXml = new System.Text.StringBuilder("<Rules>");

            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Line)
                {
                    LineControl line = element as LineControl;
                    LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(line.UniqueID)).SingleOrDefault();

                    ruleXml.Append(Environment.NewLine);
                    ruleXml.Append(@"<Rule ");
                    ruleXml.Append(@" Name=""" + line.UniqueID + @""">");

                    //ruleXml.Append(@" StrStartActive=""" + Rules[i].StrStartActive + @"""");
                    //ruleXml.Append(@" StrEndActive=""" + Rules[i].StrEndActive + @""">");

                    ruleXml.Append(Environment.NewLine);

                    if (lineObject != null && lineObject.ConditionList != null)
                    {
                        if (lineObject.ConditionList.Count > 0)
                        {
                            ruleXml.Append(@"<Conditions ");
                            ruleXml.Append(@" Name=""" + lineObject.LineId + @"""");
                            ruleXml.Append(@" Object=""" + (BOObject == null ? "" : BOObject.Name) + @"""");
                            ruleXml.Append(@" CodiCombMode=""AND"">");

                            foreach (CompareCondition cond in lineObject.ConditionList)
                            {
                                ruleXml.Append(@"<Condition ");
                                ruleXml.Append(@" Name=""" + cond.Name + @"""");
                                ruleXml.Append(@" Description=""" + cond.Description + @"""");
                                ruleXml.Append(@" CompAttr=""" + cond.CompAttr + @"""");
                                ruleXml.Append(@" DataType=""" + cond.DataType + @"""");
                                ruleXml.Append(@" Operate=""" + EscapeXMLChar(cond.Operate) + @"""");
                                ruleXml.Append(@" CompareValueMark=""" + cond.CompareValue + @"""");
                                ruleXml.Append(@" CompareValue=""" + cond.CompareValue + @""">");
                                ruleXml.Append("</Condition>");
                            }

                            ruleXml.Append("</Conditions>");

                        }
                    }

                    ruleXml.Append("</Rule>");
                }
            }

            ruleXml.Append(Environment.NewLine);
            ruleXml.Append("</Rules>");

            #endregion

            xml.Append(Environment.NewLine);
            xml.Append(activityXml.ToString());
            xml.Append(Environment.NewLine);
            xml.Append(ruleXml.ToString());
            xml.Append(Environment.NewLine);
            xml.Append(@"</WorkFlow>");

            return xml.ToString();
        }
        private string GetLayout()
        {
            StringBuilder xml = new System.Text.StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""  ?>");
            xml.Append(Environment.NewLine);
            xml.Append(@"<WorkFlow>");

            FLOW_MODELDEFINE_T system = ucFlowSetting.flowProperty.cbSystemCode.SelectedItem as FLOW_MODELDEFINE_T;
            FLOW_MODELDEFINE_T BOObject = ucFlowSetting.flowProperty.cbModelCode.SelectedItem as FLOW_MODELDEFINE_T;

            if (system != null) xml.Append("<System>" + system.SYSTEMCODE + "</System> ");

            #region Activitys
            StringBuilder activityXml = new StringBuilder("<Activitys>");
            #region 开始活动
            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Begin)
                {
                    SMT.Workflow.Platform.Designer.DesignerControl.BeginControl activity = element as SMT.Workflow.Platform.Designer.DesignerControl.BeginControl;

                    activityXml.Append(Environment.NewLine);
                    activityXml.Append("<Activity ");
                    activityXml.Append(" Name=\"" + GetStateActivityName(element) + "\"");
                    // activityXml.Append(@" Name=" + GetStateActivityName(element) + @"""");
                    activityXml.Append(@" X=""" + activity.Location.X + @"""");
                    activityXml.Append(@" Y=""" + activity.Location.Y + @"""");
                    activityXml.Append(" RoleName=\"\"");
                    activityXml.Append(" UserType=\"\"");
                    activityXml.Append(" IsOtherCompany=\"False\"");
                    activityXml.Append(" OtherCompanyID=\"\"");
                    activityXml.Append(" OtherCompanyName=\"\"");
                    activityXml.Append(" Remark=\"" + activity.Title + "\"/>");
                }
            }
            #endregion
            #region 结束活动
            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Finish)
                {
                    SMT.Workflow.Platform.Designer.DesignerControl.FinishControl activity = element as SMT.Workflow.Platform.Designer.DesignerControl.FinishControl;

                    activityXml.Append(Environment.NewLine);
                    activityXml.Append(@"<Activity ");
                    activityXml.Append(" Name=\"" + GetStateActivityName(element) + "\"");
                    // activityXml.Append(@" Name=" + GetStateActivityName(element) + @"""");
                    activityXml.Append(@" X=""" + activity.Location.X + @"""");
                    activityXml.Append(@" Y=""" + activity.Location.Y + @"""");
                    activityXml.Append(" RoleName=\"\"");
                    activityXml.Append(" UserType=\"\"");
                    activityXml.Append(" IsOtherCompany=\"False\"");
                    activityXml.Append(" OtherCompanyID=\"\"");
                    activityXml.Append(" OtherCompanyName=\"\"");
                    activityXml.Append(" Remark=\"" + activity.Title + "\"/>");
                }
            }
            #endregion
            #region Activity

            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Activity)
                {
                    // ActivityControl activity = element as ActivityControl;
                    SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl activity = element as SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl;
                    ActivityObject activityObject = _activityObjects.Where(p => p.ActivityId.Equals("State" + activity.UniqueID)).SingleOrDefault();

                    activityXml.Append(Environment.NewLine);
                    activityXml.Append("<Activity ");
                    activityXml.Append(@" Name=""State" + activity.UniqueID + @"""");
                    activityXml.Append(@" X=""" + activity.Location.X + @"""");
                    activityXml.Append(@" Y=""" + activity.Location.Y + @"""");
                    activityXml.Append(@" RoleName=""" + (activityObject == null ? "" : activityObject.RoleId) + @"""");
                    activityXml.Append(@" UserType=""" + (activityObject == null ? "" : activityObject.UserType) + @"""");
                    activityXml.Append(@" IsOtherCompany=""" + (activityObject == null ? "False" : activityObject.IsSpecifyCompany.ToString()) + @"""");
                    activityXml.Append(@" OtherCompanyID=""" + (activityObject == null ? "" : activityObject.OtherCompanyId) + @"""");
                    activityXml.Append(@" OtherCompanyName=""" + (activityObject == null ? "" : activityObject.OtherCompanyName) + @"""");
                    activityXml.Append(@" Remark=""" + activity.Title + @""">");
                    #region Countersigns

                    if (activityObject != null && activityObject.IsCounterSign)
                    {//会签
                        activityXml.Append("<Countersigns CountersignType=\"" + activityObject.CounterType + "\">");
                        // activityXml.Append("<Countersigns CountersignType=\"" + (activityObject.IsCounterSign == true ? "0" : "") + "\">");
                        if (activityObject.CounterSignRoleList == null || activityObject.CounterSignRoleList.Count == 0)
                        {
                            tipMsg +=activityObject.Remark+ "  : 没有选择会签角色\r\n";
                           //  ComfirmWindow.ConfirmationBox("提示信息", "保存成功!但你没有选择会签角色!", "确定");
                        }
                        else
                        {
                            foreach (CounterSignRole role in activityObject.CounterSignRoleList)
                            {
                                activityXml.Append("<Countersign ");
                                activityXml.Append(@" StateType=""" + role.StateCode + @"""");
                                activityXml.Append(@" RoleName=""" + role.StateName + @"""");
                                activityXml.Append(@" IsOtherCompany=""" + role.IsOtherCompany.ToString() + @"""");
                                activityXml.Append(@" OtherCompanyID=""" + role.OtherCompanyId + @"""");
                                activityXml.Append(@" OtherCompanyName=""" + role.OtherCompanyName + @"""");
                                activityXml.Append(@" UserType=""" + role.TypeCode + @"""");
                                activityXml.Append(@" UserTypeName=""" + role.TypeName + @""">");
                                activityXml.Append("</Countersign>");
                            }
                        }
                        activityXml.Append("</Countersigns>");
                        
                    }
                    #endregion
                    activityXml.Append(Environment.NewLine);
                    activityXml.Append("</Activity>");
                }

            }
            #endregion
            activityXml.Append(Environment.NewLine);
            activityXml.Append("</Activitys>");
            #endregion

            #region rule
            StringBuilder ruleXml = new System.Text.StringBuilder("<Rules>");

            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Line)
                {
                    LineControl line = element as LineControl;
                    LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(line.UniqueID)).SingleOrDefault();

                    ruleXml.Append(Environment.NewLine);
                    ruleXml.Append(@"<Rule ");
                    ruleXml.Append(@" Name=""" + line.UniqueID + @"""");

                    //added by jason, 02/16/2012
                    ruleXml.Append(@" Remark=""" + line.Title + @"""");
                    //end added by jason

                    //ruleXml.Append(@" StrStartActive=""" + Rules[i].StrStartActive + @"""");
                    //ruleXml.Append(@" StrEndActive=""" + Rules[i].StrEndActive + @""">");
                    if (line.BeginElement != null)
                    {
                        #region 开始活动
                        if (((IControlBase)line.BeginElement).Type == ElementType.Begin)
                        {//开始活动节点
                            ruleXml.Append(@" StrStartActive=""" + GetStateActivityName(line.BeginElement) + @"""");
                        }
                        if (((IControlBase)line.BeginElement).Type == ElementType.Activity)
                        {//条件活动节点
                            SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl activity = line.BeginElement as SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl;
                            if (activity != null)
                            {
                                ruleXml.Append(@" StrStartActive=""" + GetStateActivityName(line.BeginElement) + activity.UniqueID + @"""");
                            }
                        }
                        #endregion
                    }
                    if (line.EndElement != null)
                    {
                        #region 结束活动
                        if (((IControlBase)line.EndElement).Type == ElementType.Finish)
                        {//开始活动节点
                            ruleXml.Append(@" StrEndActive=""" + GetStateActivityName(line.EndElement) + @"""");
                        }
                        if (((IControlBase)line.EndElement).Type == ElementType.Activity)
                        {//条件活动节点
                            SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl activity = line.EndElement as SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl;
                            if (activity != null)
                            {
                                ruleXml.Append(@" StrEndActive=""" + GetStateActivityName(line.EndElement) + activity.UniqueID + @"""");
                            }
                        }
                        #endregion
                    }
                    ruleXml.Append(@">");

                    ruleXml.Append(Environment.NewLine);

                    if (lineObject != null && lineObject.ConditionList != null)
                    {
                        if (lineObject.ConditionList.Count > 0)
                        {
                            ruleXml.Append(@"<Conditions ");
                            ruleXml.Append(@" Name=""" + lineObject.LineId + @"""");
                            ruleXml.Append(@" Object=""" + (BOObject == null ? "" : BOObject.MODELCODE) + @"""");
                            ruleXml.Append(@" CodiCombMode=""AND"">");

                            foreach (CompareCondition cond in lineObject.ConditionList)
                            {
                                ruleXml.Append(@"<Condition ");
                                ruleXml.Append(@" Name=""" + cond.Name + @"""");
                                ruleXml.Append(@" Description=""" + cond.Description + @"""");
                                ruleXml.Append(@" CompAttr=""" + cond.CompAttr + @"""");
                                ruleXml.Append(@" DataType=""" + cond.DataType + @"""");
                                ruleXml.Append(@" Operate=""" + EscapeXMLChar(cond.Operate) + @"""");
                                ruleXml.Append(@" CompareValueMark=""" + cond.CompareValue + @"""");
                                ruleXml.Append(@" CompareValue=""" + cond.CompareValue + @""">");
                                ruleXml.Append("</Condition>");
                            }

                            ruleXml.Append("</Conditions>");

                        }
                    }

                    ruleXml.Append("</Rule>");
                }
            }

            ruleXml.Append(Environment.NewLine);
            ruleXml.Append("</Rules>");

            #endregion

            xml.Append(Environment.NewLine);
            xml.Append(activityXml.ToString());
            xml.Append(Environment.NewLine);
            xml.Append(ruleXml.ToString());
            xml.Append(Environment.NewLine);
            xml.Append(@"</WorkFlow>");

            return xml.ToString();
        }
        private string GetStateActivityName(UIElement element)
        {
            string name = "";

            if (element != null)
            {
                if (((IControlBase)element).Type == ElementType.Begin)
                {
                    name = "StartFlow";
                }
                else if (((IControlBase)element).Type == ElementType.Finish)
                {
                    name = "EndFlow";
                }
                else
                {
                    name = "State";
                }
            }

            return name;
        }

        private List<LineControl> GetOtherLines(LineControl line)
        {
            var lines = from item in Elements
                        where ((IControlBase)item).Type == ElementType.Line //&& ((IControlBase)item) != line
                        select item;

            List<LineControl> otherLines = new List<LineControl>();

            foreach (LineControl item in lines)
            {
                if (line.BeginElement == item.BeginElement)
                {
                    otherLines.Add(item);
                }
            }

            return otherLines;
        }

        private string ToXoml(string workFlowName)
        {
            StringBuilder xml = new StringBuilder(@"<ns0:SMTStateMachineWorkflowActivity x:Name=""" + workFlowName + @"""  InitialStateName=""StartFlow""
              CompletedStateName=""EndFlow"" DynamicUpdateCondition=""{x:Null}"" xmlns:ns0=""clr-namespace:SMT.WFLib;Assembly=SMT.WFLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null""
              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/workflow"">");

            List<string> usedLine = new List<string>();
            int i = 0;

            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Line)
                {
                    LineControl line = element as LineControl;

                    if (!usedLine.Contains(line.UniqueID))
                    {
                        string name = GetStateActivityName(line.BeginElement);

                        xml.Append(Environment.NewLine);
                        xml.Append(@"<StateActivity x:Name=""" + name + @""">");
                        xml.Append(Environment.NewLine);
                        xml.Append(@"<EventDrivenActivity x:Name=""Eda" + name + i.ToString() + @""">");
                        xml.Append(Environment.NewLine);
                        xml.Append(@"<ns0:SMTSubmitEvent ApproveInfo=""{x:Null}"" x:Name=""Event" + name + i.ToString() + @""" EventName=""DoFlow"" InterfaceType=""{x:Type ns0:IFlowEvent}"" />");

                        List<LineControl> otherLines = GetOtherLines(line);

                        #region 有跳转条件时
                        if (otherLines.Count > 1)
                        {
                            xml.Append(Environment.NewLine);
                            xml.Append(@"<IfElseActivity x:Name=""Conditions" + name + i.ToString() + @""">");

                            string tmpString = "";

                            for (int j = 0; j < otherLines.Count; j++)
                            {
                                string strEndActiveName = GetStateActivityName(otherLines[j].EndElement);

                                //默认跳转,默认跳转放在跳转条件节点最后
                                LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(otherLines[j].UniqueID)).SingleOrDefault();

                                if (lineObject == null || lineObject.ConditionList == null || lineObject.ConditionList.Count == 0)
                                {
                                    tmpString = Environment.NewLine;
                                    tmpString += @"<IfElseBranchActivity x:Name=""CompareCondition" + name + i.ToString() + j.ToString() + @""">";
                                    tmpString += @"<SetStateActivity x:Name=""Ts" + name + i.ToString() + j.ToString() + @""" TargetStateName=""" + strEndActiveName + @""" />";
                                    tmpString += Environment.NewLine;
                                    tmpString += @"</IfElseBranchActivity>";
                                }
                                else //有条件
                                {
                                    xml.Append(Environment.NewLine);
                                    xml.Append(@"<IfElseBranchActivity x:Name=""CompareCondition" + name + i.ToString() + j.ToString() + @""">");
                                    xml.Append(Environment.NewLine);
                                    xml.Append(@"<IfElseBranchActivity.Condition>");
                                    xml.Append(Environment.NewLine);
                                    xml.Append(@"<RuleConditionReference ConditionName=""" + lineObject.LineId + @"""/>");
                                    xml.Append(Environment.NewLine);
                                    xml.Append(@"</IfElseBranchActivity.Condition>");
                                    xml.Append(Environment.NewLine);
                                    xml.Append(@"<SetStateActivity x:Name=""Ts" + name + i.ToString() + j.ToString() + @""" TargetStateName=""" + strEndActiveName + @""" />");
                                    xml.Append(Environment.NewLine);
                                    xml.Append(@"</IfElseBranchActivity>");
                                }

                                usedLine.Add(otherLines[j].UniqueID);
                            }

                            if (tmpString != "") xml.Append(tmpString);

                            xml.Append(Environment.NewLine);
                            xml.Append(@"</IfElseActivity>");
                        }
                        #endregion

                        #region 没有跳转条件时
                        else
                        {
                            string strEndActiveName = GetStateActivityName(line.EndElement);

                            xml.Append(Environment.NewLine);
                            xml.Append(@"<SetStateActivity x:Name=""Ts" + name + i.ToString() + @""" TargetStateName=""" + strEndActiveName + @""" />");
                            usedLine.Add(line.UniqueID);
                        }
                        #endregion

                        xml.Append(Environment.NewLine);
                        xml.Append(@"</EventDrivenActivity>");
                        xml.Append(Environment.NewLine);
                        xml.Append(@"</StateActivity>");
                    }

                    i++;
                }
            }

            xml.Append(Environment.NewLine);
            xml.Append(@"<StateActivity x:Name=""EndFlow"" />");
            xml.Append(Environment.NewLine);
            xml.Append(@"</ns0:SMTStateMachineWorkflowActivity>");

            return xml.ToString();
        }
        private string GetXoml(string workFlowName)
        {
            StringBuilder xml = new StringBuilder(@"<ns0:SMTStateMachineWorkflowActivity x:Name=""" + workFlowName + @"""  InitialStateName=""StartFlow""
              CompletedStateName=""EndFlow"" DynamicUpdateCondition=""{x:Null}"" xmlns:ns0=""clr-namespace:SMT.WFLib;Assembly=SMT.WFLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null""
              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/workflow"">");

            List<string> usedLine = new List<string>();
            int i = 0;

            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Line)
                {
                    LineControl line = element as LineControl;

                    if (!usedLine.Contains(line.UniqueID))
                    {
                        if (((IControlBase)line.BeginElement) != null)
                        {
                            string name = "";
                            #region name
                            if (((IControlBase)line.BeginElement).Type == ElementType.Begin || ((IControlBase)line.BeginElement).Type == ElementType.Finish)
                            {
                                name = GetStateActivityName(line.BeginElement);
                            }
                            else
                            {
                                name = GetStateActivityName(line.BeginElement) + ((IControlBase)line.BeginElement).UniqueID;
                            }
                            //    <StateActivity x:Name="StartFlow">
                            //    <EventDrivenActivity x:Name="EdaStartFlow0">
                            //        <ns0:SMTSubmitEvent ApproveInfo="{x:Null}" x:Name="EventStartFlow0" EventName="DoFlow" InterfaceType="{x:Type ns0:IFlowEvent}" />
                            //        <SetStateActivity x:Name="TsStartFlow0" TargetStateName="State19eaa8145f6846dab60273efa91e4b9a" />
                            //    </EventDrivenActivity>
                            //</StateActivity>
                            #endregion
                            xml.Append(Environment.NewLine);
                            xml.Append(@"<StateActivity x:Name=""" + name + @""">");
                            xml.Append(Environment.NewLine);
                            xml.Append(@"<EventDrivenActivity x:Name=""Eda" + name + i.ToString() + @""">");
                            xml.Append(Environment.NewLine);
                            xml.Append(@"<ns0:SMTSubmitEvent ApproveInfo=""{x:Null}"" x:Name=""Event" + name + i.ToString() + @""" EventName=""DoFlow"" InterfaceType=""{x:Type ns0:IFlowEvent}"" />");

                            List<LineControl> otherLines = GetOtherLines(line);

                            #region 有跳转条件时
                            if (otherLines.Count > 1)
                            {
                                xml.Append(Environment.NewLine);
                                xml.Append(@"<IfElseActivity x:Name=""Conditions" + name + i.ToString() + @""">");

                                string tmpString = "";

                                for (int j = 0; j < otherLines.Count; j++)
                                {
                                    if (((IControlBase)otherLines[j].EndElement != null))
                                    {
                                        string strEndActiveName = "";// GetStateActivityName(otherLines[j].EndElement) ;
                                        #region 下一个活动名称 strEndActiveName
                                        if (((IControlBase)otherLines[j].EndElement).Type == ElementType.Begin || ((IControlBase)otherLines[j].EndElement).Type == ElementType.Finish)
                                        {
                                            strEndActiveName = GetStateActivityName(otherLines[j].EndElement);
                                        }
                                        else
                                        {
                                            strEndActiveName = GetStateActivityName(otherLines[j].EndElement) + ((IControlBase)otherLines[j].EndElement).UniqueID;
                                        }
                                        #endregion
                                        //默认跳转,默认跳转放在跳转条件节点最后
                                        LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(otherLines[j].UniqueID)).SingleOrDefault();

                                        if (lineObject == null || lineObject.ConditionList == null || lineObject.ConditionList.Count == 0)
                                        {
                                            tmpString = Environment.NewLine;
                                            tmpString += @"<IfElseBranchActivity x:Name=""CompareCondition" + name + i.ToString() + j.ToString() + @""">";
                                            tmpString += @"<SetStateActivity x:Name=""Ts" + name + i.ToString() + j.ToString() + @""" TargetStateName=""" + strEndActiveName + @""" />";
                                            tmpString += Environment.NewLine;
                                            tmpString += @"</IfElseBranchActivity>";
                                        }
                                        else //有条件
                                        {
                                            xml.Append(Environment.NewLine);
                                            xml.Append(@"<IfElseBranchActivity x:Name=""CompareCondition" + name + i.ToString() + j.ToString() + @""">");
                                            xml.Append(Environment.NewLine);
                                            xml.Append(@"<IfElseBranchActivity.Condition>");
                                            xml.Append(Environment.NewLine);
                                            xml.Append(@"<RuleConditionReference ConditionName=""" + lineObject.LineId + @"""/>");
                                            xml.Append(Environment.NewLine);
                                            xml.Append(@"</IfElseBranchActivity.Condition>");
                                            xml.Append(Environment.NewLine);
                                            xml.Append(@"<SetStateActivity x:Name=""Ts" + name + i.ToString() + j.ToString() + @""" TargetStateName=""" + strEndActiveName + @""" />");
                                            xml.Append(Environment.NewLine);
                                            xml.Append(@"</IfElseBranchActivity>");
                                        }

                                        usedLine.Add(otherLines[j].UniqueID);
                                    }
                                }

                                if (tmpString != "") xml.Append(tmpString);

                                xml.Append(Environment.NewLine);
                                xml.Append(@"</IfElseActivity>");
                            }
                            #endregion

                            #region 没有跳转条件时
                            else
                            {
                                string strEndActiveName = "";// GetStateActivityName(line.EndElement);
                                #region strEndActiveName
                                if (line.EndElement != null)
                                {
                                    if (((IControlBase)line.EndElement).Type == ElementType.Begin || ((IControlBase)line.EndElement).Type == ElementType.Finish)
                                    {
                                        strEndActiveName = GetStateActivityName(line.EndElement);
                                    }
                                    else
                                    {
                                        strEndActiveName = GetStateActivityName(line.EndElement) + ((IControlBase)line.EndElement).UniqueID;
                                    }
                                }
                                #endregion
                                xml.Append(Environment.NewLine);
                                xml.Append(@"<SetStateActivity x:Name=""Ts" + name + i.ToString() + @""" TargetStateName=""" + strEndActiveName + @""" />");
                                usedLine.Add(line.UniqueID);
                            }
                            #endregion

                            xml.Append(Environment.NewLine);
                            xml.Append(@"</EventDrivenActivity>");
                            xml.Append(Environment.NewLine);
                            xml.Append(@"</StateActivity>");
                        }
                    }

                    i++;
                }
            }

            xml.Append(Environment.NewLine);
            xml.Append(@"<StateActivity x:Name=""EndFlow"" />");
            xml.Append(Environment.NewLine);
            xml.Append(@"</ns0:SMTStateMachineWorkflowActivity>");

            return xml.ToString();
        }

        private string ToRule(string systemName, string strObject)
        {
            List<LineObject> lineList = _lineObjects.Where(p => p.ConditionList != null && p.ConditionList.Count > 0).ToList<LineObject>();

            if (lineList.Count > 0)
            {
                StringBuilder xml = new StringBuilder(@"<RuleDefinitions xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/workflow"">");
                xml.Append(Environment.NewLine);
                xml.Append(@"<RuleDefinitions.Conditions>");

                foreach (LineObject lineObject in lineList)
                {
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<RuleExpressionCondition Name=""" + lineObject.LineId + @""">");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<RuleExpressionCondition.Expression>");

                    string tmp = ProcessRule(lineObject.ConditionList, systemName, strObject, lineObject.ConditionList.Count - 1);

                    xml.Append(Environment.NewLine);
                    xml.Append(tmp);
                    xml.Append(Environment.NewLine);
                    xml.Append(@"</RuleExpressionCondition.Expression>");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"</RuleExpressionCondition>");
                }

                xml.Append(Environment.NewLine);
                xml.Append(@"</RuleDefinitions.Conditions>");
                xml.Append(Environment.NewLine);
                xml.Append(@"</RuleDefinitions>");
                return xml.ToString();
            }

            return null;
        }

        private string ProcessRule(List<CompareCondition> ComCondition, string SystemName, string strObject, int i)
        {
            string RuleXml = "";


            if (ComCondition.Count == 1)
            {
                RuleXml = RuleConst(ComCondition[i].Operate, SystemName, strObject, ComCondition[i].CompAttr, ComCondition[i].DataType, ComCondition[i].CompareValue, true);
            }
            else if (i == 0)
            {
                RuleXml = RuleConst(ComCondition[i].Operate, SystemName, strObject, ComCondition[i].CompAttr, ComCondition[i].DataType, ComCondition[i].CompareValue, false);
            }
            else
            {
                if (ComCondition.Count - 1 == i)
                    RuleXml = "<ns0:CodeBinaryOperatorExpression Operator=\"BooleanAnd\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
                else
                    RuleXml = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"BooleanAnd\">";

                RuleXml += "<ns0:CodeBinaryOperatorExpression.Left>";
                RuleXml += ProcessRule(ComCondition, SystemName, strObject, i - 1);
                RuleXml += "</ns0:CodeBinaryOperatorExpression.Left>";

                RuleXml += "<ns0:CodeBinaryOperatorExpression.Right>";
                RuleXml += RuleConst(ComCondition[i].Operate, SystemName, strObject, ComCondition[i].CompAttr, ComCondition[i].DataType, ComCondition[i].CompareValue, false);
                RuleXml += "</ns0:CodeBinaryOperatorExpression.Right>";
                RuleXml += "</ns0:CodeBinaryOperatorExpression>";
            }

            return RuleXml;
        }

        /// <summary>
        /// 生成对比节点
        /// </summary>
        /// <param name="Operate"></param>
        /// <param name="SystemName"></param>
        /// <param name="Object"></param>
        /// <param name="Attribute"></param>
        /// <param name="DataType"></param>
        /// <param name="DataValue"></param>
        /// <param name="IsOne"></param>
        /// <returns></returns>
        private string RuleConst(string Operate, string SystemName, string strObject, string Attribute, string DataType, string DataValue, bool IsOne)
        {
            string GetData = "", strDataType = "", ComFlag = "";

            #region set value
            switch (Operate)
            {
                case "==":
                    ComFlag = "ValueEquality";
                    GetData = "GetString";
                    strDataType = "String";
                    break;
                case ">":
                    ComFlag = "GreaterThan";
                    GetData = "GetDecimal";
                    strDataType = "Int32";
                    break;
                case ">=":
                    ComFlag = "GreaterThanOrEqual";
                    GetData = "GetDecimal";
                    strDataType = "Int32";
                    break;
                case "<":
                    ComFlag = "LessThan";
                    GetData = "GetDecimal";
                    strDataType = "Int32";
                    break;
                case "<=":
                    ComFlag = "LessThanOrEqual";
                    GetData = "GetDecimal";
                    strDataType = "Int32";
                    break;
                case "<>":
                    ComFlag = "ValueEquality";
                    GetData = "GetString";
                    strDataType = "String";
                    break;
                default:
                    ComFlag = null;
                    GetData = null;
                    strDataType = null;
                    break;
            }

            #endregion

            #region set rule const
            string RuleConst = "";

            if (IsOne && Operate != "<>")
            {
                RuleConst = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + ComFlag + "\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
            }
            else
            {
                if (Operate == "<>")
                {
                    RuleConst = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + ComFlag + "\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
                    RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression.Right>";
                    RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
                    RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
                    RuleConst += Environment.NewLine + "<ns1:Boolean xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">false</ns1:Boolean>";
                    RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
                    RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression>";
                    RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression.Right>";
                    RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression.Left>";
                    RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + ComFlag + "\">";
                }
                else
                    RuleConst = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + ComFlag + "\">";
            }

            RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression.Left>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodInvokeExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodInvokeExpression.Parameters>";
            RuleConst += Environment.NewLine + "<ns0:CodeFieldReferenceExpression FieldName=\"xml\">";
            RuleConst += Environment.NewLine + "<ns0:CodeFieldReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression PropertyName=\"FlowData\">";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodeThisReferenceExpression />";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeFieldReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodeFieldReferenceExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + SystemName + "</ns1:String>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + strObject + "</ns1:String>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + Attribute + "</ns1:String>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + " </ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodInvokeExpression.Parameters>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodInvokeExpression.Method>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodReferenceExpression MethodName=\"" + GetData + "\">";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression PropertyName=\"FlowData\">";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodeThisReferenceExpression />";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodReferenceExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodInvokeExpression.Method>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodInvokeExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression.Left>";
            RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression.Right>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:" + strDataType + " xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + DataValue + "</ns1:" + strDataType + ">";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression.Right>";
            RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression>";

            if (Operate == "<>")
            {
                RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression.Left>";
                RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression>";
            }
            #endregion

            return RuleConst;
        }

        private String EscapeXMLChar(string str)
        {
            return str.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }
        #endregion
    
        #region 获取流程的集合
        /// <summary>
        /// 获取流程的集合
        /// </summary>
        private void GetFlowDefine()
        {         
            _activityObjects.Clear();
            _lineObjects.Clear();
            XElement root = XElement.Parse(_currentFlow.FlowDefinition.LAYOUT);

            #region 活动集合
            var el = from e in root.Elements("Activitys")
                     select e;
            foreach (var ent in el.Nodes())
            {
                XElement node = ent as XElement;
                ActivityObject activity = new ActivityObject();
                activity.CounterSignRoleList = new List<CounterSignRole>();
                #region 活动
                activity.ActivityId = node.Attribute("Name").Value;
                activity.UserType = node.Attribute("UserType").Value;
                activity.IsSpecifyCompany = GetAttributeValue(node, "IsOtherCompany").ToLower() == "true" ? true : false; // node.Attribute("IsOtherCompany").Value.ToLower() == "true" ? true : false;
                activity.OtherCompanyId = GetAttributeValue(node, "OtherCompanyID");// node.Attribute("OtherCompanyID").Value;
                activity.RoleId = GetAttributeValue(node, "RoleName");// node.Attribute("RoleName").Value;
                activity.RoleName = GetAttributeValue(node, "RoleName");// node.Attribute("RoleName").Value;
                activity.Remark = node.Attribute("Remark").Value;
                activity.OtherCompanyName = GetAttributeValue(node, "OtherCompanyName");//node.Attribute("OtherCompanyName") != null ? node.Attribute("OtherCompanyName").Value : "";
                #endregion
                #region Countersigns
                XElement xsigns = node.Elements("Countersigns").FirstOrDefault();
                if (xsigns != null)
                {
                    activity.IsCounterSign = xsigns.Nodes().Count() > 0 ? true : false; //xsigns.Attribute("CountersignType").Value == "0" ? true : false;
                    activity.CounterType = xsigns.Attribute("CountersignType").Value;
                    #region Countersign
                    foreach (var sign in xsigns.Nodes())
                    {
                        XElement de = sign as XElement;
                        CounterSignRole role = new CounterSignRole();
                        role.StateCode = de.Attribute("StateType").Value;
                        role.StateName = de.Attribute("RoleName").Value;
                        role.IsOtherCompany = de.Attribute("IsOtherCompany").Value.ToLower() == "true" ? true : false;
                        role.OtherCompanyId = de.Attribute("OtherCompanyID").Value;
                        role.OtherCompanyName =de.Attribute("OtherCompanyName")!=null? de.Attribute("OtherCompanyName").Value:"";
                        role.TypeCode = de.Attribute("UserType").Value;
                        role.TypeName = de.Attribute("UserTypeName") != null ? de.Attribute("UserTypeName").Value : "";
                        activity.CounterSignRoleList.Add(role);
                    }
                }
                    #endregion
                #endregion
                _activityObjects.Add(activity);

            }
            #endregion         
            //==added by jason, 02/24/2012===
            if (root.Elements("System").FirstOrDefault() != null) _systemNameFromLayout = root.Elements("System").FirstOrDefault().Value;

            #region 连线集合
            var ru = from e in root.Elements("Rules")
                     select e;
            foreach (var ent in ru.Nodes())
            {
                XElement node = ent as XElement;
                LineObject line = new LineObject();
                line.ConditionList = new List<CompareCondition>();
                #region Rule
                line.LineId = node.Attribute("Name").Value;
                //added by jason, 02/16/2012
                line.Remark = node.Attribute("Remark") != null ? node.Attribute("Remark").Value : "";
                //end added by jason, 02/16/2012
                #endregion
                #region Conditions
                if (node.Elements("Conditions").FirstOrDefault() != null)
                {
                    //==added by jason, 02/24/2012===
                    _objectNameFromLayout = node.Elements("Conditions").FirstOrDefault().Attribute("Object").Value;

                    line.Object = node.Elements("Conditions").FirstOrDefault().Attribute("Object").Value;
                    line.CodiCombMode = node.Elements("Conditions").FirstOrDefault().Attribute("CodiCombMode").Value;
                #endregion
                    #region Condition
                    foreach (var tion in node.Elements("Conditions").Nodes())
                    {
                        XElement xnode = tion as XElement;
                        CompareCondition cc = new CompareCondition();
                        cc.Name = xnode.Attribute("Name").Value;
                        cc.Description = xnode.Attribute("Description").Value;
                        cc.CompAttr = xnode.Attribute("CompAttr").Value;
                        cc.DataType = xnode.Attribute("DataType").Value;
                        cc.Operate = xnode.Attribute("Operate").Value;
                        cc.CompareValueMark = GetAttributeValue(xnode, "CompareValueMark");
                        cc.CompareValue = xnode.Attribute("CompareValue").Value;
                        line.ConditionList.Add(cc);
                    }
                }
                    #endregion
                _lineObjects.Add(line);

            }
            #endregion

        }
        /// <summary>
        /// 获取活动属性数据
        /// </summary>
        /// <param name="activityID">活动ID</param>
        /// <returns></returns>
        public ActivityObject GetActivityObjectData(string activityID)
        {
            var e = from a in _activityObjects
                    where a.ActivityId == "State" + activityID
                    select a;
            var ent = e.FirstOrDefault();
            return ent;
        }
        /// <summary>
        /// 获取连线属性数据
        /// </summary>
        /// <param name="lineId">连线ID</param>
        /// <returns></returns>
        public LineObject GetLineObjectData(string lineId)
        {
            var e = from a in _lineObjects
                    where a.LineId == lineId
                    select a;
            var ent = e.FirstOrDefault();
            return ent;
        }
        #endregion
        #region 检查工作流程是否完整
 
        /// <summary>
        /// 检查工作流程是否完整 ,完整时返回空""
        /// </summary>
        /// <returns></returns>
        private string CheckFlow()
        {
            string errMsg = "";
            foreach (var ent in _activityObjects)
            {
                if (ent.IsSpecifyCompany==true && ent.OtherCompanyId == "")
                {//选中指定公司,但又没有公司ID
                    errMsg += "【"+ent.Remark+"】角色 指定了公司,但没有选中,请重新选中再保存! \r\n"; 
                }
            }

            _lineObjects = this.ucFlowSetting.lineProperty.LineObjects;//新建时连线数据
          
            foreach (UIElement element in Elements)
            {
                if (((IControlBase)element).Type == ElementType.Begin)
                {
                    BeginControl trol = element as BeginControl;
                    if (trol.BeginLines.Count < 1)
                    {
                        errMsg += "【开始】没有连线；\r\n";
                    }
                    #region 条件判断是否完整
                    string erMeg = "";
                    int n=0;
                    int k = 0;//判断是否所有的线都设置了条件
                    foreach (var line in trol.BeginLines)
                    {
                        #region 判断条件为空
                        LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(((IControlBase)line).UniqueID)).SingleOrDefault();
                        if (lineObject != null)
                        {
                            if (lineObject.ConditionList == null || lineObject.ConditionList.Count == 0)
                            {
                                if (string.IsNullOrEmpty(lineObject.Remark))
                                {
                                    erMeg += "[" + ((IControlBase)line).Title + "]" + " 条件为空!\r\n";
                                    n++;
                                }
                                else
                                {
                                    erMeg += "[" + lineObject.Remark + "]" + " 条件为空!\r\n";
                                    n++;
                                }                             
                            }
                            if (lineObject.ConditionList != null && lineObject.ConditionList.Count > 0)
                            {
                                k++;//计算设置了条件的线条数量
                            }
                        }
                        else
                        {
                            erMeg += "[" + ((IControlBase)line).Title + "]" + " 条件为空!\r\n";
                            n++; 
                        }
                        #endregion
                    }
                    if (n > 1)
                    {
                        errMsg += "【开始】所有分支,只能设置一个条件为空；\r\n" + erMeg;
                    }
                    if (k == trol.BeginLines.Count && trol.BeginLines.Count>1)
                    {
                        errMsg += "【开始】所有分支不能都设置条件,需要保留一条分支的条件设置为空!"; 
                    }
                    #region 如果只有一条线,不需要设置条件
                    if (trol.BeginLines.Count == 1)
                    {
                        LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(((IControlBase)trol.BeginLines[0]).UniqueID)).SingleOrDefault();
                           if (lineObject != null)
                           {
                               if (lineObject.ConditionList != null && lineObject.ConditionList.Count >0)
                               {
                                   errMsg += "【开始】只有一条分支不需要设置条件!"; 
                               }
                           }
                    }
                    #endregion
                    #endregion

                }
                if (((IControlBase)element).Type == ElementType.Finish)
                {
                    FinishControl trol = element as FinishControl;
                    if (trol.EndLines.Count < 1)
                    {
                        errMsg += "【结束】没有连线；\r\n";
                    }

                }
                if (((IControlBase)element).Type == ElementType.Line)
                {
                    LineControl line = element as LineControl;
                    if (line.BeginElement == null)
                    {
                        errMsg += line.Title + ": 没有开始【活动】；\r\n";
                   
                    }
                    if (line.EndElement == null)
                    {
                        errMsg += line.Title + ": 没有结束【活动】；\r\n";
                    }
                }
                if (((IControlBase)element).Type == ElementType.Activity)
                {
                    ActivityControl trol = element as ActivityControl;
                  
                    if (trol.EndLines.Count < 1)
                    {
                        errMsg += trol.Title + ": 没有开始【连线】；\r\n";
                    }
                    if (trol.BeginLines.Count < 1)
                    {
                        errMsg += trol.Title + ": 没有结束【连线】；\r\n";
                    }
                    #region 条件判断是否完整
                    string erMeg = "";
                    int n = 0;
                    int k = 0;
                    foreach (var line in trol.BeginLines)
                    {
                        LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(((IControlBase)line).UniqueID)).SingleOrDefault();
                        if (lineObject != null)
                        {
                            if (lineObject.ConditionList == null || lineObject.ConditionList.Count == 0)
                            {
                                if (string.IsNullOrEmpty(lineObject.Remark))
                                {
                                    erMeg += "[" + ((IControlBase)line).Title + "]" + " 条件为空!\r\n";
                                    n++;
                                }
                                else
                                {
                                    erMeg += "[" + lineObject.Remark + "]" + " 条件为空!\r\n";
                                    n++;
                                }
                            }
                            if (lineObject.ConditionList != null && lineObject.ConditionList.Count > 0)
                            {
                                k++;//计算设置了条件的线条数量
                            }
                        }
                        else
                        {
                            erMeg += "[" + ((IControlBase)line).Title + "]" + " 条件为空!\r\n";
                            n++;
                        }
                    }
                    if (n > 1)
                    {
                        errMsg += "【" + trol.Title + "】所有分支,只能设置一个条件为空；\r\n" + erMeg; 
                    }
                    if (k == trol.BeginLines.Count && trol.BeginLines.Count > 1)
                    {
                        errMsg += "【" + trol.Title + "】所有分支不能都设置条件,需要保留一条分支的条件设置为空!";
                    }
                    #region 如果只有一条线,不需要设置条件
                    if (trol.BeginLines.Count == 1)
                    {
                        LineObject lineObject = _lineObjects.Where(p => p.LineId.Equals(((IControlBase)trol.BeginLines[0]).UniqueID)).SingleOrDefault();
                        if (lineObject != null)
                        {
                            if (lineObject.ConditionList != null && lineObject.ConditionList.Count > 0)
                            {
                                errMsg += "【" + trol.Title + "】只有一条分支不需要设置条件!；\r\n" + erMeg;
                            }
                        }
                    }
                    #endregion
                    #endregion
                }
            }
            return errMsg;
        }
        #endregion
        #region 返回节点属性值
        /// <summary>
        /// 返回节点属性值
        /// </summary>
        /// <param name="xnode">XElement</param>
        /// <param name="name">属性名称</param>
        /// <returns></returns>
        public string GetAttributeValue(XElement xnode, string name)
        {
            if (xnode.Attribute(name) == null)
            {
                return "";
            }
            else
            {
                return xnode.Attribute(name).Value;
            }
        }
        #endregion
    }
}
