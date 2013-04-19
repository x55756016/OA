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
using System.IO.IsolatedStorage;

namespace SMT.HRM.UI.Form.Setting
{
    public partial class SetPageColor : BaseForm, IEntityEditor
    {

        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        public event EventHandler ThemeChanged;

        //private string SaveType = "0";//0:未关闭1：关闭
        public SetPageColor()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SetPageColor_Loaded);
        }

        private void ChangeTheme(string themeName)
        {
           
        }



        void SetPageColor_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "样式设定";
        }

        public string GetStatus()
        {
            return "编辑中";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    SaveAndClose();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息A",
                Tooltip = "详细信息A"
            };
            items.Add(item);
            item = new NavigateItem
            {
                Title = "详细信息B",
                Tooltip = "详细信息B"
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
                Key = "0",
                Title = "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };


            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        private string ColorCode = "";
        /// <summary>
        /// 保存并关闭新样式选择
        /// </summary>
        public void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.All);
        }

        public void Save()
        {

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
           
            
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            
           
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
           
        }

        public void ReLoad()
        {
            RefreshUI(RefreshedTypes.Close);
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Children.Clear();
            currentApp.rootGrid.Children.Add(new MainPage());
           
        }
    }
}
