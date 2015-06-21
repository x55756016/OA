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
    //导航菜单三级
    public partial class RibbonButton : UserControl
    {

        /// <summary>
        /// 单击事件
        /// </summary>
        public event RoutedEventHandler Click;

        public static readonly DependencyProperty ModelProperty =
       DependencyProperty.Register("Model", typeof(RibbonButtonModel), typeof(RibbonButton),
        new PropertyMetadata(new PropertyChangedCallback(RibbonButton.OnModelPropertyChanged)));
        public RibbonButtonModel Model
        {
            get { return (RibbonButtonModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
        public RibbonButton()
        {
            InitializeComponent();
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click(this, new RoutedEventArgs());
            }
        }

        public static void OnModelPropertyChanged(DependencyObject objects, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                RibbonButton bases = objects as RibbonButton;
                RibbonButtonModel model = args.NewValue as RibbonButtonModel;
                
                if (model.Items.Count > 0)
                {
                   
                    bases.ribbonName.Text = model.Titel;
                    bases.ribbonName.FontWeight = FontWeights.Medium;
                    bases.ribbonName.Foreground = new SolidColorBrush(Color.FromArgb(100,73,95,156));
                }
                else
                {
                    if (!model.IsActivate)
                    {
                        bases.IsEnabled = false;
                        bases.Sign.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    bases.ribbonName.Text = model.Titel; 
                }
            }
        }

        private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Model.Items.Count <= 0)
            {
                this.Cursor = Cursors.Hand;
                ribbonbtnSelectBackground.Visibility = Visibility.Visible;
            }
        }

        private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Model.Items.Count <= 0)
            {
                ribbonbtnSelectBackground.Visibility = Visibility.Collapsed;
            }
        }
    }
}
