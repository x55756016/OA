using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace SMT.SAAS.Controls.Toolkit
{

    [TemplatePart(Name = "LayoutRootMBI", Type = typeof(Canvas))]
    [TemplatePart(Name = "ItemrMBI", Type = typeof(Rectangle))]
    [TemplatePart(Name = "ItemTextMBI", Type = typeof(TextBlock))]
    [TemplatePart(Name = "ItemHighlightMBI", Type = typeof(Canvas))]
    [TemplatePart(Name = "ItemHighlightrMBI", Type = typeof(Rectangle))]
    [TemplatePart(Name = "ItemText_copyMBI", Type = typeof(TextBlock))]
    [TemplatePart(Name = "ItemDropDownMBI", Type = typeof(Canvas))]
    [TemplatePart(Name = "baseRectMBI", Type = typeof(Grid))]
    [TemplatePart(Name = "itemHolderMBI", Type = typeof(StackPanel))]
    [TemplateVisualState(Name = "ItemHighlightedMBI", GroupName = "menuNavigationMBI")]
    [TemplateVisualState(Name = "noneHighlightedMBI", GroupName = "menuNavigationMBI")]
    

    [ContentPropertyAttribute("items")]
    public class MenuBarItem : Control
    {
        public MenuBar parentMenu;
        //界面组件
        private Canvas LayoutRootMBI;
        private Rectangle ItemrMBI;
        private TextBlock ItemTextMBI;
        private Canvas ItemHighlightMBI;
        private Rectangle ItemHighlightrMBI;
        private TextBlock ItemText_copyMBI;
        private Canvas ItemDropDownMBI;
        private Grid baseRectMBI;
        private StackPanel itemHolderMBI;

        private bool isOpen = false;
        public MenuBarItem()
        {
            this.DefaultStyleKey = typeof(MenuBarItem);
            this.Loaded += new RoutedEventHandler(MenuBarItem_Loaded);

            if (items == null)
            items = new ObservableCollection<MenuItem>();
            if (string.IsNullOrEmpty(MenuText))
                MenuText = "Default";           
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LayoutRootMBI = (Canvas)GetTemplateChild("LayoutRootMBI");
            ItemrMBI = (Rectangle)GetTemplateChild("ItemrMBI");
            ItemTextMBI = (TextBlock)GetTemplateChild("ItemTextMBI");
            ItemHighlightMBI = (Canvas)GetTemplateChild("ItemHighlightMBI");
            ItemHighlightrMBI = (Rectangle)GetTemplateChild("ItemHighlightrMBI");
            ItemText_copyMBI = (TextBlock)GetTemplateChild("ItemText_copyMBI");
            ItemDropDownMBI = (Canvas)GetTemplateChild("ItemDropDownMBI");
            baseRectMBI = (Grid)GetTemplateChild("baseRectMBI");
            itemHolderMBI = (StackPanel)GetTemplateChild("itemHolderMBI");


            ItemHighlightMBI.MouseEnter += new MouseEventHandler(ItemHighlight_MouseEnter);
            ItemHighlightMBI.MouseLeftButtonDown += new MouseButtonEventHandler(ItemHighlight_MouseLeftButtonDown);
            ItemHighlightMBI.MouseLeave += new MouseEventHandler(ItemHighlight_MouseLeave);
            ItemDropDownMBI.MouseLeave += new MouseEventHandler(ItemDropDown_MouseLeave);
        }

        
        void MenuBarItem_Loaded(object sender, RoutedEventArgs e)
        {

            this.ApplyTemplate();

            // change control dimension based on the text
            if (!string.IsNullOrEmpty(MenuText))
            {
                Point xy = getTextWidthHeight(MenuText, ItemTextMBI);
                ItemTextMBI.Width = xy.X;
                ItemTextMBI.Height = xy.Y;
                ItemTextMBI.Text = MenuText;
                ItemText_copyMBI.Width = xy.X;
                ItemText_copyMBI.Height = xy.Y;
                ItemText_copyMBI.Text = MenuText;

                //set width and height for canvas
                LayoutRootMBI.Width = xy.X + 15;
                LayoutRootMBI.Height = xy.Y + 10;
                ItemHighlightMBI.Width = xy.X + 15;
                ItemHighlightMBI.Height = xy.Y + 10;
                ItemHighlightrMBI.Width = xy.X + 15;
                ItemHighlightrMBI.Height = xy.Y + 10;
                ItemrMBI.Width = xy.X + 15;
                ItemrMBI.Height = xy.Y + 10;
            }

            // add menu items if any exists
            if (items != null && items.Count > 0)
            {
                Point xy = getLargest(items);
                xy.X += 15; //add space for arrow placement


                //set menu holder dimensions
                ItemDropDownMBI.Width = xy.X + 46;
                baseRectMBI.Width = xy.X + 46;

                ItemDropDownMBI.SetValue(Canvas.TopProperty, LayoutRootMBI.Height);

                // height is the height of the textbox plus 10 since we add 10
                //for the box around the text box. Then multiply by count and add 10
                // for margins
                ItemDropDownMBI.Height = (xy.Y + 10) * (items.Count) + 8;
                baseRectMBI.Height = (xy.Y + 10) * (items.Count) + 8;


                foreach (MenuItem item in items)
                {
                    //set menuItem dimensions before adding
                    item.SetDimension(xy);
                    item.parentMenuBarItem = this;

                    itemHolderMBI.Children.Add(item);
                }
            }
        }

        public void CollapseDropDown()
        {
            foreach (MenuItem item in items)
            {
                item.CollapseDropDown();
            }
            ItemDropDownMBI.Visibility = Visibility.Collapsed;
        }

        public void ShowDropDown()
        {
            ItemDropDownMBI.Visibility = Visibility.Visible;
        }

        internal void CollapseChildDropDownMenus()
        {
            foreach (MenuItem item in items)
            {
                item.CollapseDropDown();
            }
        }

        
        public Point getLargest(ObservableCollection<MenuItem> menuItems)
        {
            double width = 0;
            double height = 0;
            foreach (MenuItem item in menuItems)
            {
                Point xy = getTextWidthHeight(item.MenuText, ItemTextMBI);
                if (xy.X > width) width = xy.X;
                if (xy.Y > height) height = xy.Y;
            }
            return new Point(width, height);
        }


        // return the dimension of the text after aplying styles from given textblock
        private Point getTextWidthHeight(string text, TextBlock itemTextTB)
        {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Style = itemTextTB.Style;
            tb.FontFamily = itemTextTB.FontFamily;
            tb.FontSize = itemTextTB.FontSize;
            tb.FontSource = itemTextTB.FontSource;
            tb.FontStretch = itemTextTB.FontStretch;
            tb.FontStyle = itemTextTB.FontStyle;
            tb.FontWeight = itemTextTB.FontWeight;

            Point xy = new Point(tb.ActualWidth, tb.ActualHeight);
            return xy;
        }
 

        public static readonly DependencyProperty itemsProperty =
                DependencyProperty.Register("items", typeof(ObservableCollection<MenuItem>), typeof(MenuBarItem), null);

        public ObservableCollection<MenuItem> items
        {
            get { return (ObservableCollection<MenuItem>)GetValue(itemsProperty); }
            set { SetValue(itemsProperty, value); }
        }

        public static readonly DependencyProperty menuTextProperty =
                DependencyProperty.Register("MenuText", typeof(string), typeof(MenuBarItem), null);

        public string MenuText
        {
            get { return (string)GetValue(menuTextProperty); }
            set { SetValue(menuTextProperty, value); }
        }

 

        void ItemDropDown_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseDropDown();
            //isOpen = false;
        }

        void ItemHighlight_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "noneHighlightedMBI", true);
            //if (isOpen)
            //{
            //    CollapseDropDown(); 
            //}

        }

        void ItemHighlight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (items != null && items.Count > 0)
            {
                ShowDropDown();
            }
        }

        void ItemHighlight_MouseEnter(object sender, MouseEventArgs e)
        {
            parentMenu.CollapseChildDropDownMenus();
            
            VisualStateManager.GoToState(this, "ItemHighlightedMBI", true);

            //if (items != null && items.Count > 0)
            //{
            //    ShowDropDown();
            //    isOpen = true;
            //}

        }  
    }
}
