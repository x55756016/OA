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
using SMT.Workflow.Platform.Designer.Common;
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.Workflow.Platform.Designer.Class;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.UControls;
using SMT.Workflow.Platform.Designer.Utils;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class EngineMain : UserControl
    {
        public Flow_ModelDefineClient client = null; //流程模块WCF定义      
        //private ObservableCollection<AppModel> appModel = new ObservableCollection<AppModel>();//XML模块代码
        //private ObservableCollection<AppSystem> appSystem = new ObservableCollection<AppSystem>();//XML系统代码
        //private ObservableCollection<AppSystem> SystemCode = new ObservableCollection<AppSystem>();//XML系统代码
        public EngineMain()
        {
          
            InitializeComponent();
            client = new Flow_ModelDefineClient(); //实例化流程模块          
        }

        public void EngineInit()
        {              
            RegisterCompletedEvent();
            client.GetSystemCodeModelCodeListAsync();        
        }
        /// <summary>
        /// 注册WCF完成事件
        /// </summary>
        private void RegisterCompletedEvent()
        {
            client.GetSystemCodeModelCodeListCompleted += new EventHandler<GetSystemCodeModelCodeListCompletedEventArgs>(client_GetSystemCodeModelCodeListCompleted);
            ////加载所有系统
            //clientXml.ListSystemCompleted += new EventHandler<ListSystemCompletedEventArgs>(clientXml_ListSystemCompleted);
            ////加载子系统xml
            //clientXml.ListModelCompleted += new EventHandler<ListModelCompletedEventArgs>(clientXml_ListModelCompleted);
        }

        void client_GetSystemCodeModelCodeListCompleted(object sender, GetSystemCodeModelCodeListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {                
                    ListBindComboBox(e.Result);                   
                }
            }
            MessageList.DefaultMessgeInit();
            RuleList.MessageRuleInit();
            triggerList.TriggerInit();
        }

        public void ListBindComboBox(ObservableCollection<FLOW_MODELDEFINE_T> list)
        {
            ObservableCollection<FLOW_MODELDEFINE_T> systemCode = new ObservableCollection<FLOW_MODELDEFINE_T>();
            var systemcode = list.GroupBy(s => s.SYSTEMCODE);
            foreach (var item in systemcode)
            {
                systemCode.Add(item.FirstOrDefault() as FLOW_MODELDEFINE_T);
            }
            list.Insert(0, new FLOW_MODELDEFINE_T() { MODELCODE = "0", DESCRIPTION = "请选择......" });
            MessageList.cbModelCode.ItemsSource = list;
            MessageList.cbModelCode.SelectedIndex = 0;
            

            systemCode.Insert(0, new FLOW_MODELDEFINE_T() {  SYSTEMCODE = "0", SYSTEMNAME = "请选择......" });
            MessageList.cbSystemCode.ItemsSource = systemCode;
            MessageList.cbSystemCode.SelectedIndex = 0;
            MessageList.appModel = list;
            MessageList.appSystem = systemCode;


            RuleList.cbSystemCode.ItemsSource = systemCode;            
            RuleList.cbModelCode.ItemsSource = list;
            RuleList.cbSystemCode.SelectedIndex = 0;
            RuleList.cbModelCode.SelectedIndex = 0;
            RuleList.appSystem = systemCode;    
            RuleList.appModel = list;


            triggerList.cbSystemCode.ItemsSource = systemCode;
            triggerList.cbModelCode.ItemsSource = list;
            triggerList.cbSystemCode.SelectedIndex = 0;
            triggerList.cbModelCode.SelectedIndex = 0;
            triggerList.appSystem = systemCode;
            triggerList.appModel = list;
        }

        #region 系统代码 模块代码完成事件
        void clientXml_ListModelCompleted(object sender, ListModelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    //appModel = e.Result;
                    //e.Result.Insert(0, new AppModel() { Name = "0", Description = "请选择......" });
                    //MessageList.cbModelCode.ItemsSource = e.Result;//XML模块代码
                    //MessageList.cbModelCode.SelectedIndex = 0;
                    //RuleList.cbModelCode.ItemsSource = e.Result;
                    //RuleList.cbModelCode.SelectedIndex = 0;
                    //triggerList.cbModelCode.ItemsSource = e.Result;
                    //triggerList.cbModelCode.SelectedIndex = 0;

                    //MessageList.appModel = e.Result;//龙康才新增      
                    //RuleList.appModel = e.Result;//龙康才新增    
                    //triggerList.appModel = e.Result;//龙康才新增    
                    //#region 开始连动
                    //MessageList.cbSystemCode.SelectedIndex = 0;
                    //RuleList.cbSystemCode.SelectedIndex = 0;
                    //triggerList.cbSystemCode.SelectedIndex = 0;
                    #endregion
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "系统模块加载错误！", "确定");
            }
        }

        void clientXml_ListSystemCompleted(object sender, ListSystemCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ObservableCollection<string> xmlurl = new ObservableCollection<string>(); //子系统XML路径
                    FlowSystemModel.ListAppSystem = e.Result == null ? null : e.Result.ToList();
                    //appSystem.Clear();
                    //foreach (var item in FlowXml.ListAppSystem)
                    //{
                    //    appSystem.Add(item);
                    //    xmlurl.Add(item.ObjectFolder);
                    //}
                    //if (xmlurl.Count > 0)
                    //{
                    //    //clientXml.ListModelAsync(xmlurl);
                    //}
                    //SystemCode = appSystem;
                    //appSystem.Insert(0, new AppSystem() { Name = "0", Description = "请选择......" });
                    //MessageList.cbSystemCode.ItemsSource = appSystem;
                    //RuleList.cbSystemCode.ItemsSource = appSystem;
                    //triggerList.cbSystemCode.ItemsSource = appSystem;


                    //MessageList.appSystem = appSystem;//龙康才新增 
                    //RuleList.appSystem = appSystem;//龙康才新增         
                    //triggerList.appSystem = appSystem;//龙康才新增         
                    //MessageList.BindDefaultGridData();//首次进入时加载
                    //RuleList.BindRuleData();//首次进入时加载
                    //triggerList.BindTriggerData();//首次进入时加载
                 
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "系统代码加载错误！", "确定");
            }
        }
     

   
    }
}
