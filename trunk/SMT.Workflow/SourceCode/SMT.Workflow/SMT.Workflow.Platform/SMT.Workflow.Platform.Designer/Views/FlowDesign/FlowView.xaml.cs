/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：zhangyh   
	 * 创建日期：2011/10/9 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer.Views.FlowDesign 
	 * 描　　述： 流程设计器视图文件
	 * 模块名称：工作流设计器
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
using System.Collections.ObjectModel;

using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.UControls;
using SMT.Workflow.Platform.Designer.PlatformService;
//using SMT.Workflow.Platform.Designer.PermissionService;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.Workflow.Platform.Designer.Dialogs;

using Telerik.Windows.Controls;
using Telerik.Windows;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
using SMT.Workflow.Platform.Designer.Class;

namespace SMT.Workflow.Platform.Designer.Views.FlowDesign
{
    public partial class FlowView : UserControl
    {
        #region 私有变量
        //树集合，用于绑定树
        ObservableCollection<TreeItem> items = new ObservableCollection<TreeItem>();
        //流程服务定义
        FlowDefineClient client = new FlowDefineClient();
        //权限服务
        PermissionServiceClient permissionClient = new PermissionServiceClient();     
      
        TreeItem treeItem = new TreeItem();//用于删除成功去除列表用
        public Flow_ModelDefineClient modelClient = null; //流程模块WCF定义
        #endregion

        public FlowView()
        {
            InitializeComponent();
            modelClient = new Flow_ModelDefineClient(); //实例化流程模块            
            RegisterEvents(); 
            tvFlow.OnSelectionChanged += new TreeControl.SelectionChanged(tvFlow_SelectionChanged);  //树形选择事件
            if (WfUtils.StateList == null)
            {
                permissionClient.GetSysRoleInfosAsync("", "");
            }

            modelClient.GetSystemCodeModelCodeListAsync();//绑定树
            ucFlowlist.BindFlowList("", "");//绑定列表

          
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            ucFlowlist.RowDoubleClicked += new FlowList.RowDoubleClickedHandler(TFlowlist_DataGridRowDoubleClick);
            ucFlowlist.SelectFlow += new FlowList.EventHandler(ucFlowlist_SelectFlow);
            client.DeleteFlowCompleted += new EventHandler<DeleteFlowCompletedEventArgs>(client_DeleteFlowCompleted);          
            permissionClient.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(permissionClient_GetSysRoleInfosCompleted);
            modelClient.GetSystemCodeModelCodeListCompleted += new EventHandler<GetSystemCodeModelCodeListCompletedEventArgs>(modelClient_GetSystemCodeModelCodeListCompleted);
        }

        void ucFlowlist_SelectFlow(string flowCode, bool IsCollapsed)
        {
            if (IsCollapsed)
            {
                LoadFlowDefine(flowCode);
            }
            else
            {
                ucDesigner.Visibility = Visibility.Collapsed;
                ucFlowlist.Visibility = Visibility.Visible;
                HideButtons();
                btnAddFlow.Visibility = Visibility.Visible;
                btnEditFlow.Visibility = Visibility.Visible;
                btnDeleteFlow.Visibility = Visibility.Visible;
            }
        }

    
        #region 龙康才新增
        /// <summary>
        /// 所有的流程节点放在一个列表里
        /// </summary>
        List<TreeItem> treeList = new List<TreeItem>();
        /// <summary>
        /// 当前先中的流程列表
        /// </summary>
        List<TreeItem> SelectedFlowList = new List<TreeItem>();
     

