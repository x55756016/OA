using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ChooseLicenseForm : BaseForm,IClient,IEntityEditor
    {
        public List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dict = null;

        #region 初始化
        public ChooseLicenseForm()
        {
            dict = new List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>();
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ChooseLicenseForm_Loaded);
        }

        void ChooseLicenseForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        private void InitData()
        {
            List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
            if (dicts == null)
            {
                dgLicense.ItemsSource = null;
                return;
            }
            else
            {
                var objs = from a in dicts
                           where a.DICTIONCATEGORY == "LICENSE"
                           select a;
                if (objs.Count() > 0)
                {
                    dgLicense.ItemsSource = objs.ToList();
                }
                else
                {
                    dgLicense.ItemsSource = null;
                }
            }
        }
        #endregion


        #region 自定义函数
        private void Save()
        {
            if (dgLicense.ItemsSource != null)
            {
                foreach (object obj in dgLicense.ItemsSource)
                {
                    if (dgLicense.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = dgLicense.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (ckbSelect.IsChecked == true)
                        {
                            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY tmp = ckbSelect.DataContext as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
                            dict.Add(tmp);
                        }
                    }
                }
            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
           
            return Utility.GetResourceStr("ADDTITLE", "LICENSELENDING");
            //return "";
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
                    Save();
                    break;
                case "1":                    
                    Close();
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return CreateFormNewButton();
        }

        private List<ToolbarItem> CreateFormNewButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("CONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);
           
            return items;
        }

        private List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
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

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            //throw new System.NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new System.NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
