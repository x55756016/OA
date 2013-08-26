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
using SMT.Workflow.Platform.Designer.PlatformService;
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.Views.Message
{
    public partial class EditDefaultMessage : ChildWindow
    {
        PlatformService.FlowXmlDefineClient flowXmlClient = new FlowXmlDefineClient();
        PlatformService.DefaultMessageClient client = new PlatformService.DefaultMessageClient();
        public event EventHandler SaveCompleted;//保存完成事件
        public ObservableCollection<AppModel> appModel;//XML模块代码
        public ObservableCollection<AppSystem> appSystem;//XML系统代码
        PlatformService.T_WF_DEFAULTMESSAGE entity = null;
        string ActionType = "";
        public EditDefaultMessage(T_WF_DEFAULTMESSAGE Entity)
        {
            InitializeComponent();
            if (Entity == null)
            {
                ActionType = "0";
                entity = new T_WF_DEFAULTMESSAGE();
            }
            else
            {
                ActionType = "1";
                entity = Entity;
                txtMSGCONTENT.Text = entity.MESSAGECONTENT == null ? "" : entity.MESSAGECONTENT; ;
                //this.cobSYSTEMCODE.Selected<AppSystem>("Name", entity.SYSTEMCODE);
                //this.cobMODELCODE.Selected<AppModel>("Name", entity.MODELCODE);
                this.cbCondition.SelectedIndex = entity.AUDITSTATE == null ? 0 : (int)entity.AUDITSTATE;
                this.txtURL.Text = entity.APPLICATIONURL == null ? "" : entity.APPLICATIONURL;
            }
            InitWCFCompleted();
        }

        private void InitWCFCompleted()
        {
            flowXmlClient.ListSystemFuncCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        txtURL.Text = e.MsgLinkUrl;
                    }
                }
            };
            client.EditMessageCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result == "1")
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "修改成功", "确定");
                        if (this.SaveCompleted != null)
                        {
                            this.SaveCompleted(this, null);
                        }
                        this.DialogResult = false;
                        this.ActionType = "1";
                    }
                    else if (e.Result == "2")
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "此模块已经存在默认消息", "确定");
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "修改失败", "确定");
                    }
                }
            };
            client.AddMessageCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result == "1")
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "新增成功", "确定");
                        if (this.SaveCompleted != null)
                        {
                            this.SaveCompleted(this, null);
                        }
                        this.DialogResult = false;
                    }
                    else if (e.Result == "2")
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "此模块已经存在默认消息", "确定");
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "新增失败", "确定");
                    }
                }
            };
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtMSGCONTENT.Text.Trim() == "")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "消息内容不能为空！", "确定");
                return;
            }
            AppSystem system = cobSYSTEMCODE.SelectedItem as AppSystem;
            AppModel model = cobMODELCODE.SelectedItem as AppModel;
            if (system.Name == "0" || model.Name == "0")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请正确选择 [系统]和[模块]！", "确定");
                return;
            }
            if (this.cbCondition.SelectedIndex < 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择审核条件!", "确定");
                return;
            }

            if (ActionType == "0")
            {
                T_WF_DEFAULTMESSAGE ent = new T_WF_DEFAULTMESSAGE();
                ent.MESSAGEID = Guid.NewGuid().ToString();
                ent.MESSAGECONTENT = txtMSGCONTENT.Text.Trim();
                ent.APPLICATIONURL = txtURL.Text.Trim();
                ent.AUDITSTATE = this.cbCondition.SelectedIndex;
                ent.SYSTEMCODE = (cobSYSTEMCODE.SelectedItem as AppSystem).Name;
                ent.SYSTEMNAME = (cobSYSTEMCODE.SelectedItem as AppSystem).Description;
                ent.MODELCODE = (cobMODELCODE.SelectedItem as AppModel).Name;
                ent.MODELNAME = (cobMODELCODE.SelectedItem as AppModel).Description;
                //entity.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                //entity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.AddMessageAsync(ent);
            }
            else
            {
                T_WF_DEFAULTMESSAGE ent = new T_WF_DEFAULTMESSAGE();
                ent.MESSAGEID = entity.MESSAGEID;
                ent.MESSAGECONTENT = txtMSGCONTENT.Text.Trim();
                ent.APPLICATIONURL = txtURL.Text.Trim();
                ent.AUDITSTATE = this.cbCondition.SelectedIndex;
                ent.SYSTEMCODE = (cobSYSTEMCODE.SelectedItem as AppSystem).Name;
                ent.SYSTEMNAME = (cobSYSTEMCODE.SelectedItem as AppSystem).Description;
                ent.MODELCODE = (cobMODELCODE.SelectedItem as AppModel).Name;
                ent.MODELNAME = (cobMODELCODE.SelectedItem as AppModel).Description;
                ent.CREATEUSERNAME = entity.CREATEUSERNAME;
                ent.CREATEUSERID = entity.CREATEUSERID;
                //ent.UPDATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                //ent.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.EditMessageAsync(ent);
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cobSYSTEMCODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cobSYSTEMCODE.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.AppSystem;
            if (item != null && item.ObjectFolder != null)
            {//ent.Name=="0" ==请选择......
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.ObjectFolder == item.ObjectFolder || ent.Name == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cobMODELCODE.ItemsSource = models;
                        this.cobMODELCODE.SelectedIndex = 0;
                    }
                }
            }
            if (item != null && item.Name == "0")
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.Name == item.Name
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cobMODELCODE.ItemsSource = models;
                        this.cobMODELCODE.SelectedIndex = 0;
                    }
                }
            }
        }

        private void cobMODELCODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSystem system = cobSYSTEMCODE.SelectedItem as AppSystem;
            AppModel model = cobMODELCODE.SelectedItem as AppModel;
            string strMsgOpen = string.Empty;
            if (system != null && model != null)
            {
                flowXmlClient.ListSystemFuncAsync(system.ObjectFolder, model.Name, strMsgOpen);
            }
        }
    }
}

