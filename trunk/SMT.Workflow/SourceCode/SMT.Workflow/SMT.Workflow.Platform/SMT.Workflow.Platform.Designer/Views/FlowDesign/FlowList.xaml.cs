/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：tianhp   
	 * 创建日期：2011/9/20 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer.Views.FlowDesign 
	 * 描　　述： 流程列表文件
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
using System.Windows.Browser;

using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.Workflow.Platform.Designer.UControls;
using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.Views.FlowDesign
{
    public partial class FlowList : UserControl
    {

        FlowDefineClient client = new FlowDefineClient();
        //行双击事件
        public delegate void EventHandler(string flowCode, bool IsCollapsed);
        public event EventHandler SelectFlow;
        public event RowDoubleClickedHandler RowDoubleClicked;
        public delegate void RowDoubleClickedHandler(object sender, DataGridRowClickedArgs e);
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        
        public FlowList()
        {
            InitializeComponent();
            RegisterEvents();
                
        }

        public bool isPager = false;
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            client.GetFlowDefineListCompleted += new EventHandler<GetFlowDefineListCompletedEventArgs>(client_GetFlowDefineListCompleted);
        }

        public void BindFlowList(string systemCode, string modelCode)
        {
            pBar.Start();
            var systemItem = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            var modelItem = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
         
            if (systemItem != null && modelItem != null)
            {
                if (cbSystemCode.SelectedIndex > 0)
                {
                    if (string.IsNullOrWhiteSpace(systemCode))
                    {
                        systemCode = systemItem.SYSTEMCODE;
                    }
                }
                if (cbModelCode.SelectedIndex > 0)
                {
                    if (string.IsNullOrWhiteSpace(modelCode))
                    {
                        modelCode = modelItem.MODELCODE;
                    }
                }

            }
            string strFliter = "";
            int pageCont = 0;
            if (systemCode != string.Empty)
            {
                strFliter += " AND c.SYSTEMCODE='" + systemCode + "'";
            }
            if (modelCode != string.Empty)
            {
                strFliter += " AND c.MODELCODE='" + modelCode + "'";
            }
            if (txtFlowName.Text.Trim() != string.Empty)
            {
                strFliter += " AND a.description like'%" + txtFlowName.Text.Trim() + "%'";
            }
            strFliter += " AND (b.companyid in (" + Utility.GetAllOwnerCompanyId() + ")";
            strFliter += " OR b.CREATECOMPANYID in (" + Utility.GetAllOwnerCompanyId() + "))";
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID == "7a613fc2-4431-4a46-ae01-232222e9fcb5")
            {//如果是物流公司总部的人
                GetChildCompanyID( systemCode,  modelCode);//物流公司总部的人需要看到所有子公司的流程
            }
            else
            {
                client.GetFlowDefineListAsync(dataPager1.PageSize, dataPager1.PageIndex, strFliter, "", pageCont);
            }
        }
        #region 物流公司总部的人需要看到所有子公司的流程
        public void GetChildCompanyID(string systemCode, string modelCode)
        {
            SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient osc = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
            #region 没有权限
            //osc.GetCompanyAllAsync("");
            ObservableCollection<string> ids = new ObservableCollection<string>();
            ids.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            osc.GetChildCompanyByCompanyIDAsync(ids);
            osc.GetChildCompanyByCompanyIDCompleted += (obj, args) =>
            {
                if (args.Result != null)
                {
                    //strAllOwnerCompanyId += "'" + usr.CompanyID + "',";
                    var childCompanyID = "'" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID + "',";
                    foreach (var companyID in args.Result)
                    {
                        childCompanyID += "'" + companyID + "',";
                    }
                    childCompanyID = childCompanyID.TrimEnd(',');
                    #region 条件
                    string strFliter = "";
                    int pageCont = 0;
                    if (systemCode != string.Empty)
                    {
                        strFliter += " AND c.SYSTEMCODE='" + systemCode + "'";
                    }
                    if (modelCode != string.Empty)
                    {
                        strFliter += " AND c.MODELCODE='" + modelCode + "'";
                    }
                    if (txtFlowName.Text.Trim() != string.Empty)
                    {
                        strFliter += " AND a.description like'%" + txtFlowName.Text.Trim() + "%'";
                    }
                    strFliter += " AND (b.companyid in (" + childCompanyID + ")";
                    strFliter += " OR b.CREATECOMPANYID in (" + childCompanyID + "))";
                    #endregion
                    client.GetFlowDefineListAsync(dataPager1.PageSize, dataPager1.PageIndex, strFliter, "", pageCont);
                }
            };
            #endregion
        }
        #endregion

        void client_GetFlowDefineListCompleted(object sender, GetFlowDefineListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!isPager)
                {
                    if (e.Result.Count == 1)
                    {
                        if (SelectFlow != null) SelectFlow(e.Result[0].FlowCode, true);
                    }
                    if (e.Result.Count > 1)
                    {
                        if (SelectFlow != null) SelectFlow(e.Result[0].FlowCode, false);
                    }
                }
                isPager = false;
                dgrFlows.ItemsSource = e.Result;
                dataPager1.PageCount = e.pageCount;             
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "异常信息：" + e.Error.Message, "确定");
            }
            pBar.Stop();
        }

        /// <summary>
        /// 当前页引索
        /// </summary>
        int curentIndex = 1;
        /// <summary>
        /// 分页事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>   
        private void dataPager1_Click(object sender, RoutedEventArgs e)
        {
            if (dataPager1.PageIndex > 0 && curentIndex != dataPager1.PageIndex)
            {
                isPager = true;
                pBar.Start();
                #region 选择条件过滤
                var systemItem = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
                var modelItem = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
                string systemCode = "", modelCode = "";
                if (systemItem != null && modelItem != null)
                {
                    if (cbSystemCode.SelectedIndex > 0)
                    {
                        systemCode = systemItem.SYSTEMCODE;
                    }
                    if (cbModelCode.SelectedIndex > 0)
                    {
                        modelCode = modelItem.MODELCODE;
                    }

                }
                #endregion
                BindFlowList(systemCode, modelCode);
                curentIndex = dataPager1.PageIndex;
            }
        }
      
   
        /// <summary>
        /// 查询单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
          
            var systemItem = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            var modelItem = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            string systemCode = "", modelCode = "";
            dataPager1.PageIndex = 1;
            #region 选择条件过滤
            if (systemItem != null && modelItem != null)
            {
                if (cbSystemCode.SelectedIndex > 0)
                {
                    systemCode = systemItem.SYSTEMCODE;
                }
                if (cbModelCode.SelectedIndex > 0)
                {
                    modelCode=modelItem.MODELCODE;
                }

            }
            #endregion
            BindFlowList(systemCode, modelCode);
        }     
     
        /// <summary>
        /// 行双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgrFlows_RowDoubleClicked(object sender, DataGridRowClickedArgs e)
        {
            if (RowDoubleClicked != null) RowDoubleClicked(sender, e);
        }

        #region 选择所属系统事件 与 模板联动
        private void cbSystemCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null)
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.SYSTEMCODE == item.SYSTEMCODE || ent.MODELCODE == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cbModelCode.ItemsSource = models;
                        this.cbModelCode.SelectedIndex = 0;
                    }
                }
                else
                {
                    appModel = new ObservableCollection<FLOW_MODELDEFINE_T>();
                    appModel.Insert(0, new FLOW_MODELDEFINE_T() { MODELCODE = "0", DESCRIPTION = "请选择......" });
                    this.cbModelCode.ItemsSource = appModel;
                    this.cbModelCode.SelectedIndex = 0;
                }
            }

        }
        #endregion
       
    }

}
