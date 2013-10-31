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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.UseractWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Form
{
    /// <summary>
    /// 操作人:杨祥红
    /// </summary>
    public partial class SysUseractLogForm : UserControl, IEntityEditor
    {

        private string saveType = "0";       //保存方式 0:添加 1:添加并关闭
        #region IEntityEditor 成员

        public string GetTitle()
        {
            return SMT.SaaS.Globalization.Localization.GetString("SYSTEMMENU");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    AddToClose();
                    break;
                case "1":
                    AddToClose();
                    this.Close();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);
            return items;
        }
        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion
        //保存调用方法
        public void AddToClose()
        {
            if (FormTypes.New == this.FormType)
            {
                SysMenu.CLIENTBROWSER = txtClientBrowser.Text.Trim();
                SysMenu.CLIENTHOSTADDRESS = txtClientHostAddress.Text.Trim();
                SysMenu.CLIENTNETRUNTIME = txtClientNetRuntime.Text.Trim();
                SysMenu.CLIENTOS = txtClientOS.Text.Trim();
                SysMenu.CLIENTOSLANGUAGE = txtClientOSLanguage.Text.Trim();
                SysMenu.LOGCONTEXT = txtLogContext.Text.Trim();
                SysMenu.CREATEDATE = System.DateTime.Now;
                ServiceClient.AddUseractLogAsync(this.SysMenu);
            }
            else
            {
                SysMenu.CLIENTBROWSER = txtClientBrowser.Text.Trim();
                SysMenu.CLIENTHOSTADDRESS = txtClientHostAddress.Text.Trim();
                SysMenu.CLIENTNETRUNTIME = txtClientNetRuntime.Text.Trim();
                SysMenu.CLIENTOS = txtClientOS.Text.Trim();
                SysMenu.CLIENTOSLANGUAGE = txtClientOSLanguage.Text.Trim();
                SysMenu.LOGCONTEXT = txtLogContext.Text.Trim();
                SysMenu.CREATEDATE = System.DateTime.Now;
                ServiceClient.UpdateUseractLogAsync(this.SysMenu);
            }
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
            
        }
        public void Close()
        {
            saveType = "1";
            RefreshUI(RefreshedTypes.Close);
        }
        private T_SYS_USERACTLOG sysMenu;
         public T_SYS_USERACTLOG SysMenu
        {
            get { return sysMenu; }
            set
            {
                sysMenu = value;
                this.DataContext = value;
            }
        }
        private FormTypes formType;
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        protected UseractLogServiceClient ServiceClient;
        //添加的构造方法
        public SysUseractLogForm(FormTypes type)
        {
            InitializeComponent();
            ////TODO:加操作用户信息
            FormType = type;
            InitParas("");
            
        }
        //修改的构造方法
        public SysUseractLogForm(FormTypes type, string menuID)
        {
            InitializeComponent();  
            FormType = type;
            InitParas(menuID);
        }
        //添加OR修改初始化
        private void InitParas(string menuID)
        {

            ServiceClient = new UseractLogServiceClient();
            ServiceClient.AddUseractLogCompleted += new EventHandler<AddUseractLogCompletedEventArgs>(ServiceClient_AddUseractLogCompleted);
            ServiceClient.UpdateUseractLogCompleted += new EventHandler<UpdateUseractLogCompletedEventArgs>(ServiceClient_UpdateUseractLogCompleted);
            ServiceClient.GetSysUseractLogByIDCompleted += new EventHandler<GetSysUseractLogByIDCompletedEventArgs>(ServiceClient_GetSysUseractLogByIDCompleted);
            if (FormType == FormTypes.New)
            {
                SysMenu = new T_SYS_USERACTLOG();
                SysMenu.LOGID = Guid.NewGuid().ToString();
            }
            if (!string.IsNullOrEmpty(menuID))
            {
                ServiceClient.GetSysUseractLogByIDAsync(menuID);
            }

        }
        //修改时页面值加载
        void ServiceClient_GetSysUseractLogByIDCompleted(object sender, GetSysUseractLogByIDCompletedEventArgs e)
        {
            if(!e.Cancelled)
            {
                if(e.Result!=null)
                {
                    SysMenu = (T_SYS_USERACTLOG)e.Result;
                    txtClientBrowser.Text = SysMenu.CLIENTBROWSER;
                    txtClientHostAddress.Text = SysMenu.CLIENTHOSTADDRESS;
                    txtClientNetRuntime.Text = SysMenu.CLIENTNETRUNTIME;
                    txtClientOS.Text = SysMenu.CLIENTOS;
                    txtClientOSLanguage.Text = SysMenu.CLIENTOSLANGUAGE;
                    txtLogContext.Text = SysMenu.LOGCONTEXT;

                }
            }
        }
        //注册的修改用户事件
        void ServiceClient_UpdateUseractLogCompleted(object sender, UpdateUseractLogCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        //注册的添加日志事件
        void ServiceClient_AddUseractLogCompleted(object sender, AddUseractLogCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ADDSUCCESSEDMENU"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }      
    }
}
