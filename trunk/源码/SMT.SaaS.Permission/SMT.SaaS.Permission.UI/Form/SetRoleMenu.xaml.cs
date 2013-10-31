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
using SMT.Saas.Tools.PermissionWS;


using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Permission.UI.Form;


using System.Collections.ObjectModel;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SetRoleMenu : BaseForm
    {

        private T_SYS_ROLE tmpRole = new T_SYS_ROLE();
        private T_SYS_ROLE role;

        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        private ObservableCollection<T_SYS_USERROLE> ViewInfosList = new ObservableCollection<T_SYS_USERROLE>();
        private ObservableCollection<T_SYS_ROLE> ViewRoleList = new ObservableCollection<T_SYS_ROLE>();
        private List<T_SYS_ENTITYMENU> ViewMenuList ;
        private List<T_SYS_ENTITYMENU> ChildViewMenuList = new List<T_SYS_ENTITYMENU>() ;
        protected PermissionServiceClient ServiceClient;
        

        public SetRoleMenu(T_SYS_ROLE RoleObj)
        {
            tmpRole = RoleObj;
            //this.tblTitle.Text = tmpUser.USERNAME.ToString() + "授权";

            InitializeComponent();
            InitControlEvent();
            LoadData();
            //tvRoleMenu.ItemContainerStyle = this.Resources["RedItemStyle"] as Style;
        }

        
        

        private void InitControlEvent()
        {
            ServiceClient = new PermissionServiceClient();                        
            ServiceClient.GetSysMenuByTypeCompleted +=new EventHandler<GetSysMenuByTypeCompletedEventArgs>(ServiceClient_GetSysMenuByTypeCompleted);
            //ServiceClient.GetSysMenuInfosListByParentIDCompleted += new EventHandler<GetSysMenuInfosListByParentIDCompletedEventArgs>(ServiceClient_GetSysMenuInfosListByParentIDCompleted);
            //SysRoleClient.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(SysRoleClient_GetSysRoleInfosCompleted);
            //SysRoleClient.UserRoleBatchAddInfosCompleted += new EventHandler<UserRoleBatchAddInfosCompletedEventArgs>(SysRoleClient_UserRoleBatchAddInfosCompleted);
        }

        void ServiceClient_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    //List<T_SYS_ENTITYMENU> menulist = e.Result.ToList();
                    ViewMenuList = e.Result.ToList();
                    AddTreeNode("f0c5324c-9f23-4a22-8ee6-8a1446c23749", null);
                    tvRoleMenu.ItemContainerStyle = this.Resources["RedItemStyle"] as Style;
                    //if (menulist.Count > 0)
                    //{
                    //    foreach (SMT.SaaS.Permission.UI.PermissionService.T_SYS_ENTITYMENU tmpmenu in menulist)
                    //    {
                    //        TreeViewItem objTreenode = new TreeViewItem();
                    //        objTreenode.Header = tmpmenu.MENUNAME;
                    //        objTreenode.DataContext = tmpmenu;
                    //        if (TreeViewItem == null)
                    //        { 

                    //        }
                    //    }

                    //}
                }
            }
        }

        /// <summary>
        /// 获取菜单的子菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="?"></param>
        void ServiceClient_GetSysMenuInfosListByParentIDCompleted(object sender,GetSysMenuInfosListByParentIDCompletedEventArgs e)
        {

        }

        void AddTreeNode(string ParentID, TreeViewItem treeViewItem)
        {
            List<T_SYS_ENTITYMENU> childs = GetChildMenuByParentID(ParentID);
            if (childs == null || childs.Count <= 0)
                return;

            foreach (T_SYS_ENTITYMENU childmenu in childs)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = childmenu.MENUNAME;
                childItem.DataContext = childmenu;
                

                if (treeViewItem == null)
                {
                    tvRoleMenu.Items.Add(childItem);
                }
                else
                {
                    treeViewItem.Items.Add(childItem);
                }
                //AddTreeNode(childmenu.T_SYS_ENTITYMENU2.ENTITYMENUID, childItem);
                AddTreeNode(childmenu.ENTITYMENUID,childItem);
                
            }
            

        }

        private List<T_SYS_ENTITYMENU> GetChildMenuByParentID(string ParentID)
        {
            List<T_SYS_ENTITYMENU> menus = new List<T_SYS_ENTITYMENU>();

            foreach (T_SYS_ENTITYMENU menu in ViewMenuList)
            {
                if (menu.T_SYS_ENTITYMENU2 != null && menu.T_SYS_ENTITYMENU2.ENTITYMENUID == ParentID)
                    menus.Add(menu);
            }
            return menus;
        }

        void LoadData()
        {
            ServiceClient.GetSysMenuByTypeAsync("","");
        }

        

        

        public SetRoleMenu()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ItemCheckbox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

