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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.UControls;
using SMT.Workflow.Platform.Designer.Utils;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class TriggerDefine : UserControl
    {
        public delegate void RowDoubleClicked(object sender, TriggerDefineClickedArgs args);
        public event RowDoubleClicked OnSubmitComplated; //注册事件
        public TriggerDefineClient client = null; //
        public FlowXmlDefineClient clientXml = null; //xmlWCF定义
        public TriggerDefine()
        {
            InitializeComponent();
            client = new TriggerDefineClient();
            clientXml = new FlowXmlDefineClient(); //实例化xml
            RegisterServices();
        }
        #region 绑定数据源
        public void DateGrid()
        {
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();//参数值
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            if (cobSYSTEMCODE.SelectedItem as AppSystem != null && (cobSYSTEMCODE.SelectedItem as AppSystem).Name != "0")
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "SYSTEMCODE = '" + (cobSYSTEMCODE.SelectedItem as AppSystem).Name + "'";
            }
            if (cobModule.SelectedItem as AppModel != null)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "MODELCODE = '" + (cobModule.SelectedItem as AppModel).Name + "'";
            }

            //==added by jason, 02/22/2012
            string strAllOwnerCompanyId = Utility.GetAllOwnerCompanyId();

            if (strAllOwnerCompanyId != "")
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }

                filter += " CompanyId in (" + strAllOwnerCompanyId + ") ";
            }

            //默认消息查询
            client.GetListFlowTriggerDefineAsync(filter, dataPager1.PageIndex, dataPager1.PageSize, pageCount);
        }
        #endregion
        #region 操作完成后事件
        /// <summary>
        /// 注册WCF
        /// </summary>
        private void RegisterServices()
        {
           //查询定时触发
            client.GetListFlowTriggerDefineCompleted += new EventHandler<GetListFlowTriggerDefineCompletedEventArgs>(client_GetListFlowTriggerDefineCompleted);
            //删除定时触发
            client.DeleteTriggerDefineCompleted += new EventHandler<DeleteTriggerDefineCompletedEventArgs>(client_DeleteTriggerDefineCompleted);
        }

        //查询定时触发
        void client_GetListFlowTriggerDefineCompleted(object sender, GetListFlowTriggerDefineCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    Utility.HandleDataPageDisplay(dataPager1, e.pageCount);
                    if (e.Result == null || e.Result.ToList().Count < 1)
                    {
                        dgTriggerDefine.ItemsSource = null;
                    }
                    else
                    {
                        this.dgTriggerDefine.ItemsSource = e.Result;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        //删除定时触发
        void client_DeleteTriggerDefineCompleted(object sender, DeleteTriggerDefineCompletedEventArgs e)
        {
            try
            {
                if (e.Result)
                {
                    //MessageBox.Show("删除成功");
                    ComfirmWindow.ConfirmationBox("提示信息", "删除成功!", "确定");
                    DateGrid();
                }
                else
                {
                    //MessageBox.Show("删除失败");
                    ComfirmWindow.ConfirmationBox("提示信息", "删除失败!", "确定");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("删除失败");
                ComfirmWindow.ConfirmationBox("提示信息", "删除失败!", "确定");
            }
            pBar.Stop();
        }
        #endregion
        #region 事件
        //所属系统选择事件
        private void cobSYSTEMCODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                AppSystem appSystem = cobSYSTEMCODE.SelectedItem as AppSystem;
                if (appSystem.Name != "0")
                {
                    clientXml.AppModelCompleted += (o, v) =>
                    {
                        cobModule.ItemsSource = v.Result;
                    };
                    clientXml.AppModelAsync(appSystem.ObjectFolder);
                }
                else
                {
                    cobModule.ItemsSource = null;
                }
            }
            catch
            {

            }
        }
        //查询
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DateGrid();
        }
        //删除
        private void btnDelect_Click(object sender, RoutedEventArgs e)
        {
          
            if (dgTriggerDefine.SelectedItems.Count > 0)
            {
                pBar.Start();
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<T_WF_TIMINGTRIGGERCONFIG> FlowDefinelList = new ObservableCollection<T_WF_TIMINGTRIGGERCONFIG>();
                    for (int i = 0; i < dgTriggerDefine.SelectedItems.Count; i++)
                    {
                        FlowDefinelList.Add(dgTriggerDefine.SelectedItems[i] as T_WF_TIMINGTRIGGERCONFIG);
                    }
                    client.DeleteTriggerDefineAsync(FlowDefinelList);
                };
                com.SelectionBox("删除确定", "你确定删除选中信息吗？", ComfirmWindow.titlename, Result);
            }
            else 
            {
                //MessageBox.Show("请先选择一条需要删除的记录!");
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录!", "确定");
            }
        }
        /// <summary>
        /// 分页事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataPager1_Click(object sender, RoutedEventArgs e)
        {
            DateGrid();
        }
        /// <summary>
        /// 行双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTriggerDefine_RowDoubleClicked(object sender, DataGridRowClickedArgs e)
        {
            if (OnSubmitComplated != null)
                OnSubmitComplated(sender, new TriggerDefineClickedArgs() { SelectedItem = dgTriggerDefine.SelectedItem as T_WF_TIMINGTRIGGERCONFIG });                
        }
        #endregion
     }
    public class TriggerDefineClickedArgs : EventArgs
    {
        public T_WF_TIMINGTRIGGERCONFIG SelectedItem { get; set; }
    }
}
