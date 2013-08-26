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
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class TriggerList : UserControl
    {
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        PlatformService.TimingTriggerClient timingClient = new PlatformService.TimingTriggerClient();
        public TriggerList()
        {
            InitializeComponent();
        }

        public void TriggerInit()
        {
            BindTriggerData();
            timingClient.DeleteTimingActivityCompleted += new EventHandler<DeleteTimingActivityCompletedEventArgs>(timingClient_DeleteTimingActivityCompleted);
            timingClient.GetTimingTriggerListCompleted += new EventHandler<PlatformService.GetTimingTriggerListCompletedEventArgs>(timingClient_GetTimingTriggerListCompleted);
        }
        void timingClient_DeleteTimingActivityCompleted(object sender, DeleteTimingActivityCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "1")
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "删除成功！", "确定");
                    BindTriggerData();
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "删除失败！", "确定");
                }
            }
        }

        void timingClient_GetTimingTriggerListCompleted(object sender, PlatformService.GetTimingTriggerListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                TimingTriggerDataGrid.ItemsSource = e.Result;
                dataPager1.PageCount = e.pageCount;
            }
            else
            {

            }
        }

        public void BindTriggerData()
        {
            int pageCont = 0;           
            string strFilter = "";
            if (Utility.CurrentUser != null)
            {
                strFilter = " AND COMPANYID='" + Utility.CurrentUser.OWNERCOMPANYID + "'";
            }          
            #region 选择条件过滤
            var systemcode = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            var modelCode = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
 
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
            timingClient.GetTimingTriggerListAsync(dataPager1.PageIndex, dataPager1.PageSize, strFilter, "CREATEDATETIME DESC", pageCont);

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindTriggerData();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {           
            TimingTriggerEdit edit = new TimingTriggerEdit("0", null);
             edit.SaveTimingClick += (obj, ev) =>
                {
                    BindTriggerData();
                };          
            edit.appSystem = appSystem;
            edit.appModel = appModel;
            edit.cmbModelCode.ItemsSource = this.cbModelCode.ItemsSource;
            edit.cmbModelCode.SelectedIndex = 0;
            edit.cmbSystemCode.ItemsSource = this.cbSystemCode.ItemsSource;
            edit.cmbSystemCode.SelectedIndex = 0;
            edit.Show();
          
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (TimingTriggerDataGrid.SelectedItems.Count != 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择一条记录进行修改！", "确定");
                return;
            }
            PlatformService.T_WF_TIMINGTRIGGERACTIVITY entity = TimingTriggerDataGrid.SelectedItem as PlatformService.T_WF_TIMINGTRIGGERACTIVITY;
            if (entity != null)
            {
                TimingTriggerEdit edit = new TimingTriggerEdit("1", entity);
                edit.SaveTimingClick += (obj, ev) =>
                {
                    BindTriggerData();
                };          
                edit.appSystem = appSystem;
                edit.appModel = appModel;
                edit.cmbModelCode.ItemsSource = this.cbModelCode.ItemsSource;
                edit.cmbModelCode.SelectedIndex = 0;
                edit.cmbSystemCode.ItemsSource = this.cbSystemCode.ItemsSource;
                if (this.cbSystemCode.Items.Count > 1)
                {
                    for (int i = 0; i < this.cbSystemCode.Items.Count; i++)
                    {
                        if ((this.cbSystemCode.Items[i] as FLOW_MODELDEFINE_T).SYSTEMCODE == entity.SYSTEMCODE)
                        {
                            edit.cmbSystemCode.SelectedIndex = i;
                        }
                    }
                }
                else
                {
                    edit.cmbSystemCode.SelectedIndex = 0;
                }
                edit.cmbModelCode.Selected<FLOW_MODELDEFINE_T>("MODELCODE", entity.MODELCODE);       
                edit.Show();
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择一条记录进行修改！", "确定");
            }
        }

        private void btnDelect_Click(object sender, RoutedEventArgs e)
        {
            if (TimingTriggerDataGrid.SelectedItems.Count == 1)
            {
                pBar.Start();
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    T_WF_TIMINGTRIGGERACTIVITY ent = TimingTriggerDataGrid.SelectedItem as T_WF_TIMINGTRIGGERACTIVITY;
                    timingClient.DeleteTimingActivityAsync(ent.TRIGGERID);
                };
                com.SelectionBox("删除确定", "你确定删除选中信息吗？", ComfirmWindow.titlename, Result);
            }
            else
            {                
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
                BindTriggerData();
                curentIndex = dataPager1.PageIndex;
            }
            
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
