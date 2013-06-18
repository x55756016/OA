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
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace SMT.SAAS.Controls.Toolkit.Windows
{
    /// <summary>
    /// 任务栏项
    /// </summary>
    public partial class TaskBarItem : UserControl
    {
        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        public event EventHandler Clicked;
        public TaskBarItem()
        {
            InitializeComponent();
            InitWidth = this.ActualWidth;
        }
        public string IocPath
        {
            get { return (string)base.GetValue(IocPathProperty); }
            set { base.SetValue(IocPathProperty, value); }
        }
        public static readonly DependencyProperty IocPathProperty =
            DependencyProperty.Register("IocPath", typeof(string), typeof(TaskBarItem), new PropertyMetadata(new PropertyChangedCallback(TaskBarItem.OnIocPathProperty)));
        public static void OnIocPathProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TaskBarItem item = (TaskBarItem)d;
            item.iocImage.Source = new BitmapImage(new Uri((string)e.NewValue, UriKind.Relative));
        }
        private void btnTitle_Click(object sender, RoutedEventArgs e)
        {
            if (this.Clicked != null)
            {
                this.Clicked(this, EventArgs.Empty);
            }
        }
        public double InitWidth { get; set; }
        private string GetToolTip()
        {
            return Caption;
        }
        public string Caption
        {
            get
            {
                return this.TaskBarTitel.Text;
            }
            set
            {
                //ToolTipService.SetToolTip(this,value);
                this.TaskBarTitel.Text = value;
            }
        }
        private void btnTitle_MouseEnter(object sender, MouseEventArgs e)
        {
            SelectBackground.Visibility = Visibility.Visible;
           
        }
        private void btnTitle_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectBackground.Visibility = Visibility.Collapsed;  
        }
    }
}
