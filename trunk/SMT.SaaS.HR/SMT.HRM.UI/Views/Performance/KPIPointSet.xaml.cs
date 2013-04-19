/// <summary>
/// Log No.： 1
/// Modify Desc： 滚动条未成，等待控制，角色（或节点）使用Remark（自写备注），过滤权限，流程平台脚本错误
/// Modifier： 绩效模块使用
/// Modify Date： 2010-08-09
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using SMT.Saas.Tools.PerformanceWS;
using FlowDesignerWS = SMT.Saas.Tools.FlowDesignerWS;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form.Performance;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using SMT.HRM.UI.Active;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.FlowDesignerWS;

namespace SMT.HRM.UI.Views.Performance
{
    public partial class KPIPointSet : BasePage, IContainer
    {
        SMTLoading loadbar = new SMTLoading();

        private string isTag;

        public string IsTag
        {
            get { return isTag; }
            set { isTag = value; }
        }

        private PerformanceServiceClient client = new PerformanceServiceClient();  //绩效考核服务
        private ServiceClient fClient = new ServiceClient();     //流程信息服务
        PermissionServiceClient PermissionServiceWcf = new PermissionServiceClient();
        string FlowDefine = null;
        FlowDesignerWS.WFBOSystem BOSystem = new FlowDesignerWS.WFBOSystem();
        FlowDesignerWS.WFBOObject BOObject = new FlowDesignerWS.WFBOObject();
        //节点列表
        public List<StateActive> Actives = new List<StateActive>();
        //规则列表
        public List<RuleLine> Rules = new List<RuleLine>();

        List<StateType> StateList = new List<StateType> 
                       {
                         new StateType{StateCode="HrState",StateName="人事经理2"},
                         new StateType{StateCode="AcountState",StateName="财务经理2"},
                         new StateType{StateCode="ManageState",StateName="部门经理2"}
                        };
        bool d_RuleActiveSet = true;
        bool d_StateActiveSetS = true;

        //  public StateActiveSet StateActiveSetS = new StateActiveSet();


        StateActiveSet StateActiveSetS = new StateActiveSet();//创建状态窗口
        RuleActiveSet RuleActiveSetS = new RuleActiveSet();//创建规则窗口

        public FLOW_MODELFLOWRELATION_T SelectedModelFlowRelation { get; set; }                  //当前所选模块

        public ObservableCollection<FLOW_MODELFLOWRELATION_T> ModelFlowRelations = new ObservableCollection<FLOW_MODELFLOWRELATION_T>();  //模块流程列表
        public ObservableCollection<T_HR_KPIPOINT> KPIPointList = new ObservableCollection<T_HR_KPIPOINT>();  //模块流程列表

        public KPIPointSet()
        {
            InitializeComponent();            
            this.Loaded += new RoutedEventHandler(KPIPointSet_Loaded);
        }

