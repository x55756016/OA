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
using SMT.Workflow.Platform.Designer.Utils;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.Class;
namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class DefaultMessgeEdit : ChildWindow
    {
        #region 全局变量
        PlatformService.FlowXmlDefineClient flowXmlClient = new FlowXmlDefineClient();
        PlatformService.MessageBodyDefineClient messageClient = new MessageBodyDefineClient();
        public ObservableCollection<TableColumn> ListTableColumn = new ObservableCollection<TableColumn>(); //赋值集合定义
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        public event EventHandler SaveCompleted;//保存完成事件
        string DEFINEID = "";
        private List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> OrgObj = null;
        private List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> OrgObjPost = null;
        public string ActionType = "";
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">0:新增;1修改</param>
        /// <param name="messageID">ID</param>
        public DefaultMessgeEdit(string action, string messageID)
        {
            InitializeComponent();
            DEFINEID = messageID;
            ActionType = action;
            ListTableColumn.Add(new TableColumn() { Description = "请选择.......", FieldName = "" });
            this.cmbColumn.ItemsSource = ListTableColumn;
            this.cmbColumn.SelectedIndex = 0;
            InitWcfEvent();
            BindData(messageID);
        }
        #region wcf注册完成事件
        private void InitWcfEvent()
        {
            #region 基本信息
            messageClient.GetDefaultMessgeListCompleted += (o, e) =>
                {
                    if (e.Error == null)
                    {
                        if (e.Result != null)
                        {
                            if (e.Result.FirstOrDefault() != null)
                            {
                                txtMSGCONTENT.Text = e.Result.FirstOrDefault().MESSAGEBODY;
                                this.cobSYSTEMCODE.Selected<FLOW_MODELDEFINE_T>("SYSTEMCODE", e.Result.FirstOrDefault().SYSTEMCODE);
                                this.cobMODELCODE.Selected<FLOW_MODELDEFINE_T>("MODELCODE", e.Result.FirstOrDefault().MODELCODE);                            
                                if (e.Result.FirstOrDefault().RECEIVETYPE == 1)
                                {
                                    this.rBUser.IsChecked = true;
                                    this.txtReceiveUser.Text = e.Result.FirstOrDefault().RECEIVERUSERNAME == null ? "" : e.Result.FirstOrDefault().RECEIVERUSERNAME;
                                    if (e.Result.FirstOrDefault().RECEIVERUSERNAME != null)
                                    {
                                        BindRoleUser(e.Result.FirstOrDefault(), true);
                                    }
                                }
                                else
                                {                                                   
                                    this.rBUser.IsChecked = false;
                                    this.txtReceivePost.Text = e.Result.FirstOrDefault().RECEIVEPOSTNAME == null ? "" : e.Result.FirstOrDefault().RECEIVEPOSTNAME;
                                    if (e.Result.FirstOrDefault().RECEIVEPOSTNAME != null)
                                    {
                                        BindRoleUser(e.Result.FirstOrDefault(), false);
                                    }
                                }
                                //OrgObjPost = new SaaS.FrameworkUI.OrganizationControl.ExtOrgObj() { ObjectID = e.Result.FirstOrDefault().RECEIVEPOSTID, ObjectName = e.Result.FirstOrDefault().RECEIVEPOSTNAME };
                                //OrgObj = new SaaS.FrameworkUI.OrganizationControl.ExtOrgObj() { ObjectID = e.Result.FirstOrDefault().RECEIVERUSERID, ObjectName = e.Result.FirstOrDefault().RECEIVERUSERNAME };
                                #region
                                FLOW_MODELDEFINE_T system = cobSYSTEMCODE.SelectedItem as FLOW_MODELDEFINE_T;
                                FLOW_MODELDEFINE_T model = cobMODELCODE.SelectedItem as FLOW_MODELDEFINE_T;
                                string strMsgOpen = string.Empty;
                                flowXmlClient.ListFuncTableColumnAsync(XmlUtils.GetSystemPath(system.SYSTEMCODE), model.MODELCODE, strMsgOpen);
                                #endregion

                            }
                        }
                    }
                };
            #endregion
            #region 消息连接
            flowXmlClient.ListFuncTableColumnCompleted += (o, e) =>
                {
                    if (e.Error == null)
                    {
                        if (e.Result != null)
                        {
                            ListTableColumn = e.Result.Values.FirstOrDefault();
                            ListTableColumn.Insert(0, new TableColumn() { Description = "请选择.......", FieldName = "" });
                            this.cmbColumn.ItemsSource = ListTableColumn;
                            this.cmbColumn.SelectedIndex = 0;
                            txtURL.Text = e.MsgLinkUrl;
                        }
                    }
                };
            #endregion
            #region 新增完成
            messageClient.AddDefaultMessgeCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        if (e.Result == "1")
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "新增成功", "确定");
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
                            ComfirmWindow.ConfirmationBox("提示信息", "新增失败", "确定");
                        }

                    }
                }

            };
            #endregion
            #region 修改完成
            messageClient.EditDefaultMessgeCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        if (e.Result == "1")
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "修改成功", "确定");
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
                            ComfirmWindow.ConfirmationBox("提示信息", "修改失败", "确定");
                        }
                    }
                }
            };

            #endregion
        }
        #endregion

        #region
        private void BindRoleUser(T_WF_MESSAGEBODYDEFINE entity, bool mark)
        {
            if (!mark)
            {
                string[] postarrID = entity.RECEIVEPOSTID.Split(',');
                string[] postarrName = entity.RECEIVEPOSTNAME.Split(',');
                OrgObjPost = new List<SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
                for (int i = 0; i < postarrID.Length; i++)
                {                 
                    OrgObjPost.Add(new SaaS.FrameworkUI.OrganizationControl.ExtOrgObj() { ObjectID = postarrID[i], ObjectName = postarrName[i] });
                }
            }
            else
            {
                string[] userID = entity.RECEIVERUSERID.Split(',');
                string[] userName = entity.RECEIVERUSERNAME.Split(',');
                OrgObj = new List<SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
                for (int i = 0; i < userID.Length; i++)
                {                   
                    OrgObj.Add(new SaaS.FrameworkUI.OrganizationControl.ExtOrgObj() { ObjectID = userID[i], ObjectName = userName[i] });
                }
            }
        }
        #endregion
        #region 绑定数据
        private void BindData(string messageID)
        {
            int pageCont = 0;
            string strFilter = " AND DEFINEID='" + messageID + "'";
            //var systemcode = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.AppSystem;
            //var modelCode = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.AppModel;          
            messageClient.GetDefaultMessgeListAsync(1, 1, strFilter, null, pageCont);
        }
        #endregion
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtMSGCONTENT.Text.Trim() == "")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "消息内容不能为空！", "确定");
                return;
            }
            FLOW_MODELDEFINE_T system = cobSYSTEMCODE.SelectedItem as FLOW_MODELDEFINE_T;
            FLOW_MODELDEFINE_T model = cobMODELCODE.SelectedItem as FLOW_MODELDEFINE_T;
            if (system.SYSTEMCODE == "0" || model.MODELCODE == "0")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请正确选择 [系统]和[模块]！", "确定");
                return;
            }
            PlatformService.T_WF_MESSAGEBODYDEFINE entity = new PlatformService.T_WF_MESSAGEBODYDEFINE();
            entity.MESSAGEBODY = ParamOperate.MessageBodyExchange(ListTableColumn.ToList(), true, txtMSGCONTENT.Text);
            entity.MESSAGEURL = txtURL.Text.Trim();
            entity.SYSTEMCODE = (cobSYSTEMCODE.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE;
            entity.MODELCODE = (cobMODELCODE.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;
            entity.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entity.RECEIVETYPE = this.rBRole.IsChecked == true ? 0 : 1;
            if (entity.RECEIVETYPE == 0 && OrgObjPost!=null&&OrgObjPost.Count>0)
            {
                foreach (var item in OrgObjPost)
                {
                    entity.RECEIVEPOSTID += item.ObjectID + ",";
                    entity.RECEIVEPOSTNAME += item.ObjectName + ",";
                }
                entity.RECEIVEPOSTID = entity.RECEIVEPOSTID.TrimEnd(',');
                entity.RECEIVEPOSTNAME = entity.RECEIVEPOSTNAME.TrimEnd(',');
            }
            else if (entity.RECEIVETYPE == 1 && OrgObj != null && OrgObj.Count > 0)
            {
                foreach (var item in OrgObj)
                {
                    entity.RECEIVERUSERID += item.ObjectID + ",";
                    entity.RECEIVERUSERNAME += item.ObjectName + ",";
                }
                entity.RECEIVERUSERID = entity.RECEIVERUSERID.TrimEnd(',');
                entity.RECEIVERUSERNAME = entity.RECEIVERUSERNAME.TrimEnd(',');
            }
            if (ActionType == "0")
            {

                entity.DEFINEID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                entity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                entity.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                messageClient.AddDefaultMessgeAsync(entity);
            }
            else
            {
                entity.DEFINEID = DEFINEID;
                messageClient.EditDefaultMessgeAsync(entity);
            }
            //this.DialogResult = true;
        }
   
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        #region 模块选择事件
        private void cobMODELCODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            FLOW_MODELDEFINE_T system = cobSYSTEMCODE.SelectedItem as FLOW_MODELDEFINE_T;
            FLOW_MODELDEFINE_T model = cobMODELCODE.SelectedItem as FLOW_MODELDEFINE_T;         
            string strMsgOpen = string.Empty;
            if (system != null && model != null && system.SYSTEMCODE != "0" && model.MODELCODE!="0")
            {
                flowXmlClient.ListFuncTableColumnAsync(XmlUtils.GetSystemPath(system.SYSTEMCODE), model.MODELCODE, strMsgOpen);
            }
        }
        #endregion
        #region 选择所属系统事件 与 模板联动
        private void cobSYSTEMCODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cobSYSTEMCODE.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null)
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.SYSTEMCODE == item.SYSTEMCODE || ent.MODELCODE == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cobMODELCODE.ItemsSource = models;
                        this.cobMODELCODE.SelectedIndex = 0;
                    }
                }
            }
        }
        #endregion

        private void btnLookUpUserName_Click(object sender, RoutedEventArgs e)
        {
            UserLooKUP up = new UserLooKUP();
            up.SelectedClick += (obj, ev) =>
            {
                if (up.SelectList != null)
                {
                    if (OrgObj != null)
                    {
                        OrgObj.Clear();
                    }   
                    OrgObj = up.SelectList;
                    string userName = "";
                    foreach (var item in up.SelectList)
                    {
                        userName += item.ObjectName + ",";
                    }
                    this.txtReceiveUser.Text = userName.TrimEnd(',');
                }
            };
            up.Show();
        }
        private void btnLookUpPostName_Click(object sender, RoutedEventArgs e)
        {
            LooKUP up = new LooKUP();
            up.SelectedClick += (obj, ev) =>
            {
                if (up.SelectList != null)
                {
                    string postName = "";
                    if (OrgObjPost != null)
                    {
                        OrgObjPost.Clear();
                    }                   
                    OrgObjPost = up.SelectList;
                    foreach (var item in up.SelectList)
                    {
                        postName += item.ObjectName + ",";
                    }
                    this.txtReceivePost.Text = postName.TrimEnd(',');
                }
            };
            up.Show();
        }

        private void cmbColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cmbColumn != null && this.cmbColumn.SelectedIndex > 0)
            {
                TableColumn Column = (sender as ComboBox).SelectedItem as TableColumn;
                this.txtMSGCONTENT.Text = this.txtMSGCONTENT.Text + "{new:" + Column.Description + "}";

            }
        }

        private void rBRole_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtReceiveUser != null)
            {
                this.tbReceiveUser.Visibility = Visibility.Visible;
                this.txtReceiveUser.Visibility = Visibility.Visible;
                this.btnLookUpUserName.Visibility = Visibility.Visible;
                tbReceivePost.Visibility = Visibility.Collapsed;
                txtReceivePost.Visibility = Visibility.Collapsed;
                btnLookUpPostName.Visibility = Visibility.Collapsed;
            }
        }

        private void rBRole_Checked(object sender, RoutedEventArgs e)
        {
            if (txtReceivePost != null)
            {
                tbReceivePost.Visibility = Visibility.Visible;
                txtReceivePost.Visibility = Visibility.Visible;
                btnLookUpPostName.Visibility = Visibility.Visible;
                this.tbReceiveUser.Visibility = Visibility.Collapsed;
                this.txtReceiveUser.Visibility = Visibility.Collapsed;
                this.btnLookUpUserName.Visibility = Visibility.Collapsed;
            }
        }
    }
}

