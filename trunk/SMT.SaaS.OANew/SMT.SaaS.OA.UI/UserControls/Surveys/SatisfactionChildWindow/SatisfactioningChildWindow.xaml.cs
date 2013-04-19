//****满意度参与调查页面***
//负责人:lezy
//创建时间:2011-6-7
//完成时间：2011-6-30
//**************************

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
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SatisfactioningChildWindow : BaseForm, IClient, IEntityEditor
    {
        #region 全局变量定义

        #endregion

        #region 构造函数
        public SatisfactioningChildWindow(string appId)
        {
            InitializeComponent();
        }
        #endregion

        #region 事件注册
        #endregion

        #region 事件处理程序
        #endregion

        #region XAML事件处理程序
        #endregion

        #region 其他函数
        void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        void SaveSatisfactioning()
        {

        }
        #endregion

        #region 接口实现

        #region IClient资源回收
        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEntityEditor窗体控制

        public string GetTitle()
        {
            return Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATION");
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
                    SaveSatisfactioning();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "1":
                    SaveSatisfactioning();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> navigateItems = new List<NavigateItem>() { new NavigateItem { Title = Utility.GetResourceStr("InfoDetail"), Tooltip = Utility.GetResourceStr("InfoDetail") } };
            return navigateItems;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> tooltbalitems = new List<ToolbarItem>()
            {
                new ToolbarItem
           {DisplayType=ToolbarItemDisplayTypes.Image,Title=Utility.GetResourceStr("SEARCH"),Key="1",ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/(09,24).png"},
           new ToolbarItem
           {DisplayType=ToolbarItemDisplayTypes.Image,Title=Utility.GetResourceStr("SAVEANDCLOSE"),Key="0",ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"}
            };
            return tooltbalitems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
    }
        #endregion

        #endregion



}
