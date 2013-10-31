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
using System.Reflection;
using System.Collections;

//using SMT.SaaS.Permission.UI.OrganizationWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMTWindow = System.Windows.Controls;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.Permission.UI
{
    public partial class MenuLookupForm : SMTWindow.Window
    {
        private List<T_SYS_ENTITYMENU> allMenu;

        public object SelectedObj
        {
            get;
            set;
        }
        public OrgTreeItemTypes SelectedObjType { get; set; }

        public event EventHandler SelectedClick;

        public string SystemType = "";

        public MenuLookupForm(string sysType)
        {
            SystemType = sysType;

            InitializeComponent();
            InitParas();
        }

        PermissionServiceClient client = new PermissionServiceClient();
        private void InitParas()
        {
            //初始化控件的状态            

            //client.GetSysMenuByTypeToLookUpCompleted += new EventHandler<GetSysMenuByTypeToLookUpCompletedEventArgs>(client_GetSysMenuByTypeToLookUpCompleted);
            client.GetSysMenuByTypeToLookUpForFbAdminCompleted += new EventHandler<GetSysMenuByTypeToLookUpForFbAdminCompletedEventArgs>(client_GetSysMenuByTypeToLookUpForFbAdminCompleted);
            //client.GetSysMenuByTypeTOFbAdminAsync(SystemType, "",Common.CurrentLoginUserInfo.EmployeeID);
            client.GetSysMenuByTypeToLookUpForFbAdminAsync(SystemType, "", Common.CurrentLoginUserInfo.EmployeeID);
            
        }

        void client_GetSysMenuByTypeToLookUpForFbAdminCompleted(object sender, GetSysMenuByTypeToLookUpForFbAdminCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allMenu = e.Result.ToList();
            }
            BindTree();
        }

        

        void client_GetSysMenuByTypeToLookUpCompleted(object sender, GetSysMenuByTypeToLookUpCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allMenu = e.Result.ToList();
            }
            BindTree();
        }

       

        private void BindTree()
        {
            if (allMenu != null)
            {
                var menulist = from m in allMenu
                               where m.T_SYS_ENTITYMENU2Reference.EntityKey == null
                               //where m.T_SYS_ENTITYMENU2Reference.EntityKey.EntityKeyValues.ToString()==""
                               select m;
                
                foreach (T_SYS_ENTITYMENU menu in menulist)
                {
                    TreeViewItem childItem = new TreeViewItem();
                    childItem.Header = menu.MENUNAME;
                    childItem.DataContext = menu;

                    AddChildItems(childItem, menu);

                    treeMenu.Items.Add(childItem);

                }
            }
        }
        private void AddChildItems(TreeViewItem treeitem, T_SYS_ENTITYMENU menu)
        {
            var menulist = from m in allMenu
                           where m.T_SYS_ENTITYMENU2Reference.EntityKey != null
                           && m.T_SYS_ENTITYMENU2Reference.EntityKey.EntityKeyValues[0].Value.ToString() == menu.ENTITYMENUID
                           select m;

            foreach (var tmpMenu in menulist)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = tmpMenu.MENUNAME;
                childItem.DataContext = tmpMenu;

                AddChildItems(childItem, tmpMenu);

                treeitem.Items.Add(childItem);
            }
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedObj == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "所选不能为空！", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            //this.DialogResult = true;
            this.Close();

            if (SelectedClick != null)
                SelectedClick(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            this.Close();
        }

        private void lookUpTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeMenu.SelectedItem.GetType() != typeof(TreeViewItem))
                return;

            TreeViewItem item = (TreeViewItem)treeMenu.SelectedItem;

            if (item == null || item.DataContext == null)
                return;

            SelectedObj = item.DataContext;
        }
    }
}