        /// <summary>
        /// 读取页面事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void KPIPointSet_Loaded(object sender, RoutedEventArgs e)
        {
            InitPara();
            GetEntityLogo("T_HR_KPIPOINT");
            // 1s 冉龙军
            //LoadData();
            //PermissionServiceWcf.GetSysRoleInfosAsync("", "");
            //没有权限没有数据
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue("T_HR_KPIPOINT", SMT.SaaS.FrameworkUI.Common.Permissions.Browse) < 0)
            {
            }
            else
            {
                LoadData();
                PermissionServiceWcf.GetSysRoleInfosAsync("", "");
            }
            // 1e
            // 1s 冉龙军
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_KPIPOINT", false);
            // 1e
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 1s 冉龙军
            //Utility.DisplayGridToolBarButton(ToolBar, "KPIPointSet", false);
            // 1e
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 初始化事件
        /// </summary>
        public void InitPara()
        {
            try
            {
                // 1s 冉龙军
                //LayoutRoot.Children.Add(loadbar);
                //loadbar.Start();
                PARENT.Children.Add(loadbar);
                // 1e
                fClient.GetModelFlowRelationInfosListBySearchCompleted += new EventHandler<GetModelFlowRelationInfosListBySearchCompletedEventArgs>(fClient_GetModelFlowRelationInfosListBySearchCompleted);
                fClient.GetFlowDefineByFlowCodeCompleted += new EventHandler<GetFlowDefineByFlowCodeCompletedEventArgs>(fClient_GetFlowDefineByFlowCodeCompleted);

                PermissionServiceWcf.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(GetSysRoleInfosCompleted);

                client.GetKPIPointListByBusinessCodeCompleted += new EventHandler<GetKPIPointListByBusinessCodeCompletedEventArgs>(client_GetKPIPointListByBusinessCodeCompleted);

                //this.Loaded += new RoutedEventHandler(KPIPointSet_Loaded);
                StateActiveSetS.StateActiveSet_Click += new EventHandler<OnClickEventArgs>(StateActiveSetS_StateActiveSet_Click);
                RuleActiveSetS.Rule_OnClick += new EventHandler<Rule_OnClickEventArgs>(RuleActiveSetS_Rule_OnClick);
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
        }

        /// <summary>
        /// 刷新抽查组列表
        /// </summary>
        void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            filter = " COMPANYID =@" + paras.Count().ToString();
            paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtMoudleName");
            if (!string.IsNullOrEmpty(txtEmpName.Text))
            {
                filter += " and ";

                filter += " FLOW_MODELDEFINE_T.DESCRIPTION==@" + paras.Count().ToString();
                paras.Add(txtEmpName.Text);
            }
            // fClient.GetModelFlowRelationInfosListBySearchAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, "");//(dataPager.PageIndex, dataPager.PageSize, "RANDOMGROUPID", filter, paras, pageCount);
            fClient.GetModelFlowRelationInfosListBySearchAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount);
            // 1s 冉龙军
            loadbar.Start();
            // 1e
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
        /// <summary>
        /// 获取所有模块定义后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fClient_GetModelFlowRelationInfosListBySearchCompleted(object sender, GetModelFlowRelationInfosListBySearchCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else                {
                    if (e.Result != null)
                    {
                        List<FLOW_MODELFLOWRELATION_T> dicts = Application.Current.Resources["SYS_FlowInfo"] as List<FLOW_MODELFLOWRELATION_T>;
                        if (dicts == null || dicts.Count == 0)
                        {
                            Application.Current.Resources.Add("SYS_FlowInfo", e.Result.ToList());
                        }
                        ModelFlowRelations = e.Result;
                    }
                    dtgProject.ItemsSource = ModelFlowRelations;
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                loadbar.Stop();
            }
        }

        /// <summary>
        /// 获取所有模块定义后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIPointListByBusinessCodeCompleted(object sender, GetKPIPointListByBusinessCodeCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                this.KPIPointList = e.Result;
                //SetLayout();
                fClient.GetFlowDefineByFlowCodeAsync(((FLOW_MODELFLOWRELATION_T)dtgProject.SelectedItem).FLOW_FLOWDEFINE_T.FLOWCODE);
            }
        }

        /// <summary>
        /// 获取所有模块定义后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fClient_GetFlowDefineByFlowCodeCompleted(object sender, GetFlowDefineByFlowCodeCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                if (e.Result != null)
                {
                    FlowDefine = e.Result.ToString();
                    SetLayout();
                }
            }
        }

        /// <summary>
        /// 獲取角色的東東
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {
            // 1s 冉龙军
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                return;
            }
            // 1e
            if (e.Result != null)
            {
                StateList.Clear();

                List<T_SYS_ROLE> dt = e.Result.ToList<T_SYS_ROLE>(); ;

                for (int i = 0; i < dt.Count; i++)
                {
                    StateType tmp = new StateType();
                    tmp.StateCode = "State" + new Guid(dt[i].ROLEID).ToString("N");
                    tmp.StateName = dt[i].ROLENAME;
                    StateList.Add(tmp);

                }
                SetStatelist();
            }
        }

        private void dtgProject_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //清理掉设计器里的东西
            ClearContain();
            if (dtgProject.SelectedItem != null)
            {
                this.SelectedModelFlowRelation = (FLOW_MODELFLOWRELATION_T)dtgProject.SelectedItem;
                client.GetKPIPointListByBusinessCodeAsync(SelectedModelFlowRelation.MODELFLOWRELATIONID);
            }
            //fClient.GetFlowDefineByFlowCodeAsync(((FLOW_MODELFLOWRELATION_T)dtgProject.SelectedItem).FLOW_FLOWDEFINE_T.FLOWCODE);
        }

        #region 流程图形界面

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 1s 冉龙军
            //cnsDesignerContainer.Height = svContainer.ActualHeight;
            //cnsDesignerContainer.Width = svContainer.ActualWidth;
            try
            {
                cnsDesignerContainer.DataContext = e.NewSize;
            }
            catch (Exception ex)
            {

            }
            // 1e
        }

        private void ScrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            if (FlowDefine != null)
                foreach (RuleLine rule in Rules)
                {
                    SetRuleLinePos(rule.Name);
                }
            //  Rules.Add(RuleLineS);
        }

        /// <summary>
        /// 设置状态位置
        /// </summary>
        /// <param name="Active"></param>
        public void SetPos(StateActive Active)//,string ActiveName,Point Pos)
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].StrStartActive == Active.Name || Rules[i].StrEndActive == Active.Name)
                    SetRuleLinePos(Rules[i].Name);
            }

        }

        /// <summary>
        /// 设置规则线位置
        /// </summary>
        /// <param name="RuleName"></param>
        void SetRuleLinePos(string RuleName)//,string ActiveName,Point Pos)
        {
            RuleLine class2 = FindName(RuleName) as RuleLine;
            StateActive StartActive = FindName(class2.StrStartActive) as StateActive;
            StateActive EndActive = FindName(class2.StrEndActive) as StateActive;

            Double X1 = StartActive.ActualHeight + Convert.ToDouble(StartActive.GetValue(Canvas.TopProperty).ToString());
            Double Y1 = StartActive.ActualWidth / 2 + Convert.ToDouble(StartActive.GetValue(Canvas.LeftProperty).ToString());

            Double X2 = Convert.ToDouble(EndActive.GetValue(Canvas.TopProperty).ToString()) - X1;
            Double Y2 = Convert.ToDouble(EndActive.GetValue(Canvas.LeftProperty).ToString()) - Y1 + EndActive.ActualWidth / 2;

            class2.SetValue(Canvas.TopProperty, X1);
            class2.SetValue(Canvas.LeftProperty, Y1);

            class2.line.X2 = Y2;
            class2.line.Y2 = X2;
            class2.SetAngleByPoint(new Point(Y2, X2));

        }

        private String backEscapeXMLChar(String str)
        {
            return str.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&amp;", "&");
        }
        /// <summary>
        /// 设置规则
        /// </summary>
        /// <param name="ActiveName"></param>
        public void RuleActiveSet(string RuleName)
        {

            RuleLine a = FindName(RuleName) as RuleLine;
            RuleActiveSet RuleActiveSetS = new RuleActiveSet();
            RuleActiveSetS.OptState = OptState.Update;
            // RuleActiveSetS.btnDele.Visibility = Visibility.Visible;
            RuleActiveSetS.RuleName = RuleName;
            RuleActiveSetS.BOSystem = BOSystem;
            RuleActiveSetS.BOObject = BOObject;
            RuleActiveSetS.StartStateName = a.StrStartActive;
            RuleActiveSetS.EndStateName = a.StrEndActive;
            RuleActiveSetS.SetRuleList(Actives);
            RuleActiveSetS.BOSystem = this.BOSystem;
            RuleActiveSetS.BOObject = this.BOObject;
            RuleActiveSetS.chkUseCondition.IsChecked = false;
            RuleActiveSetS.CoditionList.Clear();
            RuleActiveSetS.dgCodition.ItemsSource = null;
            RuleActiveSetS.dgCodition.ItemsSource = RuleActiveSetS.CoditionList;
            RuleActiveSetS.txtCompareValue.Text = "";
            RuleActiveSetS.cboOperate.SelectedIndex = 0;
            RuleActiveSetS.requestSystemBOAttributeList();
            RuleActiveSetS.Rule_OnClick += new EventHandler<Rule_OnClickEventArgs>(RuleActiveSetS_Rule_OnClick);

            if (a.ruleCoditions != null)
            {
                if (a.ruleCoditions.subCondition.Count > 0)
                {
                    foreach (CompareCondition cpItem in a.ruleCoditions.subCondition)
                    {
                        RuleActiveSetS.CoditionList.Add(cpItem);
                    }
                    RuleActiveSetS.chkUseCondition.IsChecked = true;
                    RuleActiveSetS.conditionPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    RuleActiveSetS.chkUseCondition.IsChecked = false;
                    RuleActiveSetS.conditionPanel.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                RuleActiveSetS.chkUseCondition.IsChecked = false;
                RuleActiveSetS.conditionPanel.Visibility = Visibility.Collapsed;
            }
            //  RuleActiveSetS.Show();
            EntityBrowser rule_browser = new EntityBrowser(RuleActiveSetS);
            rule_browser.Show<string>(DialogMode.ApplicationModal, Common.ParentLayoutRoot, "", (result) => { });
        }
        /// <summary>
        /// 关闭设置状态窗口
        /// </summary>
        public void RemoveStateActiveSet()
        {
            if (cnsDesignerContainer.Children.Contains(StateActiveSetS))
            {
                cnsDesignerContainer.Children.Remove(StateActiveSetS);

            }
        }
        private void AddRule()
        {
            RuleActiveSetS = new RuleActiveSet();
            //SetStatelist();
            // RuleActiveSet("");
            RuleActiveSetS.OptState = OptState.Add;
            //RuleActiveSetS.btnDele.Visibility = Visibility.Collapsed;
            RuleActiveSetS.SetRuleList(Actives);
            //RuleActiveSetS.chkUseCondition.IsChecked = false;
            RuleActiveSetS.conditionPanel.Visibility = Visibility.Collapsed;
            RuleActiveSetS.CoditionList.Clear();
            RuleActiveSetS.dgCodition.ItemsSource = null;
            RuleActiveSetS.dgCodition.ItemsSource = RuleActiveSetS.CoditionList;
            RuleActiveSetS.txtCompareValue.Text = "";
            RuleActiveSetS.cboOperate.SelectedIndex = 0;
            RuleActiveSetS.BOSystem = this.BOSystem;
            RuleActiveSetS.BOObject = this.BOObject;
            RuleActiveSetS.requestSystemBOAttributeList();

            //RuleActiveSetS = new RuleActiveSet();
            // SetStatelist();
            // RuleActiveSetS.Rule_OnClick +=new EventHandler<Rule_OnClickEventArgs>(RuleActiveSetS_Rule_OnClick);
            RuleActiveSetS.Rule_OnClick += new EventHandler<Rule_OnClickEventArgs>(RuleActiveSetS_Rule_OnClick);
            EntityBrowser rule_browser = new EntityBrowser(RuleActiveSetS);
            rule_browser.Width = 680;
            rule_browser.MinHeight = 600;
            rule_browser.Show<string>(DialogMode.ApplicationModal, Common.ParentLayoutRoot, "", (result) => { });
        }

        #region 加载数据，生成界面

        void SetLayout()
        {
            string Layout = this.FlowDefine;

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(Layout);
            string qq;
            string WFBOSystemName = string.Empty;
            string WFBOObjectName = string.Empty;
            qq = "";
            XmlReader XmlReader;
            StringReader bb = new StringReader(Layout);
            XmlReader = XmlReader.Create(bb);

            #region 遍历XML
            //while (XmlReader.Read())
            //{
            //    qq+="<li>节点类型：" + XmlReader.NodeType + "==<br>";
            //    switch (XmlReader.NodeType)
            //    {
            //        case XmlNodeType.XmlDeclaration:
            //            for (int i = 0; i < XmlReader.AttributeCount; i++)
            //            {
            //                XmlReader.MoveToAttribute(i);
            //                qq+="属性：" + XmlReader.Name + "=" + XmlReader.Value + "&nbsp;";
            //            }
            //            break;
            //        case XmlNodeType.Attribute:
            //            for (int i = 0; i < XmlReader.AttributeCount; i++)
            //            {
            //                XmlReader.MoveToAttribute(i);
            //                qq+="属性：" + XmlReader.Name + "=" + XmlReader.Value + "&nbsp;";
            //            }
            //            break;
            //        case XmlNodeType.CDATA:
            //            qq+="CDATA:" + XmlReader.Value + "&nbsp;";
            //            break;
            //        case XmlNodeType.Element:
            //            qq+="节点名称：" + XmlReader.LocalName + "<br>";
            //            for (int i = 0; i < XmlReader.AttributeCount; i++)
            //            {
            //                XmlReader.MoveToAttribute(i);
            //               qq+="属性：" + XmlReader.Name + "=" + XmlReader.Value + "&nbsp;";
            //            }
            //            break;
            //        case XmlNodeType.Comment:
            //            qq+="Comment:" + XmlReader.Value;
            //            break;
            //        case XmlNodeType.Whitespace:
            //            qq+="Whitespace:" + "&nbsp;";
            //            break;
            //        case XmlNodeType.ProcessingInstruction:
            //            qq+="ProcessingInstruction:" + XmlReader.Value;
            //            break;
            //        case XmlNodeType.Text:
            //            qq+="Text:" + XmlReader.Value;
            //            break;
            //    }
            // }

            #endregion

            XElement XElementS = XElement.Load(XmlReader);

            var flowSystem = from c in XElementS.Descendants("System")
                             select c;
            if (flowSystem != null)
            {
                foreach (var tmp in flowSystem)
                {
                    WFBOSystemName = tmp.Value;
                }
            }

            //BOSystem

            var a = from c in XElementS.Descendants("Activity")
                    select c;

            foreach (var tmp in a)
            {
                string stepname = tmp.Attribute("Name").Value;
                if (stepname != "StartFlow" && stepname != "EndFlow")
                {
                    StateActive Flow = new StateActive();
                    Flow.MinWidth = 60;
                    Flow.Name = stepname;
                    // 1s 冉龙军
                    //string tmpStateName = (StateList.Where(s => s.StateCode.ToString() == stepname).ToList().First().StateName);
                    string tmpStateName = tmp.Attribute("Remark").Value;
                    // 1e
                    string StateName = "";

                    for (int i = 0; i < tmpStateName.Length; i = i + 6)
                    {
                        if (tmpStateName.Length < i + 6)
                            StateName += (i == 0 ? tmpStateName : "\r\n" + tmpStateName.Substring(i));
                        else
                            StateName += (i == 0 ? tmpStateName.Substring(i, 6) : "\r\n" + tmpStateName.Substring(i, 6));
                    }

                    Flow.StateName.Text = StateName;// (StateList.Where(s => s.StateCode.ToString() == stepname).ToList().First().StateName); 

                    #region 加载KPI点的信息
                    if (KPIPointList != null && KPIPointList.Count != 0)
                        foreach (T_HR_KPIPOINT point in KPIPointList)
                        {
                            if (stepname.Equals(point.STEPID))
                                Flow.StateName.Text += "(KPI点)";
                        }
                    #endregion 加载KPI点的信息
                    Flow.SetValue(Canvas.TopProperty, Convert.ToDouble(tmp.Attribute("X").Value));
                    Flow.SetValue(Canvas.LeftProperty, Convert.ToDouble(tmp.Attribute("Y").Value));
                    if (!cnsDesignerContainer.Children.Contains(Flow))
                    {
                        // 1s 冉龙军 暂不处理错误
                        //cnsDesignerContainer.Children.Add(Flow);
                        try
                        {
                            cnsDesignerContainer.Children.Add(Flow);
                        }
                        catch (Exception ex)
                        {
                        }
                        // 1e
                        Flow.Container = this;
                        Actives.Add(Flow);
                    }
                }
            }

            var b = from c in XElementS.Descendants("Rule")
                    select c;

            foreach (var tmp in b)
            {
                RuleLine RuleLineS = new RuleLine();
                RuleLineS.Name = tmp.Attribute("Name").Value;

                RuleLineS.StrStartActive = tmp.Attribute("StrStartActive").Value;
                RuleLineS.StrEndActive = tmp.Attribute("StrEndActive").Value;
                RuleLineS.Container = this;

                if (tmp.Element("Conditions") != null)
                {
                    RuleConditions newRuleCondition = new RuleConditions();
                    newRuleCondition.Name = tmp.Element("Conditions").Attribute("Name").Value;
                    newRuleCondition.ConditionObject = tmp.Element("Conditions").Attribute("Object").Value;
                    newRuleCondition.CodiCombMode = tmp.Element("Conditions").Attribute("CodiCombMode").Value;

                    if (!string.IsNullOrEmpty(newRuleCondition.ConditionObject))
                    {
                        WFBOObjectName = newRuleCondition.ConditionObject;
                    }

                    if (tmp.Element("Conditions").Elements("Condition").Count() > 0)
                    {
                        var e = from f in tmp.Element("Conditions").Descendants("Condition")
                                select f;

                        foreach (var tmp2 in e)
                        {
                            CompareCondition newCD = new CompareCondition();
                            newCD.Name = tmp2.Attribute("Name").Value;
                            newCD.Description = tmp2.Attribute("Description").Value;
                            newCD.CompAttr = tmp2.Attribute("CompAttr").Value;
                            newCD.Operate = backEscapeXMLChar(tmp2.Attribute("Operate").Value);
                            newCD.DataType = tmp2.Attribute("DataType").Value;
                            newCD.CompareValue = tmp2.Attribute("CompareValue").Value;
                            newRuleCondition.subCondition.Add(newCD);
                        }
                    }

                    RuleLineS.ruleCoditions = newRuleCondition;
                }

                if (!cnsDesignerContainer.Children.Contains(RuleLineS))
                {
                    // 1s 冉龙军 暂不处理错误
                    //cnsDesignerContainer.Children.Add(RuleLineS);
                    try
                    {
                        cnsDesignerContainer.Children.Add(RuleLineS);
                    }
                    catch (Exception ex)
                    {
                    }
                    // 1e
                    SetRuleLinePos(RuleLineS.Name);
                    Rules.Add(RuleLineS);
                    // a.ActivityChanged += new ActivityChangeDelegate(OnActivityChanged);
                }
                // MessageBox.Show(tmp.Attribute("Name").Value);
            }

        }

        #endregion 加载数据，生成界面

        #region 规则设置
        void rule_browser_ReloadDataEvent(RuleActiveSet RuleActiveSetS)
        {
            if (d_RuleActiveSet == true)
            {
                #region 添加新规则
                if (RuleActiveSetS.OptState == OptState.Add && RuleActiveSetS.StartStateName != "" && RuleActiveSetS.EndStateName != "")
                {

                    //检测是否已添加此状态
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        if (Rules[i].StrStartActive == RuleActiveSetS.StartStateName && Rules[i].StrEndActive == RuleActiveSetS.EndStateName)
                        {

                            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("M00008"), Utility.GetResourceStr("CONFIRMBUTTON"));
                            //MessageBox.Show("此规则已添加，不能添加相同的规则!");
                            return;
                        }
                    }

                    if ((string.IsNullOrEmpty(BOSystem.Name) || string.IsNullOrEmpty(BOObject.Name)) && !string.IsNullOrEmpty(RuleActiveSetS.BOSystem.Name) && !string.IsNullOrEmpty(RuleActiveSetS.BOObject.Name))
                    {
                        BOSystem = RuleActiveSetS.BOSystem;
                        BOObject = RuleActiveSetS.BOObject;
                        //this.tbSystemAndBusinessObject.Text = 
                    }

                    RuleLine a = new RuleLine();

                    a.Name = System.Guid.NewGuid().ToString();
                    a.StrStartActive = RuleActiveSetS.StartStateName;
                    a.StrEndActive = RuleActiveSetS.EndStateName;
                    a.Container = this;
                    a.ruleCoditions = new RuleConditions();
                    a.ruleCoditions.subCondition.Clear();

                    if (RuleActiveSetS.CoditionList.Count > 0)
                    {
                        a.ruleCoditions.ConditionObject = BOObject.Name;

                        foreach (CompareCondition cpItem in RuleActiveSetS.CoditionList)
                        {
                            a.ruleCoditions.subCondition.Add(cpItem);
                        }
                    }



                    // StartFlow.GetValue(Canvas.TopProperty) ;
                    //Double X1 = 60 + Convert.ToDouble((FindName(a.StrStartActive) as StateActive).GetValue(Canvas.TopProperty).ToString());
                    //Double Y1 = 50 + Convert.ToDouble((FindName(a.StrStartActive) as StateActive).GetValue(Canvas.LeftProperty).ToString());
                    //a.SetValue(Canvas.TopProperty, X1);//(double)110);
                    //a.SetValue(Canvas.LeftProperty, Y1);//(double)450);
                    if (!cnsDesignerContainer.Children.Contains(a))
                    {
                        cnsDesignerContainer.Children.Add(a);

                        SetRuleLinePos(a.Name);
                        Rules.Add(a);
                        // a.ActivityChanged += new ActivityChangeDelegate(OnActivityChanged);
                    }
                }


                #endregion

                #region 修改规则
                else if (RuleActiveSetS.OptState == OptState.Update && RuleActiveSetS.StartStateName != "" && RuleActiveSetS.EndStateName != "")
                {

                    if ((string.IsNullOrEmpty(BOSystem.Name) || string.IsNullOrEmpty(BOObject.Name)) && !string.IsNullOrEmpty(RuleActiveSetS.BOSystem.Name) && !string.IsNullOrEmpty(RuleActiveSetS.BOObject.Name))
                    {
                        BOSystem = RuleActiveSetS.BOSystem;
                        BOObject = RuleActiveSetS.BOObject;
                    }

                    RuleLine a = FindName(RuleActiveSetS.RuleName) as RuleLine;
                    a.StrStartActive = RuleActiveSetS.StartStateName;
                    a.StrEndActive = RuleActiveSetS.EndStateName;

                    if (a.ruleCoditions == null)
                        a.ruleCoditions = new RuleConditions();
                    a.ruleCoditions.subCondition.Clear();

                    if (RuleActiveSetS.CoditionList.Count > 0)
                    {
                        a.ruleCoditions.ConditionObject = BOObject.Name;
                        foreach (CompareCondition cpItem in RuleActiveSetS.CoditionList)
                        {
                            a.ruleCoditions.subCondition.Add(cpItem);
                        }
                    }

                    SetRuleLinePos(a.Name);
                }

                #endregion

                #region 删除规则
                else if (RuleActiveSetS.OptState == OptState.Delete)
                {
                    RuleLine a = FindName(RuleActiveSetS.RuleName) as RuleLine;
                    if (a == null)
                        return;
                    if (a.ruleCoditions != null)
                        a.ruleCoditions.subCondition.Clear();
                    Rules.Remove(a);
                    cnsDesignerContainer.Children.Remove(a);
                }
                #endregion
            }
        }
        #endregion 规则设置

        private void ClearContain()
        {
            cnsDesignerContainer.Children.Clear();
            Actives.Clear();
            Rules.Clear();
            BOSystem.Name = "";
            BOSystem.Description = "";
            BOObject.Name = "";
            BOObject.Description = "";

            StateActive StartFlow = new StateActive();
            StartFlow.Name = "StartFlow";
            StartFlow.bdrName.Width = StartFlow.bdrName.Height;
            StartFlow.bdrName2.Width = StartFlow.bdrName2.Height;
            ToolTipService.SetToolTip(StartFlow, "开始流程");

            StartFlow.StateName.Text = "开始";
            StartFlow.bdrName.Background = new SolidColorBrush(Colors.Orange);
            StartFlow.bdrName.CornerRadius = new CornerRadius(40);

            StartFlow.bdrName2.CornerRadius = new CornerRadius(40);
            StartFlow.SetValue(Canvas.TopProperty, (double)50);
            StartFlow.SetValue(Canvas.LeftProperty, (double)400);
            if (!cnsDesignerContainer.Children.Contains(StartFlow))
            {
                cnsDesignerContainer.Children.Add(StartFlow);

                StartFlow.Container = this;
                Actives.Add(StartFlow);
            }

            StateActive EndFlow = new StateActive();
            EndFlow.Name = "EndFlow";
            EndFlow.bdrName.Width = StartFlow.bdrName.Width; ;
            EndFlow.bdrName2.Height = StartFlow.bdrName2.Height; ;
            ToolTipService.SetToolTip(EndFlow, "结束流程");
            EndFlow.StateName.Text = "结束";

            EndFlow.StateName.SetValue(Canvas.LeftProperty, (double)8);

            EndFlow.bdrName.Background = new SolidColorBrush(Colors.Red);
            EndFlow.bdrName.CornerRadius = new CornerRadius(40);

            EndFlow.bdrName2.CornerRadius = new CornerRadius(40);

            EndFlow.SetValue(Canvas.TopProperty, (double)400);
            EndFlow.SetValue(Canvas.LeftProperty, (double)400);

            if (!cnsDesignerContainer.Children.Contains(EndFlow))
            {
                cnsDesignerContainer.Children.Add(EndFlow);

                EndFlow.Container = this;
                Actives.Add(EndFlow);
            }
        }

        #endregion 流程图形界面

        #region IContainer 成员


        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddState()
        {
            StateActiveSetS.optFlag = true;
            //StateActiveSetS.StateList.Clear();

            StateActiveSetS = new StateActiveSet();
            SetStatelist();
            StateActiveSetS.StateActiveSet_Click += new EventHandler<OnClickEventArgs>(StateActiveSetS_StateActiveSet_Click);

            EntityBrowser State_browser = new EntityBrowser(StateActiveSetS);
            State_browser.Width = 350;
            State_browser.Height = 210;
            //State_browser.Show<string>(DialogMode.ApplicationModal, Common.ParentLayoutRoot, "", (result) => { }, false);
        }

        public void AddStateActive()
        {

            if (StateActiveSetS.ActiveName != "")
            {
                StateActive Flow = new StateActive();
                Flow.Name = StateActiveSetS.ActiveName;
                Flow.StateName.Text = StateActiveSetS.ActiveName;
                Flow.SetValue(Canvas.TopProperty, (double)100);
                Flow.SetValue(Canvas.LeftProperty, (double)100);
                if (!cnsDesignerContainer.Children.Contains(Flow))
                {
                    cnsDesignerContainer.Children.Add(Flow);
                    Flow.Container = this;
                    // a.ActivityChanged += new ActivityChangeDelegate(OnActivityChanged);
                }
            }
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="ActiveName"></param>
        public void StateActiveSet(string ActiveName)
        {
            //StateActiveSetS = new StateActiveSet();
            //SetStatelist();
            //StateActiveSetS.StateActiveSet_Click += new EventHandler<OnClickEventArgs>(StateActiveSetS_StateActiveSet_Click);
            //StateActiveSetS.optFlag = false;
            //StateActiveSetS.ActiveName = ActiveName;
            //StateActiveSetS.GotFocused(null, null);

            KPIPointInfo kpiPointInfo = new KPIPointInfo(FormTypes.Edit, KPIPointList, ActiveName, SelectedModelFlowRelation);
            kpiPointInfo.StateList = this.StateList;
            EntityBrowser State_browser = new EntityBrowser(kpiPointInfo);

            State_browser.MinWidth = 680;
            State_browser.ReloadDataEvent += new EntityBrowser.refreshGridView(RefreshStateActive);
            State_browser.Show<string>(DialogMode.ApplicationModal, Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 刷新控件
        /// </summary>
        public void RefreshStateActive()
        {
            ClearContain();
            client.GetKPIPointListByBusinessCodeAsync(SelectedModelFlowRelation.MODELFLOWRELATIONID);            
        }

        void State_browser_ReloadDataEvent()
        {
            if (d_StateActiveSetS == true && StateActiveSetS.ActiveName != "")
            {
                if (StateActiveSetS.optFlag == false)
                {
                    StateActive a = FindName(StateActiveSetS.ActiveName) as StateActive;
                    Actives.Remove(a);
                    cnsDesignerContainer.Children.Remove(a);

                    RuleLine b;
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        if (Rules[i].StrStartActive == StateActiveSetS.ActiveName || Rules[i].StrEndActive == StateActiveSetS.ActiveName)
                        {
                            b = FindName(Rules[i].Name) as RuleLine;
                            Rules.Remove(b);
                            cnsDesignerContainer.Children.Remove(b);
                            --i;
                        }
                    }
                }
                else
                {
                    //检测是否已添加此状态
                    for (int i = 0; i < Actives.Count; i++)
                    {
                        if (Actives[i].Name == StateActiveSetS.ActiveName)
                        {
                            // MessageBox.Show("此状态已添加，不能添加相同的状态!");
                            //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("M00001"), Utility.GetResourceStr("CONFIRMBUTTON"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("M00001"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                            return;
                        }
                    }

                    StateActive Flow = new StateActive();
                    Flow.Name = StateActiveSetS.ActiveName;

                    // 1s 冉龙军
                    //string tmpStateName = (StateList.Where(s => s.StateCode.ToString() == StateActiveSetS.ActiveName).ToList().First().StateName);
                    string tmpStateName = StateActiveSetS.Remark;
                    // 1e
                    string StateName = "";

                    for (int i = 0; i < tmpStateName.Length; i = i + 6)
                    {
                        if (tmpStateName.Length < i + 6)
                            StateName += (i == 0 ? tmpStateName : "\r\n" + tmpStateName.Substring(i));
                        else
                            StateName += (i == 0 ? tmpStateName.Substring(i, 6) : "\r\n" + tmpStateName.Substring(i, 6));
                    }

                    // StateName += (StateName == "" ? tmp : "\r\n" + tmp.Substring(StateName.Length));

                    Flow.StateName.Text = StateName;// (StateList.Where(s => s.StateCode.ToString() == StateActiveSetS.ActiveName).ToList().First().StateName);
                    Flow.SetValue(Canvas.TopProperty, (double)100);
                    Flow.SetValue(Canvas.LeftProperty, (double)100);
                    if (!cnsDesignerContainer.Children.Contains(Flow))
                    {
                        cnsDesignerContainer.Children.Add(Flow);
                        Flow.Container = this;
                        Actives.Add(Flow);
                        // a.ActivityChanged += new ActivityChangeDelegate(OnActivityChanged);
                    }
                }
            }
        }

        //设置当前面板序列
        public void SetStatelist()
        {
            StateActiveSetS.StateList = StateList;
            StateActiveSetS.cboInfo.ItemsSource = StateList;
            StateActiveSetS.cboInfo.SelectedIndex = 0;
        }



        void RuleActiveSetS_Rule_OnClick(object sender, Rule_OnClickEventArgs e)
        {
            d_RuleActiveSet = true;
            RuleActiveSetS.Rule_OnClick -= new EventHandler<Rule_OnClickEventArgs>(RuleActiveSetS_Rule_OnClick);
            rule_browser_ReloadDataEvent(sender as RuleActiveSet);
        }

        void StateActiveSetS_StateActiveSet_Click(object sender, OnClickEventArgs e)
        {
            d_StateActiveSetS = true;
            StateActiveSetS.StateActiveSet_Click -= new EventHandler<OnClickEventArgs>(StateActiveSetS_StateActiveSet_Click);
            State_browser_ReloadDataEvent();
        }

        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
