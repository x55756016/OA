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
using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.Utils;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class TimingTriggerEdit : ChildWindow
    {
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        public ObservableCollection<AppFunc> ListFunc = new ObservableCollection<AppFunc>();//系统功能
        public ObservableCollection<TableColumn> ListTableColumn = new ObservableCollection<TableColumn>(); //赋值集合定义
        private ObservableCollection<Param> listpatrm = new ObservableCollection<Param>();
        PlatformService.FlowXmlDefineClient flowXmlClient = new FlowXmlDefineClient();
        PlatformService.TimingTriggerClient triggerClient = new TimingTriggerClient();
        public Parameter paramet = new Parameter();//功能参数
        public ObservableCollection<Cycle> ListCycle = null; //周期
        public FlowXmlDefineClient XmlClient = null; //xml实例化
        private string MsgOpen = string.Empty;//消息链接定义     
        private PlatformService.T_WF_TIMINGTRIGGERACTIVITY Entity = null;       
        public string ActionType = "";
        public event EventHandler SaveTimingClick;
        public TimingTriggerEdit()
        {
            InitializeComponent();
        }
        public TimingTriggerEdit(string action, PlatformService.T_WF_TIMINGTRIGGERACTIVITY entity)
        {
            InitializeComponent();
            XmlClient = new FlowXmlDefineClient();
            ActionType = action;
            ListCycle = new ObservableCollection<Cycle>() 
            { 
                new Cycle{ CycleType="一次"},
                new Cycle{ CycleType="分钟"},
                new Cycle{ CycleType="小时"},
                new Cycle{ CycleType="天"},
                new Cycle{ CycleType="月"},
                new Cycle{ CycleType="年"}
            };
            this.cmbCycle.ItemsSource = ListCycle;
            if (entity != null)
            {
                Entity = entity;
                InitTiming();
            }
            else
            {
                myDate.Text = DateTime.Now.ToShortDateString();
                myTime.Value = DateTime.Now;
                this.cmbCycle.SelectedIndex = 3;
            }          
            triggerClient.EditTimingActivityCompleted += new EventHandler<EditTimingActivityCompletedEventArgs>(triggerClient_EditTimingActivityCompleted);
            triggerClient.AddTimingActivityCompleted += new EventHandler<AddTimingActivityCompletedEventArgs>(triggerClient_AddTimingActivityCompleted);
            XmlClient.ListFuncTableColumnCompleted += new EventHandler<ListFuncTableColumnCompletedEventArgs>(XmlClient_ListFuncTableColumnCompleted);
        }

        void InitTiming()
        {
            this.txtTriggerName.Text = Entity.TRIGGERNAME;
            this.myTime.Value = Entity.TRIGGERTIME;
            this.myDate.Text = Entity.TRIGGERTIME.ToShortDateString();
            if (Entity.TRIGGERTYPE == "User")
            {
                rbUser.IsChecked = true;
                rbSystem.IsChecked = false;
            }
            else
            {
                rbUser.IsChecked = false;
                rbSystem.IsChecked = true;
            }
            if (Entity.TRIGGERACTIVITYTYPE == 1)
            {
                rbSMS.IsChecked = true;
                rbService.IsChecked = false;
            }
            else
            {
                rbSMS.IsChecked = false;
                rbService.IsChecked = true;
            }
            for (int i = 0; i < ListCycle.Count; i++)
            {
                if (CycleOperate.CycleExchangeTo(ListCycle[i].CycleType) == Entity.TRIGGERROUND)
                {
                    this.cmbCycle.SelectedIndex = i;
                }
            }
            if (!string.IsNullOrEmpty(Entity.FUNCTIONPARAMTER))
            {
                listpatrm.Clear();
                ObservableCollection<Param> Params = ParamOperate.FieldToCollection(Entity.FUNCTIONPARAMTER);
                if (Params.Count > 0)
                {
                    foreach (Param p in Params)
                    {
                        listpatrm.Add(p);
                    }
                }
                this.txtValue.Text = Params[0].FieldName;
                RowList.ItemsSource = listpatrm;
            }

        }
        void triggerClient_AddTimingActivityCompleted(object sender, AddTimingActivityCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "1")
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "保存成功！", "确定");
                    if (this.SaveTimingClick != null)
                    {
                        this.SaveTimingClick(this, null);
                    }
                    this.DialogResult = false;
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "保存失败！", "确定");
                }
            }
        }

        void triggerClient_EditTimingActivityCompleted(object sender, EditTimingActivityCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "1")
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "修改成功！", "确定");
                    if (this.SaveTimingClick != null)
                    {
                        this.SaveTimingClick(this, null);
                    }
                    this.DialogResult = false;
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "修改失败！", "确定");
                }
            }
        }


        void XmlClient_ListFuncTableColumnCompleted(object sender, ListFuncTableColumnCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                AppFunc app = new AppFunc();
                ListFunc.Clear();
                app.Address = "";
                app.Binding = "";
                app.FuncName = "";
                app.Language = "请选择.......";
                app.Parameter = new System.Collections.ObjectModel.ObservableCollection<Parameter>();
                app.SplitChar = "";
                ListFunc.Add(app);
                if (e.Result != null)
                {
                    foreach (var item in e.Result.Keys.FirstOrDefault())
                    {
                        ListFunc.Add(item);
                    }
                }
                if (e.Result != null)
                {
                    ListTableColumn = e.Result.Values.FirstOrDefault();
                    this.cmbFunc.ItemsSource = ListFunc;
                    if (Entity == null)
                    {
                        this.cmbFunc.SelectedIndex = 0;
                    }
                    else
                    {
                        for (int i = 0; i < ListFunc.Count; i++)
                        {
                            if (ListFunc[i].FuncName == Entity.FUNCTIONNAME)
                            {
                                this.cmbFunc.SelectedIndex = i;
                            }
                        }
                    }
                }

                MsgOpen = e.MsgLinkUrl;
            }
        }
        #region 保存

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtTriggerName.Text.Trim() == "")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "定时触发名称不能为空！", "确定");
                return;
            }
            FLOW_MODELDEFINE_T system = cmbSystemCode.SelectedItem as FLOW_MODELDEFINE_T;
            FLOW_MODELDEFINE_T model = cmbModelCode.SelectedItem as FLOW_MODELDEFINE_T;
            if (system.SYSTEMCODE == "0" || model.MODELCODE == "0")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请正确选择 [系统名称]和[模块名称]！", "确定");
                return;
            }
            if (myDate.Text == "")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "开始时间不能为空！", "确定");
                return;
            }
            if (ActionType == "0")
            {
                #region 新增
                Entity = new PlatformService.T_WF_TIMINGTRIGGERACTIVITY();
                Entity.TRIGGERID = Guid.NewGuid().ToString();
                Entity.TRIGGERNAME = txtTriggerName.Text;
                Entity.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                Entity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Entity.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                Entity.SYSTEMCODE = (this.cmbSystemCode.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE;
                Entity.SYSTEMNAME = (this.cmbSystemCode.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMNAME;
                Entity.MODELCODE = (this.cmbModelCode.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;
                Entity.MODELNAME = (this.cmbModelCode.SelectedItem as FLOW_MODELDEFINE_T).DESCRIPTION;
                Entity.TRIGGERROUND = CycleOperate.CycleExchangeTo((this.cmbCycle.SelectedItem as Cycle).CycleType);
                Entity.CONTRACTTYPE = "Engine";
                Entity.TRIGGERTYPE = rbUser.IsChecked == true ? "User" : "System";
                Entity.TRIGGERTIME = DateTime.Parse(myDate.Text + " " + myTime.Value.Value.TimeOfDay.ToString());
                Entity.TRIGGERACTIVITYTYPE = rbSMS.IsChecked == true ? 1 : 2;
                Entity.MESSAGEURL = MsgOpen;

                if (this.txtValue.Text.Trim() != "" && cmbFunc.SelectedIndex > 0)
                {
                    AppFunc func = cmbFunc.SelectedItem as AppFunc;
                    Entity.FUNCTIONNAME = func.FuncName;
                    Entity.FUNCTIONPARAMTER = ParamOperate.CollectionToString(listpatrm);
                    Entity.WCFURL = func.Address;
                    Entity.WCFBINDINGCONTRACT = func.Binding;
                    Entity.PAMETERSPLITCHAR = func.SplitChar;
                }
                else
                {
                    Entity.FUNCTIONNAME = string.Empty;
                    Entity.FUNCTIONPARAMTER = string.Empty;
                    Entity.WCFURL = string.Empty;
                    Entity.WCFBINDINGCONTRACT = string.Empty;
                    Entity.PAMETERSPLITCHAR = string.Empty;
                }
                triggerClient.AddTimingActivityAsync(Entity);
                #endregion
            }
            else
            {
                #region 修改
                Entity.TRIGGERNAME = txtTriggerName.Text;
                Entity.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                Entity.SYSTEMCODE = (this.cmbSystemCode.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE;
                Entity.SYSTEMNAME = (this.cmbSystemCode.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMNAME;
                Entity.MODELCODE = (this.cmbModelCode.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;
                Entity.MODELNAME = (this.cmbModelCode.SelectedItem as FLOW_MODELDEFINE_T).DESCRIPTION;
                Entity.TRIGGERROUND = CycleOperate.CycleExchangeTo((this.cmbCycle.SelectedItem as Cycle).CycleType);
                Entity.CONTRACTTYPE = "Engine";
                Entity.TRIGGERTYPE = rbUser.IsChecked == true ? "User" : "System";
                Entity.TRIGGERTIME = DateTime.Parse(myDate.Text + " " + myTime.Value.Value.TimeOfDay.ToString());
                Entity.TRIGGERACTIVITYTYPE = rbSMS.IsChecked == true ? 1 : 2;
                Entity.MESSAGEURL = MsgOpen;
                if (this.txtValue.Text.Trim() != "" && cmbFunc.SelectedIndex > 0)
                {
                    AppFunc func = cmbFunc.SelectedItem as AppFunc;
                    Entity.FUNCTIONNAME = func.FuncName;
                    Entity.FUNCTIONPARAMTER = ParamOperate.CollectionToString(listpatrm);
                    Entity.WCFURL = func.Address;
                    Entity.WCFBINDINGCONTRACT = func.Binding;
                    Entity.PAMETERSPLITCHAR = func.SplitChar;
                }
                else
                {
                    Entity.FUNCTIONNAME = string.Empty;
                    Entity.FUNCTIONPARAMTER = string.Empty;
                    Entity.WCFURL = string.Empty;
                    Entity.WCFBINDINGCONTRACT = string.Empty;
                    Entity.PAMETERSPLITCHAR = string.Empty;
                }
                #endregion
                triggerClient.EditTimingActivityAsync(Entity);
            }
        }
        #endregion
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        private void ParamRows_OnSubmitComplated(object sender, Form.DefineClickedArgs args)
        {
            listpatrm.Remove(args.SelectedItem);
        }

        private void btnSaveParam_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtValue.Text.Trim() != "" && cmbFunc.SelectedIndex > 0)
            {
                listpatrm = ParamOperate.Init();
                Parameter param = this.cmbParameter.SelectedItem as Parameter;
                Param paramClass = new Param();
                paramClass.ParamName = param.Description;
                paramClass.ParamID = param.Name;
                paramClass.FieldName = this.txtValue.Text.Trim();
                paramClass.FieldID = this.txtValue.Text.Trim();
                paramClass.TableName = param.TableName;
                paramClass.Description = param.Description;
                listpatrm.Add(paramClass);
                RowList.ItemsSource = listpatrm;
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "功能参数和参数赋值不能为空!", "确定");
            }
        }



        private void cmbFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFunc != null && this.cmbFunc.SelectedIndex > 0)
            {
                this.cmbParameter.ItemsSource = (this.cmbFunc.SelectedItem as AppFunc).Parameter;
                if (this.cmbParameter.Items.Count > 0)
                {
                    this.cmbParameter.SelectedIndex = 0;
                }
            }
        }

        private void cmbModelCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbModelCode != null)
            {
                string strMsgOpen = string.Empty;
                var itemSystem = cmbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
                var itemMode = cmbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;

                if (itemSystem != null && itemMode != null)
                {
                    XmlClient.ListFuncTableColumnAsync(XmlUtils.GetSystemPath(itemSystem.SYSTEMCODE), itemMode.MODELCODE, strMsgOpen);
                }
            }
        }
        #region 选择所属系统事件 与 模板联动
        private void cmbSystemCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cmbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null)
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.SYSTEMCODE == item.SYSTEMCODE || ent.MODELCODE == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cmbModelCode.ItemsSource = models;
                        this.cmbModelCode.SelectedIndex = 0;
                    }
                }
            }           
        }
        #endregion




    }
}

