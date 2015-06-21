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

namespace SMT.SAAS.Controls.Toolkit.InfoPanel
{
    public partial class InfoIndexItem : UserControl
    {
         private int id;
        public RoutedEventHandler Click;
        public InfoIndexItem()
        {
            this.InitializeComponent();
            this.LayoutRoot.MouseLeftButtonDown += new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonDown);
        }
        public void DeSelectItem()
        {
            VisualStateManager.GoToState(this, "Deselect", false);
        }


        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Click != null)
            {
                this.Click.Invoke(this, e);
            }
        }

        public void SelectItem()
        {
            VisualStateManager.GoToState(this, "Select", false);
        }

        // Properties
        public int Id
        {

            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }
        //public InfoIndexItem()
        //{
        //    InitializeComponent();

        //    this.MouseEnter += new MouseEventHandler(InfoIndexItem_MouseEnter);
        //    this.MouseLeave += new MouseEventHandler(InfoIndexItem_MouseLeave);
            
        //}
        //public object DataContext 
        //{
        //    get { return LayoutRoot.DataContext; }
        //    set { LayoutRoot.DataContext = value; }
        //}
        //void InfoIndexItem_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    Mask.Visibility = Visibility.Collapsed;
        //}

        //void InfoIndexItem_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    Mask.Visibility = Visibility.Visible;

        //}
    }
}
