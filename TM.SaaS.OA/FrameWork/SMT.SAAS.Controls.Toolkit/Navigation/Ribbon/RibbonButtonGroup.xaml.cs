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
using System.Collections.ObjectModel;


namespace SMT.SAAS.Controls.Toolkit.Navigation.Ribbon
{
    //导航菜单二级
    public partial class RibbonButtonGroup : UserControl
    {
        public RibbonButtonGroup()
        {
            InitializeComponent();

        }
        private Thickness itemMargin = new Thickness(-7.5, 0, 15, 0);
        private double offsetItemHeight = 6;
        private double offsetPanelHeight = 10;
        private double offsetTitelHeight = 20;
        public static readonly DependencyProperty ItemsProperty =
       DependencyProperty.Register("Items", typeof(ObservableCollection<RibbonButtonModel>), typeof(RibbonButtonGroup),
                                     new PropertyMetadata(new PropertyChangedCallback(RibbonButtonGroup.OnItemsPropertyChanged)));

        /// <summary>
        /// RibbonButtonGroup元素
        /// </summary>
        public ObservableCollection<RibbonButtonModel> Items
        {
            get { return (ObservableCollection<RibbonButtonModel>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }


        public string GroupTitle { get; set; }
        public double GroupHeight { get; private set; }
        public double GroupWidth { get; private set; }
        /// <summary>
        /// 单击事件
        /// </summary>
        public event EventHandler<RibbonButtonClickArgs> Click;
        /// <summary>
        /// 加载RibbonBar子元素
        /// </summary>
        private void LoadItems()
        {
            Titel.Text = GroupTitle;
            double parentwidth = 0;
            foreach (RibbonButtonModel item in Items)
            {
                RibbonButton childbutton = new RibbonButton() { Model = item };
                
                Point itemTextSize = GetTextWidthHeight(item.Titel, childbutton.ribbonName);
                if (parentwidth < itemTextSize.X)
                    parentwidth = itemTextSize.X;

                childbutton.Height = itemTextSize.Y + offsetItemHeight;
                childbutton.Margin = itemMargin;

                GroupHeight = GroupHeight + itemTextSize.Y + offsetItemHeight + itemMargin.Top + itemMargin.Bottom;

                childbutton.Click += (obj, args) =>
                {
                    if (Click != null)
                        Click(childbutton, new RibbonButtonClickArgs() { DataContext = childbutton.Model.DataContext });
                };
                ChildPanel.Children.Add(childbutton);
            }

            double titelHeight = GetTextWidthHeight(GroupTitle, Titel).Y;
            GroupHeight = GroupHeight + titelHeight + offsetItemHeight + offsetPanelHeight;
            GroupWidth = parentwidth + itemMargin.Left + itemMargin.Right;

        }

        private Point GetTextWidthHeight(string text, TextBlock sourceText)
        {
            TextBlock tbtemp = new TextBlock();
            tbtemp.Text = text;
            tbtemp.Style = sourceText.Style;
            tbtemp.FontFamily = sourceText.FontFamily;
            tbtemp.FontSize = sourceText.FontSize;
            tbtemp.FontSource = sourceText.FontSource;
            tbtemp.FontStretch = sourceText.FontStretch;
            tbtemp.FontStyle = sourceText.FontStyle;
            tbtemp.FontWeight = sourceText.FontWeight;
            Point xy = new Point(tbtemp.ActualWidth, tbtemp.ActualHeight);
            return xy;
        }

        public static void OnItemsPropertyChanged(DependencyObject objects, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                RibbonButtonGroup bases = objects as RibbonButtonGroup;
                bases.LoadItems();
            }
        }
    }
}
