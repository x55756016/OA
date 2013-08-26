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
using Telerik.Windows.Controls;
using Telerik.Windows;

namespace SMT.Workflow.Platform.UI.ProcessBar
{
    public partial class FlowTreeview : UserControl
    {
        public FlowTreeview()
        {
            InitializeComponent();
            LoadTreeData();
            RadTreeView1.AddHandler(RadMenuItem.ClickEvent, new RoutedEventHandler(OnMenuItemClicked));
            LoadContentMenu();
        }

        #region 事件跳转
        private void OnMenuItemClicked(object sender, RoutedEventArgs args)
        {
            RadRoutedEventArgs e = args as RadRoutedEventArgs;
            RadMenuItem item = e.OriginalSource as RadMenuItem;
            if ((item == null))
            {
                return;
            }
            switch (item.Tag.ToString())
            {
                case "Add":
                    this.AddItem();
                    break;
                case "Remove":
                    this.RemoveItem();
                    break;
                case "Disable":
                    this.DisableItem();
                    break;
            }
        }
        #endregion

        #region 加载基础数据树
        public void LoadTreeData()
        {
            for (int i = 0; i < 6; i++)
            {
                RadTreeViewItem viewitem = new RadTreeViewItem();
              

                viewitem.Header = i.ToString();
                viewitem.Tag = i;
                if (i > 3)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        RadTreeViewItem viewitem2 = new RadTreeViewItem();
                        viewitem2.Header = j.ToString() + "第二层";
                        viewitem2.Tag = j * 10;
                        viewitem.Items.Add(viewitem2);
                        //viewitem2.Click += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(viewitem_Click);
                    }
                }
                RadTreeView1.Items.Add(viewitem);
                //viewitem.Click += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(viewitem_Click);
            }

        }
        #endregion

        #region 加载右键菜单项
        public void LoadContentMenu()
        {
            RadContextMenu Contextmenu = new RadContextMenu();
            Contextmenu.Opened += new RoutedEventHandler(ContextMenuOpened);
            RadMenu menus = new RadMenu();
            menus.Orientation = Orientation.Vertical;

            RadMenuItem menuitem1 = new RadMenuItem();
            menuitem1.Header = "新建";
            menuitem1.Tag = "Add";
            menus.Items.Add(menuitem1);

            RadMenuItem menuitem2 = new RadMenuItem();
            menuitem2.Header = "删除";
            menuitem2.Tag = "Remove";
            menus.Items.Add(menuitem2);

            RadMenuItem menuitem3 = new RadMenuItem();
            menuitem3.Header = "禁用";
            menuitem3.Tag = "Disable";
            menus.Items.Add(menuitem3);
            Contextmenu.Items.Add(menus);

            RadContextMenu.SetContextMenu(RadTreeView1, Contextmenu);
        }
        #endregion

        #region 选中项事件
        void viewitem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var item = e.Source as RadTreeViewItem;
            var t = e.Source;
        }
        #endregion

        #region 新增项
        void AddItem()
        {
            RadTreeViewItem treeViewItem = RadTreeView1.SelectedContainer;
            if (treeViewItem == null)
            {
                this.RadTreeView1.Items.Add(new RadTreeViewItem()
                {
                    Header = "New Node",
                });
                return;
            }
            treeViewItem.Items.Add(new RadTreeViewItem()
            {
                Header = "New Node",
            });
            treeViewItem.IsExpanded = true;

        }
        #endregion

        #region 移除项
        void RemoveItem()
        {
            while (RadTreeView1.SelectedItems.Count > 0)
            {
                RadTreeViewItem treeViewItem = RadTreeView1.ContainerFromItemRecursive(RadTreeView1.SelectedItems[0]);

                if (treeViewItem == null)
                {
                    return;
                }
                if (treeViewItem.Parent is RadTreeViewItem)
                {
                    (treeViewItem.Parent as RadTreeViewItem).Items.Remove(treeViewItem);
                }
                else
                {
                    this.RadTreeView1.Items.Remove(treeViewItem);
                }
            }

        }
        #endregion

        #region 禁用项
        void DisableItem()
        {
            foreach (object item in RadTreeView1.SelectedItems)
            {
                RadTreeViewItem treeViewItem = RadTreeView1.ContainerFromItemRecursive(item);

                if (treeViewItem != null)
                {
                    treeViewItem.IsEnabled = false;
                }
            }
        }
        #endregion

        #region 打开右键菜单事件
        private void ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (sender as RadContextMenu).GetClickedElement<RadTreeViewItem>();

            if (treeViewItem == null)
            {
                (sender as RadContextMenu).IsOpen = false;
                return;
            }

            if (!treeViewItem.IsSelected)
            {
                RadTreeView1.SelectedItems.Clear();
                RadTreeView1.SelectedItems.Add(treeViewItem.Item);
            }
        }
        #endregion
    }
}
