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
using System.Windows.Controls.Primitives;
using System.Windows.Media.Effects;

namespace SMT.SaaS.FrameworkUI
{
    public partial class ToolBarItemGroup : UserControl
    {
        public event EventHandler Click;
        Popup itemPop = new Popup();
        private static bool _createChild = false;

        public ToolBarItemGroup()
        {
            InitializeComponent();
            this.titel.Click += new RoutedEventHandler(titel_Click);
            this.more.Click += new RoutedEventHandler(more_Click);
        }

        void more_Click(object sender, RoutedEventArgs e)
        {
            if (itemPop.IsOpen)
            {
                itemPop.IsOpen = false;
            }
            else
            {
                if (_createChild)
                {
                    itemPop.IsOpen = true;
                }
                else
                {
                    #region 初始化子菜单
                    _createChild = true;

                    Canvas parent = new Canvas();
                    parent.MouseLeftButtonDown += (o, args) =>
                    {
                        if (itemPop.IsOpen)
                        {
                            itemPop.IsOpen = false;
                        }
                    };
                    StackPanel itemPanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        Background = new SolidColorBrush(Colors.Gray),
                        Margin = new Thickness(3, 0, 0, 0)
                    };
                    itemPanel.Effect = new DropShadowEffect();

                    ToolBarItemGroupModel model = new ToolBarItemGroupModel()
                    {
                         DataContext=this.Model.DataContext, IocPath=this.Model.IocPath, IsActivate=this.Model.IsActivate, Titel=this.Model.Titel, URL=this.Model.URL
                    };
                    ToolBarItemGroup firstChild = new ToolBarItemGroup() { Model = model, Margin = new Thickness(1), Height = 26, Width = 75 };
                    firstChild.Click += (o, args) =>
                    {
                        if (Click != null)
                            Click(o, args);
                    };
                    itemPanel.Children.Add(firstChild);

                    foreach (var item in Model.Items)
                    {
                        ToolBarItemGroup child = new ToolBarItemGroup() { Model = item, Margin = new Thickness(1), Height = 26, Width = 75 };
                        child.Click += (o, args) =>
                            {
                                if (Click != null)
                                    Click(o, args);
                            };
                        itemPanel.Children.Add(child);
                    }
                    itemPanel.Height = (Model.Items.Count + 1) * 28;
                    parent.Children.Add(itemPanel);
                    itemPop.Child = parent;

                    double top = Canvas.GetTop(Item) + Item.ActualHeight;
                    Canvas.SetTop(itemPop, top);
                    Root.Children.Add(itemPop);
                    itemPop.IsOpen = true;
                    #endregion
                }
            }
        }

        void titel_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
                Click(this, EventArgs.Empty);
        }
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(ToolBarItemGroupModel), typeof(ToolBarItemGroup),
            new PropertyMetadata(new PropertyChangedCallback(ToolBarItemGroup.OnModelPropertyChanged)));
        public ToolBarItemGroupModel Model
        {
            get { return (ToolBarItemGroupModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        public static void OnModelPropertyChanged(DependencyObject objects, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ToolBarItemGroup bases = objects as ToolBarItemGroup;
                ToolBarItemGroupModel model = args.NewValue as ToolBarItemGroupModel;

                bases.titel.DataContext = model;
                if (model.Items.Count > 0)
                {
                    bases.more.Visibility = Visibility.Visible;
                }
                else
                {
                    bases.more.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            Item.Background = Application.Current.Resources["CommonStyleYellow"] as Brush;
            ItemBorder.BorderThickness = new Thickness(1);
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            Item.Background = new SolidColorBrush(Colors.Transparent);
            ItemBorder.BorderThickness = new Thickness(0);
            more.BorderThickness = new Thickness(0);
        }
    }
}
