using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;

namespace SMT.FB.UI.Common
{
    public class FBBaseControl : UserControl
    {
        public FBBaseControl()
        {
            
            this.Loaded += new RoutedEventHandler(FBBaseForm_Loaded);
        }

        void FBBaseForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitProcess();
            if (FBBasePageLoaded != null)
            {
                FBBasePageLoaded(this, null);
            }
        }

        public event EventHandler FBBasePageLoaded;

        SMTLoading loadbar = null;
        private void InitProcess()
        {
            Panel parent = this.Content as Panel;
            if (parent != null)
            {
                Grid g = new Grid();

                this.Content = g;
                g.Children.Add(parent);
                loadbar = new SMTLoading(); //全局变量
                g.Children.Add(loadbar);
            }
        }
        public void ShowProcess()
        {
            if (loadbar != null)
            {
                loadbar.Start();//调用服务时写
            }
        }
        public void CloseProcess()
        {
            if (loadbar != null)
            {
                loadbar.Stop();
            }

        }
    }
      
}