        #region 树形操作
        void modelClient_GetSystemCodeModelCodeListCompleted(object sender, GetSystemCodeModelCodeListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ListBindTree(e.Result);
                }
            }
        }

        public void ListBindTree(ObservableCollection<FLOW_MODELDEFINE_T> list)
        {
            ObservableCollection<FLOW_MODELDEFINE_T> systemCode = new ObservableCollection<FLOW_MODELDEFINE_T>();
            var systemcode = list.GroupBy(s => s.SYSTEMCODE);
            foreach (var item in systemcode)
            {
                systemCode.Add(item.FirstOrDefault() as FLOW_MODELDEFINE_T);
            }
            items.Clear();
            //创建流程树的根       
            foreach (FLOW_MODELDEFINE_T item in systemCode)
            {
                TreeItem item2 = new TreeItem() { ID = item.SYSTEMCODE, Name = item.SYSTEMNAME, LevelID = 2, ParentID = "" };            
                var childrens = from e in list
                                where e.SYSTEMCODE == item.SYSTEMCODE
                                select e;
                foreach (FLOW_MODELDEFINE_T child in childrens)
                {
                    TreeItem subItem = new TreeItem() { ID = child.MODELCODE, Name = child.DESCRIPTION, LevelID = 3, ParentID = item.SYSTEMCODE };
                    if (!string.IsNullOrEmpty(subItem.ID))
                    {
                        item2.Children.Add(subItem);
                        //treeList.Add(subItem);
                    }
                }
                items.Add(item2);
            }
         
            //绑定树控件
            tvFlow.ItemsSource = items;
            systemCode.Insert(0, new FLOW_MODELDEFINE_T() { SYSTEMCODE = "0", SYSTEMNAME = "请选择......" });
            list.Insert(0, new FLOW_MODELDEFINE_T() { SYSTEMCODE = "0", MODELCODE = "0", DESCRIPTION = "请选择......" });
            ucFlowlist.appSystem = systemCode;
            ucFlowlist.appModel = list;
            FlowSystemModel.appModel =  list;
            FlowSystemModel.appSystem = systemCode;
            ucFlowlist.cbModelCode.ItemsSource = list;
            ucFlowlist.cbModelCode.SelectedIndex = 0;
            ucFlowlist.cbSystemCode.ItemsSource = systemCode;
            ucFlowlist.cbSystemCode.SelectedIndex = 0;
        }

        /// <summary>
        /// 树选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void tvFlow_SelectionChanged(object sender, SelectionChangedArgs args)
        {
            ucFlowlist.txtFlowName.Text = "";
            if (args.SelectedItem != null)
            {
                ucDesigner.Visibility = Visibility.Collapsed;
                ucFlowlist.Visibility = Visibility.Visible;
                HideButtons();
                btnAddFlow.Visibility = Visibility.Visible;
                btnEditFlow.Visibility = Visibility.Visible;
                btnDeleteFlow.Visibility = Visibility.Visible;
                ucFlowlist.dataPager1.PageIndex = 1;
                if (args.SelectedItem.LevelID == 2)
                {                 
                    ucFlowlist.cbSystemCode.Selected<FLOW_MODELDEFINE_T>("SYSTEMCODE", args.SelectedItem.ID);
                    ucFlowlist.cbModelCode.SelectedIndex = 0;
                    ucFlowlist.BindFlowList(args.SelectedItem.ID, "");//绑定列表
                }
                else
                {
                    ucFlowlist.cbSystemCode.Selected<FLOW_MODELDEFINE_T>("SYSTEMCODE", args.SelectedItem.ParentID);
                    ucFlowlist.cbModelCode.Selected<FLOW_MODELDEFINE_T>("MODELCODE", args.SelectedItem.ID);           
                    ucFlowlist.BindFlowList(args.SelectedItem.ParentID, args.SelectedItem.ID);//绑定列表
                }
            }
        }

        #endregion

        #region 权限操作

        private List<StateType> InitRoleList(List<T_SYS_ROLE> roleList)
        {
            List<StateType> StateList = new List<StateType>();
            //下面三项是回定出现的,不分公司
            //添加直接上级
            StateList.Add(new StateType() { CompanyID = "11111", StateCode = Higher.Superior.ToString(), StateName = "直接上级" });
            //添加隔级上级
            StateList.Add(new StateType() { CompanyID = "11111", StateCode = Higher.SuperiorSuperior.ToString(), StateName = "隔级上级" });
            //添加部门负责人
            StateList.Add(new StateType() { CompanyID = "11111", StateCode = Higher.DepartHead.ToString(), StateName = "部门负责人" });

            foreach (T_SYS_ROLE role in roleList)
            {
                StateList.Add(new StateType() { StateCode = role.ROLEID.ToString(), StateName = role.ROLENAME, CompanyID = role.OWNERCOMPANYID });
            }

            return StateList;
        }

        void permissionClient_GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                WfUtils.StateList = InitRoleList(e.Result.ToList<T_SYS_ROLE>());
            }
        }
        #endregion


        //void ucDesigner_OnSubmitComplated(object o, TreeItem item, string option)
        //{
           
        //    if (item == null) return;
        //    ucFlowlist.BindFlowList("", "");//绑定列表

        //}
    
        #endregion

        #region WCF删除流程完成

        void client_DeleteFlowCompleted(object sender, DeleteFlowCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "1")
                {

                    ComfirmWindow.ConfirmationBox("提示信息", "删除成功!", "确定");
                    ucFlowlist.BindFlowList("", "");//绑定列表
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "错误信息：" + e.Result, "确定");
                }
            }
            ucFlowlist.pBar.Stop();
        }       
        #endregion


   
        #region 私有方法
        /// <summary>
        /// 隐藏列表及容器面板
        /// </summary>
        private void HidePanel()
        {
            this.ucFlowlist.Visibility = Visibility.Collapsed;
            this.ucDesigner.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 隐藏所有按钮
        /// </summary>
        private void HideButtons()
        {
        
            btnAddFlow.Visibility = Visibility.Collapsed;
            btnEditFlow.Visibility = Visibility.Collapsed;
            btnDeleteFlow.Visibility = Visibility.Collapsed;
            btnClear.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
        }
        #region 加载流程定义
        /// <summary>
        /// 加载流程定义
        /// </summary>
        /// <param name="flowCode">流程编码</param>
        private void LoadFlowDefine(string flowCode)
        {
            HidePanel();
            HideButtons();

            ucDesigner.Visibility = Visibility.Visible;
            btnAddFlow.Visibility = Visibility.Visible;
            btnClear.Visibility = Visibility.Visible;
            btnSave.Visibility = Visibility.Visible;
            ucDesigner.LoadFlow(flowCode);///////////////
        }
        #endregion
     
        #endregion


        #region  事件
        /// <summary>
        /// 流程列表双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TFlowlist_DataGridRowDoubleClick(object sender, DataGridRowClickedArgs e)
        {
            V_FlowDefine item = e.DataGridRowItem as V_FlowDefine;

            if (item != null)
            {
                LoadFlowDefine(item.FlowCode);
            }
        }

     

        /// <summary>
        /// 编辑流程事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEditFlow_Click(object sender, RoutedEventArgs e)
        {
            var obj = ucFlowlist.dgrFlows.SelectedItem as V_FlowDefine;
            if (obj == null)
            {              
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要编辑的流程!", "确定");
                return;
            }
            else
            {
                LoadFlowDefine(obj.FlowCode);
            }

        }

        /// <summary>
        /// 删除流程事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteFlow_Click(object sender, RoutedEventArgs e)
        {
            if (ucFlowlist.dgrFlows.SelectedItems.Count > 0)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ucFlowlist.pBar.Start();
                    ObservableCollection<string> flowList = new ObservableCollection<string>();
                    int count = ucFlowlist.dgrFlows.SelectedItems.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var entity = (ucFlowlist.dgrFlows.SelectedItems[i] as V_FlowDefine);
                        flowList.Add(entity.FlowCode);
                    }
                    client.DeleteFlowAsync(flowList);                  
                };
                com.SelectionBox("删除确定", "你确定删除选中的流程吗？", ComfirmWindow.titlename, Result);
            }
            else
            {               
                ComfirmWindow.ConfirmationBox("提示信息", "没有选中流程,请先选择流程进行删除!", "确定");               
            }
        }

     
        private void btnAddFlow_Click(object sender, RoutedEventArgs e)
        {
            ucDesigner.Visibility = Visibility.Visible;
            ucFlowlist.Visibility = Visibility.Collapsed;
            HideButtons();
            btnAddFlow.Visibility = Visibility.Visible;
            btnClear.Visibility = Visibility.Visible;
            btnSave.Visibility = Visibility.Visible;
            ucDesigner.AddFlow();
        }
        /// <summary>
        /// 清除流程容器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ucDesigner.ClearFlow();
        }

        /// <summary>
        /// 保存流程事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ucDesigner.SaveFlow();          
        }
        #endregion

     

    }

}
