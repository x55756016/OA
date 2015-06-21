using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace SMT.SAAS.Controls.Toolkit
{
    [TemplatePart(Name = "baseRectMB", Type = typeof(StackPanel))]
    [ContentPropertyAttribute("Items")]
    public class MenuBar : Control
    {
        //菜单主容器
        public StackPanel baseRectMB;
        public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register("Items", typeof(ObservableCollection<MenuBarItem>), typeof(MenuBar), new PropertyMetadata(new PropertyChangedCallback(MenuBar.OnItemsPropertyChanged)));

        public MenuBar()
        {            
            this.DefaultStyleKey = typeof(MenuBar);
            this.Loaded += new RoutedEventHandler(MenuBar_Loaded);
            //初始化菜单项容器
            if (Items == null)
                Items = new ObservableCollection<MenuBarItem>();           
            
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            baseRectMB = (StackPanel)GetTemplateChild("baseRectMB");
            //设置子元素的父容器
            foreach (MenuBarItem item in Items)
            {
                item.parentMenu = this;
            }
            
        }

        void MenuBar_Loaded(object sender, RoutedEventArgs e)
        {
            this.ApplyTemplate();
            //加载菜单项
            LoadItem();
        }

        private void LoadItem()
        {
           
            foreach (MenuBarItem item in Items)
            {
                item.parentMenu = this;
                baseRectMB.Children.Add(item);
            }
        }

        internal void CollapseChildDropDownMenus()
        {
            foreach (MenuBarItem item in Items)
            {
                item.CollapseDropDown();
            }
        }
        /// <summary>
        /// 菜单项集合
        /// </summary>
        public ObservableCollection<MenuBarItem> Items
        {
            get { return (ObservableCollection<MenuBarItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        
        public static void OnItemsPropertyChanged(DependencyObject o,DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                MenuBar bases = o as MenuBar;
                bases.LoadItem();
            }
        }
    }
}
