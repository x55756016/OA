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
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.Workflow.Platform.Designer.Utils;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class DefaultMessgeList : UserControl
    {       
        PlatformService.MessageBodyDefineClient messageClient = new MessageBodyDefineClient();
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        public DefaultMessgeList()
        {
            InitializeComponent();
          
           
        }

        public void DefaultMessgeInit()
        {
            pBar.Start();
            messageClient.GetDefaultMessgeListCompleted += new EventHandler<GetDefaultMessgeListCompletedEventArgs>(messageClient_GetDefineMessageListCompleted);
            messageClient.DeleteDefaultMessgeCompleted += new EventHandler<DeleteDefaultMessgeCompletedEventArgs>(messageClient_DeleteDefaultMessgeCompleted);
            BindDefaultGridData();
        }
       
        #region WCF返回结果       

        void messageClient_GetDefineMessageListCompleted(object sender, GetDefaultMessgeListCompletedEventArgs e)
        {
            if (e.Error == null)
            {               
                DataGridDefaultMessage.ItemsSource = e.Result;
                dataPager1.PageCount = e.pageCount;
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "异常信息：" + e.Error.Message, "确定");
            }
            pBar.Stop();
        }
        //删除返回结果    
        void messageClient_DeleteDefaultMessgeCompleted(object sender, DeleteDefaultMessgeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "1")
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "删除成功！", "确定");
                    BindDefaultGridData();
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "删除失败！", "确定"); 
                }
            }
        }
        #endregion    

        public  void BindDefaultGridData()
        {
            pBar.Start();
            int pageCont = 0;
            string strFilter = "";
            var systemcode = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            var modelCode = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (Utility.CurrentUser != null)
            {
                strFilter = " AND COMPANYID='" + Utility.CurrentUser.OWNERCOMPANYID + "'";
            }

            #region 选择条件过滤
            if (systemcode != null && modelCode != null)
            {
                if (systemcode.SYSTEMCODE != "0" && modelCode.MODELCODE != "0")
                {
                    strFilter += " AND SYSTEMCODE='" + systemcode.SYSTEMCODE + "' AND MODELCODE='" + modelCode.MODELCODE + "'";
                }
                if (systemcode.SYSTEMCODE != "0" && modelCode.MODELCODE == "0")
                {
                    strFilter += " AND SYSTEMCODE='" + systemcode.SYSTEMCODE + "'";
                }
                if (systemcode.SYSTEMCODE == "0" && modelCode.MODELCODE != "0")
                {
                    strFilter += " AND MODELCODE='" + modelCode.MODELCODE + "'";
                }
            }
            #endregion
           // ComfirmWindow.ConfirmationBox("提示信息", "开始执行strFilter：" + strFilter, "确定");
            messageClient.GetDefaultMessgeListAsync(dataPager1.PageIndex, dataPager1.PageSize, strFilter, "CREATEDATE DESC", pageCont);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindDefaultGridData();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
           
            DefaultMessgeEdit edit = new DefaultMessgeEdit("0", "");
            edit.SaveCompleted += (obj, ev) =>
            {//保存成功，重新绑定
                BindDefaultGridData();
            };
            edit.appSystem = appSystem;
            edit.appModel = appModel;
            edit.cobMODELCODE.ItemsSource = this.cbModelCode.ItemsSource;
            edit.cobMODELCODE.SelectedIndex = 0;
            edit.cobSYSTEMCODE.ItemsSource = this.cbSystemCode.ItemsSource;
            edit.cobSYSTEMCODE.SelectedIndex = 0;
            edit.Show();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
            PlatformService.T_WF_MESSAGEBODYDEFINE entity = DataGridDefaultMessage.SelectedItem as PlatformService.T_WF_MESSAGEBODYDEFINE;
            if (entity != null)
            {
                DefaultMessgeEdit edit = new DefaultMessgeEdit("1", entity.DEFINEID);
                edit.SaveCompleted += (obj, ev) =>
                {//保存成功，重新绑定
                    BindDefaultGridData();
                };
                edit.appSystem = appSystem;
                edit.appModel = appModel;
                edit.cobMODELCODE.ItemsSource = this.cbModelCode.ItemsSource;
                edit.cobMODELCODE.SelectedIndex = 0;
                edit.cobSYSTEMCODE.ItemsSource = this.cbSystemCode.ItemsSource;
                edit.cobSYSTEMCODE.SelectedIndex = 0;
                edit.Show();
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择一条记录进行修改！", "确定");  
            }
        }
        
        private void btnDelect_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDefaultMessage.SelectedItems.Count == 1)
            {
                pBar.Start();
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {                   
                    T_WF_MESSAGEBODYDEFINE ent = DataGridDefaultMessage.SelectedItem as T_WF_MESSAGEBODYDEFINE;
                    messageClient.DeleteDefaultMessgeAsync(ent.DEFINEID);
                };
                com.SelectionBox("删除确定", "你确定删除选中信息吗？", ComfirmWindow.titlename, Result);
            }
            else
            {
                //MessageBox.Show("请先选择一条需要删除的记录!");
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录", "确定");
            }
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
                BindDefaultGridData();
                curentIndex = dataPager1.PageIndex;
            }           
        }

        #region 选择所属系统事件 与 模板联动
        private void cbSystemCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null )
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
