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

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalaryLoginForm : BasePage, IClient
    {
        private SalaryServiceClient client;
        public bool Result  { get ; set ; }
        public event EventHandler HandlerClick;  
        public SalaryLoginForm()
        {
            InitializeComponent();
            InitParas();
        }
        private void InitParas()
        {
            client = new SalaryServiceClient();
            this.Result = false;
            client.LoginCheckCompleted += new EventHandler<LoginCheckCompletedEventArgs>(client_LoginCheckCompleted);
        }

        public T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
         {
             DependencyObject parent = VisualTreeHelper.GetParent(obj);
 
             while (parent != null)
             {
                 if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                 {
                     return (T)parent;
                 }
 
                 parent = VisualTreeHelper.GetParent(parent);
             }
 
             return null;
         }


        private void btDecryption_Click(object sender, RoutedEventArgs e)
        {
            string currentPwd = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(pwd.Password);
            client.LoginCheckAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, currentPwd);
            //client.LoginCheckAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, pwd.Password);
        }
        void client_LoginCheckCompleted(object sender, LoginCheckCompletedEventArgs e)
        {
            if (e.Result)
            {
                LayoutRoot.Visibility = Visibility.Collapsed;
                HandlerClicked();
            }
            else
            {
                tbmsg.Text = Utility.GetResourceStr("PASSWORDERROR");
            }
        }
        private void HandlerClicked()                      
        {
            if (HandlerClick != null)
            {
                Result = true;
                HandlerClick(this, null);
            }
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

    }
}
