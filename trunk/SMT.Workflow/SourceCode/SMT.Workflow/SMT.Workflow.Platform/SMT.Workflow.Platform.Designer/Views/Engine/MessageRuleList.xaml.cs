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
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.PlatformService;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class MessageRuleList : UserControl
    {
        PlatformService.DoTaskRuleClient DoTaskClient = new PlatformService.DoTaskRuleClient();
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        public MessageRuleList()
        {
            InitializeComponent();
        }

        public void MessageRuleInit()
        {
            BindRuleData();
            DoTaskClient.GetGetDoTaskListCompleted += new EventHandler<PlatformService.GetGetDoTaskListCompletedEventArgs>(DoTaskClient_GetGetDoTaskListCompleted);
            DoTaskClient.DeleteDoTaskRuleCompleted += new EventHandler<PlatformService.DeleteDoTaskRuleCompletedEventArgs>(DoTaskClient_DeleteDoTaskRuleCompleted);
        }
        void DoTaskClient_DeleteDoTaskRuleCompleted(object sender, PlatformService.DeleteDoTaskRuleCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                BindRuleData();
                ComfirmWindow.ConfirmationBox("提示信息", "删除成功!", "确定");

            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "删除失败!", "确定");
            }
        }

        void DoTaskClient_GetGetDoTaskListCompleted(object sender, PlatformService.GetGetDoTaskListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DoTaskRuleDataGrid.ItemsSource = e.Result;
                dataPager1.PageCount = e.pageCount;
            }
            else
            {

            }
            pBar.Stop();
        }

        public void BindRuleData()
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
            pBar.Start();
            #endregion
            DoTaskClient.GetGetDoTaskListAsync(dataPager1.PageIndex, dataPager1.PageSize, strFilter, "CREATEDATETIME DESC", pageCont);

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dataPager1.PageIndex = 1;
            BindRuleData();
            pBar.Start();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DoTaskRuleDetailEdit edit = new DoTaskRuleDetailEdit(null);
            edit.SaveDetailClick += (obj, ev) =>
            {
                BindRuleData();
            };
            edit.appSystem = appSystem;
            edit.appModel = appModel;
            edit.cbModelName.ItemsSource = this.cbModelCode.ItemsSource;
            edit.cbModelName.SelectedIndex = 0;
            edit.cbSystemName.ItemsSource = this.cbSystemCode.ItemsSource;
            edit.cbSystemName.SelectedIndex = 0;
            edit.Show();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DoTaskRuleDataGrid.SelectedItems.Count == 1)
            {
                PlatformService.T_WF_DOTASKRULE Rule = DoTaskRuleDataGrid.SelectedItem as PlatformService.T_WF_DOTASKRULE;
                DoTaskRuleDetailEdit edit = new DoTaskRuleDetailEdit(Rule);
                edit.SaveDetailClick += (obj, ev) =>
                {
                    BindRuleData();
                };
                edit.appSystem = appSystem;
                edit.appModel = appModel;
                edit.cbModelName.ItemsSource = this.cbModelCode.ItemsSource;
                edit.cbModelName.SelectedIndex = 0;
                edit.cbSystemName.ItemsSource = this.cbSystemCode.ItemsSource;
                edit.cbSystemName.SelectedIndex = 0;
                for (int i = 0; i < edit.cbSystemName.Items.Count(); i++)
                {
                    if ((edit.cbSystemName.Items[i] as FLOW_MODELDEFINE_T).SYSTEMCODE == Rule.SYSTEMCODE)
                    {
                        edit.cbSystemName.SelectedIndex = i;
                    }
                }
                for (int i = 0; i < edit.cbModelName.Items.Count(); i++)
                {
                    if ((edit.cbModelName.Items[i] as FLOW_MODELDEFINE_T).MODELCODE == Rule.MODELCODE)
                    {
                        edit.cbModelName.SelectedIndex = i;
                    }
                }
                edit.Show();
            }
            else if (DoTaskRuleDataGrid.SelectedItems.Count > 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "只能选择一条需要修改的记录", "确定");
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要修改的记录！", "确定");
            }
        }

        private void btnDelect_Click(object sender, RoutedEventArgs e)
        {
            if (DoTaskRuleDataGrid.SelectedItems.Count > 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "只能选择一条需要删除的记录！", "确定");
                return;
            }
            if (DoTaskRuleDataGrid.SelectedItems.Count == 1)
            {
                pBar.Start();
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    PlatformService.T_WF_DOTASKRULE rule = (DoTaskRuleDataGrid.SelectedItem as PlatformService.T_WF_DOTASKRULE);
                    DoTaskClient.DeleteDoTaskRuleAsync(rule.DOTASKRULEID);
                };
                com.SelectionBox("删除确定", "你确定删除选中的记录吗？", ComfirmWindow.titlename, Result);
            }
            else
            {
                //MessageBox.Show("请先选择一条需要删除的记录!");
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录！", "确定");
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
                pBar.Start();
                BindRuleData();
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
