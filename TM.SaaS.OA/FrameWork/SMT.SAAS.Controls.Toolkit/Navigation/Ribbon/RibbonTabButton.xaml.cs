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
    public partial class RibbonTabButton : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register("Items", typeof(ObservableCollection<RibbonButtonModel>), typeof(RibbonTabButton), null);
        public ObservableCollection<RibbonButtonModel> Items
        {
            get { return (ObservableCollection<RibbonButtonModel>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public string Titel { get; set; }
        public string IocPath { get; set; }
        public string URL { get; set; }

        public static double OffsetWidth = 0.3;
        public bool IsSelected = false;

        public RibbonTabButton()
        {
            InitializeComponent();
            if (Items == null)
                Items = new ObservableCollection<RibbonButtonModel>();
        }

        private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Items.Count <= 0)
            { 
                ribbontabSelectBackground.Visibility=Visibility.Visible;
            }
        }

        private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Items.Count <= 0)
            {
                ribbontabSelectBackground.Visibility = Visibility.Collapsed;
            }
        }

    }
}
