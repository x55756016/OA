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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.Utils;
using System.Text.RegularExpressions;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class DoTaskRuleEdit : ChildWindow
    {
        public ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
        public FlowXmlDefineClient XmlClient = null; //xml实例化
        public ObservableCollection<AppFunc> ListFunc = new ObservableCollection<AppFunc>();//系统功能
        private string MsgOpen = string.Empty;//消息链接定义
        private ObservableCollection<Param> listpatrm =ParamOperate.Init();// new ObservableCollection<Param>();
        public ObservableCollection<TableColumn> ListTableColumn = new ObservableCollection<TableColumn>(); //赋值集合定义
        private PlatformService.T_WF_DOTASKRULEDETAIL Entity = null;
        private PlatformService.DoTaskRuleClient client = new DoTaskRuleClient();
        private string RuleID = "";
        private SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj OrgObj = null;
        public event EventHandler SaveClick;
        public DoTaskRuleEdit(FLOW_MODELDEFINE_T system, FLOW_MODELDEFINE_T model, PlatformService.T_WF_DOTASKRULEDETAIL entity, string ruleID)
        {
            InitializeComponent();
            XmlClient = new FlowXmlDefineClient();
            string strMsgOpen = string.Empty;
            RuleID = ruleID;

            XmlClient.ListFuncTableColumnCompleted += new EventHandler<ListFuncTableColumnCompletedEventArgs>(XmlClient_ListFuncTableColumnCompleted);

            client.EditDoTaskRuleDetailCompleted += new EventHandler<EditDoTaskRuleDetailCompletedEventArgs>(client_EditDoTaskRuleDetailCompleted);
            client.AddDoTaskRuleDetailCompleted += new EventHandler<AddDoTaskRuleDetailCompletedEventArgs>(client_AddDoTaskRuleDetailCompleted);
            //加载子系统xml
            //XmlClient.ListModelCompleted += new EventHandler<ListModelCompletedEventArgs>(clientXml_ListModelCompleted);
            if (system != null && model != null)
            {
                XmlClient.ListFuncTableColumnAsync(XmlUtils.GetSystemPath(system.SYSTEMCODE), model.MODELCODE, strMsgOpen);
            }
            if (entity != null)
            {
                Entity = entity;
             
                InitDetail();
            }
            else
            {
                txtAvailabilityProcessDates.Text = "3";
            }
            if (chkIsMsg.IsChecked == true)
            {
                SetIsEnabled(false);
            }
        }
        #region 初始化明细      
        void InitDetail()
        {
            if (Entity.ISDEFAULTMSG == 1)
            {
                chkIsMsg.IsChecked = true;
            }
            if (Entity.ISOTHERSOURCE == "1")
            {
                chkOther.IsChecked = true;
            }
            txtAvailabilityProcessDates.Text = Entity.LASTDAYS.ToString();
            if (!string.IsNullOrEmpty(Entity.RECEIVEUSERNAME))
            {
                OrgObj = new SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                OrgObj.ObjectID = Entity.OWNERPOSTID;
                OrgObj.ObjectName = Entity.RECEIVEUSERNAME;
                txtReceiveUser.Text = Entity.RECEIVEUSERNAME;
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
                RowList.ItemsSource = listpatrm;
            }
            this.txtMessageBody.Text = Entity.MESSAGEBODY;
        }
        #endregion

        void client_AddDoTaskRuleDetailCompleted(object sender, AddDoTaskRuleDetailCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存成功！", "确定");
                if (this.SaveClick != null)
                {
                    this.SaveClick(this, null);
                }
                this.DialogResult = false;
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存失败！", "确定");
            }
        }

        void client_EditDoTaskRuleDetailCompleted(object sender, EditDoTaskRuleDetailCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存成功！", "确定");
                if (this.SaveClick != null)
                {
                    this.SaveClick(this, null);
                }
                this.DialogResult = false;
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "保存失败！", "确定");
            }
        }
      
        #region 加载功能名称
        private int first = 0;//第一次进来绑定
        private bool otherModel = false;//是否是其他模块选择，如果是，则功能名称不变
        void XmlClient_ListFuncTableColumnCompleted(object sender, ListFuncTableColumnCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (otherModel)
                {
                    #region 其他模块选择                  
                    if (e.Result != null)
                    {
                        ListTableColumn = e.Result.Values.FirstOrDefault();
                        this.cmbValue.ItemsSource = ListTableColumn;
                        //this.cmbColumn.ItemsSource = ListTableColumn;                       
                    }                    
                    #endregion
                }
                else
                {
                    #region 模块名称选择
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
                        this.cmbValue.ItemsSource = ListTableColumn;
                        this.cmbColumn.ItemsSource = ListTableColumn;
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
                    if (Entity != null)
                    {
                        if (first == 0)
                        {
                            this.cbSystemCode.SelectedByObject<FLOW_MODELDEFINE_T>("SYSTEMCODE", Entity.SYSTEMCODE);
                            this.cbModelCode.SelectedByObject<FLOW_MODELDEFINE_T>("MODELCODE", Entity.MODELCODE);
                            this.cmbFunc.SelectedByObject<AppFunc>("FuncName", Entity.FUNCTIONNAME);
                            first++;
                        }
                    }                   
                    #endregion
                    MsgOpen = e.MsgLinkUrl;
                }
                if (this.chkIsMsg.IsChecked == true)
                {
                    SetIsEnabled(false);
                }
              
            }
        }
        #endregion


        /// <summary>
        /// 功能参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveParam_Click(object sender, RoutedEventArgs e)
        {
            
            if (cmbFunc.SelectedIndex > 0 && cmbParameter.SelectedItem != null)
            {
              //  listpatrm = ParamOperate.Init();
                Parameter param = this.cmbParameter.SelectedItem as Parameter;
                Param paramClass = new Param();
                paramClass.ParamName = param.Description;
                paramClass.ParamID = param.Name;
               TableColumn tc= this.cmbValue.SelectedItem as TableColumn;
                paramClass.FieldName = tc!=null?tc.Description:"";          
                paramClass.FieldID = tc!=null?tc.FieldName:"";

                paramClass.TableName = param.TableName;
                paramClass.Description = param.Description;
                listpatrm.Add(paramClass);
                RowList.ItemsSource = listpatrm;
            }
            else
            {
                //MessageBox.Show("功能参数和赋值变量不能为空!");
                ComfirmWindow.ConfirmationBox("提示信息", "功能参数和赋值变量不能为空!", "确定");
            }
        }

        private void cmbColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cmbColumn != null && this.cmbColumn.SelectedItem != null)
            {
                TableColumn Column = (sender as ComboBox).SelectedItem as TableColumn;
                this.txtMessageBody.Text = this.txtMessageBody.Text + "{new:" + Column.Description + "}";

            }
        }

        private void chkOther_Checked(object sender, RoutedEventArgs e)
        {
            if (chkOther.IsChecked == true)
            {
                this.cbOtherSystemCode1.IsEnabled = true;
                this.cbotherModelCode1.IsEnabled = true;
            }
        }
        private void chkOther_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkOther.IsChecked != true)
            {
                this.cbOtherSystemCode1.IsEnabled = false;
                this.cbotherModelCode1.IsEnabled = false;
            }
        }


        private void ParamRows_OnSubmitComplated(object sender, Form.DefineClickedArgs args)
        {
            listpatrm.Remove(args.SelectedItem);
        }
        #region 选择岗位
      
        private void btnLookUpUserName_Click(object sender, RoutedEventArgs e)
        {
            LooKUP up = new LooKUP();
            up.SelectedClick += (obj, ev) =>
                {
                    if (up.SelectList != null)
                    {
                        OrgObj = up.SelectList.FirstOrDefault();
                        this.txtReceiveUser.Text = up.SelectList.FirstOrDefault().ObjectName;
                    }
                };
            up.Show();
        }
         #endregion
        private void cbOtherSystemCode1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbOtherSystemCode1.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            if (item != null)
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.SYSTEMCODE == item.SYSTEMCODE || ent.MODELCODE == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cbotherModelCode1.ItemsSource = models;
                        this.cbotherModelCode1.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Entity == null)
            {
                client.AddDoTaskRuleDetailAsync(GetDetail("0"));
            }
            else
            {
                client.EditDoTaskRuleDetailAsync(GetDetail("1"));
            }
        }

        private T_WF_DOTASKRULEDETAIL GetDetail(string action)
        {
            if (action == "0")
            {
                Entity = new T_WF_DOTASKRULEDETAIL();
                Entity.DOTASKRULEDETAILID = Guid.NewGuid().ToString();
            }
            Entity.DOTASKRULEID = RuleID;
            Entity.COMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
            Entity.OWNERCOMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
            Entity.CREATEUSERID = Utility.CurrentUser.OWNERID;
            Entity.OWNERDEPARTMENTID = Utility.CurrentUser.OWNERDEPARTMENTID;
            Entity.CREATEUSERNAME = Utility.CurrentUser.USERNAME;

            Entity.SYSTEMCODE = ((cbSystemCode.SelectedItem) as FLOW_MODELDEFINE_T).SYSTEMCODE;
            Entity.SYSTEMNAME = ((cbSystemCode.SelectedItem) as FLOW_MODELDEFINE_T).SYSTEMNAME;
            Entity.MODELCODE = ((cbModelCode.SelectedItem) as FLOW_MODELDEFINE_T).MODELCODE;
            Entity.MODELNAME = ((cbModelCode.SelectedItem) as FLOW_MODELDEFINE_T).DESCRIPTION;
            if (txtMessageBody.Text.Replace(" ", "") != "")
            {
                Entity.MESSAGEBODY = ParamOperate.MessageBodyExchange(ListTableColumn.ToList(), true, txtMessageBody.Text);
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "触发规则中消息定义不能为空！", "确定");
                return null;
            }
            if (txtAvailabilityProcessDates.Text != null)
            {
                if (Regex.IsMatch(txtAvailabilityProcessDates.Text.ToString(), @"^\d+$"))
                {
                    Entity.LASTDAYS = int.Parse(txtAvailabilityProcessDates.Text);
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "有效处理日期格式不正确!请输入正确格式!", "确定");
                    return null;
                }
            }
            else
            {
                Entity.LASTDAYS = 3;
            }
            if (this.txtReceiveUser.Text.Trim() != "" && OrgObj != null)
            {
                Entity.OWNERPOSTID = OrgObj.ObjectID;
                Entity.RECEIVEUSERNAME = OrgObj.ObjectName;
            }
            Entity.ISDEFAULTMSG = chkIsMsg.IsChecked == true ? 1 : 0;
            Entity.APPLICATIONURL = MsgOpen;
            if (chkOther.IsChecked == true)
            {
                Entity.ISOTHERSOURCE = "1";
                Entity.OTHERSYSTEMCODE = (cbOtherSystemCode1.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE;
                Entity.OTHERMODELCODE = (cbotherModelCode1.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;
            }
            else
            {
                Entity.ISOTHERSOURCE = "0";
                Entity.OTHERSYSTEMCODE = "";
                Entity.OTHERMODELCODE = "";
            }
            if (RowList.ItemsSource != null)
            {
                AppFunc func = this.cmbFunc.SelectedItem as AppFunc;
                if (func != null)
                {
                    Entity.FUNCTIONNAME = func.FuncName;
                    Entity.FUNCTIONPARAMTER = ParamOperate.CollectionToString(listpatrm);
                    Entity.WCFURL = func.Address;
                    Entity.WCFBINDINGCONTRACT = func.Binding;
                    Entity.PAMETERSPLITCHAR = func.SplitChar;
                }
            }
            return Entity;
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
            }
        }
        #endregion   

        #region 模块名称选择
       
        private void cbModelCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            otherModel = false;
            string strMsgOpen = string.Empty;
            var system = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            var model = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            
            if (system != null && model != null && system.SYSTEMCODE != "0" && model.MODELCODE!="0")
            {
                    XmlClient.ListFuncTableColumnAsync(XmlUtils.GetSystemPath(system.SYSTEMCODE), model.MODELCODE, strMsgOpen);
                
            }
        }
        #endregion
        #region 是否选中默认消息
        private void chkIsMsg_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsMsg.IsChecked==true)
            {
                SetIsEnabled(false);               
            }
            else
            {
              SetIsEnabled(true);
               
            }
        }
        public void SetIsEnabled(bool bol)
        {
            this.cmbFunc.IsEnabled = bol;//功能名称
            this.cmbParameter.IsEnabled = bol;//功能参数
            this.cmbValue.IsEnabled = bol;//赋值变量
            this.RowList.IsEnabled = bol;//赋值变量列表
            this.chkOther.IsEnabled = bol;//其它来源
            this.cbOtherSystemCode1.IsEnabled = bol;//其他系统
            this.cbotherModelCode1.IsEnabled = bol;//其他模块
            this.btnSaveParam.IsEnabled = bol;//添加
            this.btnLookUpUserName.IsEnabled = bol;//接收岗位
        }       
        #endregion
        #region 其他模块选择关联
       
        private void cbotherModelCode1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            otherModel = true;
            string strMsgOpen = string.Empty;
            var system = cbOtherSystemCode1.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;
            var model = cbotherModelCode1.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.FLOW_MODELDEFINE_T;

            if (system != null && model != null && system.SYSTEMCODE != "0" && model.MODELCODE != "0")
            {
                XmlClient.ListFuncTableColumnAsync(XmlUtils.GetSystemPath(system.SYSTEMCODE), model.MODELCODE, strMsgOpen);
            }
            
        }
        #endregion
    }
}

