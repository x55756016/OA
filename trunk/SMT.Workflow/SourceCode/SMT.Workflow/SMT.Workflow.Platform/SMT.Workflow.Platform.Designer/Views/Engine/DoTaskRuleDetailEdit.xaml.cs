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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.Workflow.Platform.Designer.Utils;
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.Class;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class DoTaskRuleDetailEdit : ChildWindow
    {
        PlatformService.DoTaskRuleClient client = new PlatformService.DoTaskRuleClient();
        private FlowXmlDefineClient clientXml = new FlowXmlDefineClient(); //实例化xml 
        private PlatformService.T_WF_DOTASKRULE Rule = null;
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        public event EventHandler SaveDetailClick;
        public DoTaskRuleDetailEdit(PlatformService.T_WF_DOTASKRULE rule)
        {
            InitializeComponent();
            client.AddDoTaskRuleCompleted += new EventHandler<AddDoTaskRuleCompletedEventArgs>(client_AddDoTaskRuleCompleted);
            client.EditDoTaskRuleCompleted += new EventHandler<EditDoTaskRuleCompletedEventArgs>(client_EditDoTaskRuleCompleted);

            client.GetDoTaskRuleDetailCompleted += new EventHandler<GetDoTaskRuleDetailCompletedEventArgs>(client_GetDoTaskRuleDetailCompleted);
            client.DeleteDoTaskRuleDetailCompleted += new EventHandler<DeleteDoTaskRuleDetailCompletedEventArgs>(client_DeleteDoTaskRuleDetailCompleted);
            if (rule != null)
            {
                DetailItem.Visibility = Visibility.Visible;
                Rule = rule;
                InitEdit();
                client.GetDoTaskRuleDetailAsync(Rule.DOTASKRULEID);

            }
            else
            {
                DetailItem.Visibility = Visibility.Collapsed;
            }
        }

        void client_EditDoTaskRuleCompleted(object sender, EditDoTaskRuleCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存成功!", "确定");
                if (SaveDetailClick != null)
                {
                    SaveDetailClick(this, null);
                }
                DetailItem.Visibility = Visibility.Visible;
                // this.DialogResult = false;              
            }
            else if (e.Result == "2")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "此模块已存在此条件!", "确定");
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存失败!", "确定");
                //this.DialogResult = true;
            }
        }

        void InitEdit()
        {
            this.cbCondition.SelectedIndex = Rule.TRIGGERORDERSTATUS == null ? 0 : (int)Rule.TRIGGERORDERSTATUS;
        }
        void client_GetDoTaskRuleDetailCompleted(object sender, GetDoTaskRuleDetailCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DoTaskRuleDetailDataGrid.ItemsSource = e.Result;
            }
        }

        void client_AddDoTaskRuleCompleted(object sender, AddDoTaskRuleCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存成功!", "确定");
                DetailItem.Visibility = Visibility.Visible;
                //this.DialogResult = false;
                if (SaveDetailClick != null)
                {
                    SaveDetailClick(this, null);
                }
            }
            else if (e.Result == "2")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "此模块已存在此条件!", "确定");
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存失败!", "确定");
                // this.DialogResult = true;
            }
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbModelName.SelectedIndex < 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择模块!", "确定");
                return;
            }
            if (this.cbSystemName.SelectedIndex < 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择系统!", "确定");
                return;
            }
            if (this.cbCondition.SelectedIndex < 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择审核条件!", "确定");
                return;
            }

            if (Rule == null)
            {
                PlatformService.T_WF_DOTASKRULE rule = new PlatformService.T_WF_DOTASKRULE();
                rule.DOTASKRULEID = Guid.NewGuid().ToString();
                rule.SYSTEMCODE = (this.cbSystemName.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE;
                rule.SYSTEMNAME = (this.cbSystemName.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMNAME;
                rule.COMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
                rule.MODELCODE = (this.cbModelName.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;
                rule.MODELNAME = (this.cbModelName.SelectedItem as FLOW_MODELDEFINE_T).DESCRIPTION;
                rule.TRIGGERORDERSTATUS = this.cbCondition.SelectedIndex;
                Rule = rule;
                client.AddDoTaskRuleAsync(rule);
            }
            else
            {
                PlatformService.T_WF_DOTASKRULE WFrule = new PlatformService.T_WF_DOTASKRULE();
                WFrule.DOTASKRULEID = Rule.DOTASKRULEID;
                WFrule.SYSTEMCODE = (this.cbSystemName.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE;
                WFrule.SYSTEMNAME = (this.cbSystemName.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMNAME;
                WFrule.COMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
                WFrule.MODELCODE = (this.cbModelName.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;
                WFrule.MODELNAME = (this.cbModelName.SelectedItem as FLOW_MODELDEFINE_T).DESCRIPTION;
                WFrule.TRIGGERORDERSTATUS = this.cbCondition.SelectedIndex;
                client.EditDoTaskRuleAsync(WFrule);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cbSystemName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbSystemName.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null)
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.SYSTEMCODE == item.SYSTEMCODE || ent.MODELCODE == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cbModelName.ItemsSource = models;
                        this.cbModelName.SelectedIndex = 0;
                    }
                }
            }         
        }

        #region 子表事件
        private void btnDetailAdd_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbSystemName.SelectedItem != null && this.cbModelName.SelectedItem != null)
            {
                DoTaskRuleEdit edit = new DoTaskRuleEdit(this.cbSystemName.SelectedItem as FLOW_MODELDEFINE_T, this.cbModelName.SelectedItem as FLOW_MODELDEFINE_T, null, Rule.DOTASKRULEID);
                edit.SaveClick += (obj, ev) =>
                {
                    client.GetDoTaskRuleDetailAsync(Rule.DOTASKRULEID);

                };
                edit.chkIsMsg.IsChecked = true;//默认选 中

                edit.cbSystemCode.ItemsSource = this.cbSystemName.ItemsSource;
                edit.cbModelCode.ItemsSource = this.cbModelName.ItemsSource;
                edit.cbotherModelCode1.ItemsSource = this.cbModelName.ItemsSource;
                edit.cbotherModelCode1.SelectedIndex = 0;
                edit.cbOtherSystemCode1.ItemsSource = this.cbSystemName.ItemsSource;
                edit.cbOtherSystemCode1.SelectedIndex = 0;
                edit.cbModelCode.SelectedItem = this.cbModelName.SelectedItem;
                edit.cbSystemCode.SelectedItem = this.cbSystemName.SelectedItem;

                edit.appSystem = appSystem;
                edit.appModel = appModel;
                edit.Show();
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "系统或者模块未选择！", "确定");
            }
        }

        private void btnDetailEdit_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.T_WF_DOTASKRULEDETAIL Detail = DoTaskRuleDetailDataGrid.SelectedItem as PlatformService.T_WF_DOTASKRULEDETAIL;
            if (Detail == null)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择一条需要修改的记录!", "确定");
                return;
            }
            if (this.cbSystemName.SelectedItem != null && this.cbModelName.SelectedItem != null)
            {
                DoTaskRuleEdit edit = new DoTaskRuleEdit(this.cbSystemName.SelectedItem as FLOW_MODELDEFINE_T, this.cbModelName.SelectedItem as FLOW_MODELDEFINE_T, Detail, Rule.DOTASKRULEID);
                edit.SaveClick += (obj, ev) =>
                {
                    client.GetDoTaskRuleDetailAsync(Rule.DOTASKRULEID);

                };
                edit.cbSystemCode.ItemsSource = this.cbSystemName.ItemsSource;
                edit.cbModelCode.ItemsSource = this.cbModelName.ItemsSource;
                edit.cbOtherSystemCode1.ItemsSource = this.cbSystemName.ItemsSource;
                edit.appSystem = appSystem;
                edit.appModel = appModel;

                if (Detail != null)
                {
                    if (!string.IsNullOrEmpty(Detail.OTHERSYSTEMCODE))
                    {
                        for (int i = 0; i < this.cbSystemName.Items.Count; i++)
                        {
                            if (((this.cbSystemName.Items[i]) as AppSystem).Name == Detail.OTHERSYSTEMCODE)
                            {
                                edit.cbOtherSystemCode1.SelectedIndex = i;
                            }
                        }
                    }
                    else
                    {
                        edit.cbOtherSystemCode1.SelectedIndex = 0;
                    }
                }
                edit.cbModelCode.SelectedItem = this.cbModelName.SelectedItem;
                edit.cbSystemCode.SelectedItem = this.cbSystemName.SelectedItem;
                edit.Show();
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "系统或者模块未选择！", "确定");
            }
        }

        private void btnDetailDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DoTaskRuleDetailDataGrid.SelectedItems.Count == 1)
            {
                string Result = "";
                PlatformService.T_WF_DOTASKRULEDETAIL Detail = DoTaskRuleDetailDataGrid.SelectedItem as PlatformService.T_WF_DOTASKRULEDETAIL;
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.DeleteDoTaskRuleDetailAsync(Detail.DOTASKRULEDETAILID);
                };
                com.SelectionBox("删除确定", "你确定删除选中信息吗？", ComfirmWindow.titlename, Result);
            }
            else if (DoTaskRuleDetailDataGrid.SelectedItems.Count > 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "只能选择一条需要删除的数据!", "确定");
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择要删除的数据!", "确定");
            }
        }
        void client_DeleteDoTaskRuleDetailCompleted(object sender, DeleteDoTaskRuleDetailCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                client.GetDoTaskRuleDetailAsync(Rule.DOTASKRULEID);//再次帮定明细表
                ComfirmWindow.ConfirmationBox("提示信息", "删除成功!", "确定");
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "删除失败!", "确定");
            }
        }

        #endregion
    }
}

