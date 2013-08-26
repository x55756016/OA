/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：zhangyh   
	 * 创建日期：2011/9/20 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer.Views.FlowDesign 
	 * 描　　述： 树控件（带右键弹出菜单）
	 * 模块名称：工作流设计器
* ---------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

using Telerik.Windows.Controls;
using SMT.Workflow.Platform.Designer.Common;

namespace SMT.Workflow.Platform.Designer.UControls
{
    public partial class TreeControl : UserControl
    {
        /// <summary>
        /// 树节点选择委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>        
        public delegate void SelectionChanged(object sender, SelectionChangedArgs args);

        /// <summary>
        /// 树节点选择事件
        /// </summary>
        public event SelectionChanged OnSelectionChanged;

        /// <summary>
        /// 菜单单击委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void MenuClicked(object sender, MenuClickArgs args);

        /// <summary>
        /// 菜单单击事件
        /// </summary>
        public event MenuClicked OnMenuClicked;

        /// <summary>
        /// 右键菜单集合
        /// </summary>
        private ObservableCollection<ContextMenuItem> _menuItems;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TreeControl()
        {
            InitializeComponent();
            treeView.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(treeView_SelectionChanged);
        }

        /// <summary>
        /// 树节点项
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return treeView.ItemsSource; }
            set { treeView.ItemsSource = value; }
        }

        /// <summary>
        /// 右键菜单项
        /// </summary>
        public ObservableCollection<ContextMenuItem> MenuItems
        {
            get { return _menuItems; }
            set { _menuItems = value; }
        }

        /// <summary>
        /// 树节点选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            TreeItem item = treeView.SelectedItem as TreeItem;

            if (item != null)
            {
                if (OnSelectionChanged != null) OnSelectionChanged(sender, new SelectionChangedArgs() { SelectedItem = item });
            }
        }

        /// <summary>
        /// 菜单弹出  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (_menuItems != null && _menuItems.Count > 0) 
            {
                RadTreeViewItem item = radContextMenu.GetClickedElement<RadTreeViewItem>();
                if (item != null)
                {
                    radContextMenu.ItemsSource = _menuItems;
                }
            }
            else
            {
                (sender as RadContextMenu).IsOpen = false;
            }
        }

        /// <summary>
        /// 菜单单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radContextMenu_ItemClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadMenuItem item = e.OriginalSource as RadMenuItem;

            if (item != null)
            {
                ContextMenuItem menu = item.Header as ContextMenuItem;

                if (menu != null)
                {
                    if (OnMenuClicked != null) OnMenuClicked(sender, new MenuClickArgs() { MenuId = menu.MenuId });
                }
            }
        }


    }

    public class SelectionChangedArgs : EventArgs
    {
        public TreeItem SelectedItem { get; set; }
    }

    public class MenuClickArgs : EventArgs
    {
        public string MenuId { get; set; }
    }

    public class ContextMenuItem
    {

        public Image Icon
        {
            get
            {
                if (this.ImagePath != null)
                {
                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(this.ImagePath, UriKind.Relative));
                    return image;
                }
                else
                    return null;
            }
        }

        public string MenuId { get; set; }

        public string ImagePath { get; set; }

        public string Header
        {
            get;
            set;
        }

        public bool IsSeparator
        {
            get;
            set;
        }

        public ContextMenuItem()
        {
            this.Header = null;
            this.ImagePath = null;
            this.IsSeparator = false;
        }
    }
}
