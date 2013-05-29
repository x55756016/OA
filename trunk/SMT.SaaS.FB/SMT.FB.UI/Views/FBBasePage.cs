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
using SMT.FB.UI.Common;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Platform;
using System.ComponentModel;
using SMT.FB.UI.FBCommonWS;
using System.Collections.Generic;

namespace SMT.FB.UI.Views
{
    public class FBBasePage : Page
    {
        
        public event EventHandler FBBasePageLoaded;

        public FBBasePage()
        {
            this.Loaded += new RoutedEventHandler(FBBasePage_Loaded);

        }

        void FBBasePage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!this.CheckPermisstion())
                {

                    this.CloseParent(CommonFunction.GetString("NOPERMISSION"));
                    return;
                }


                InitProcess();
                ShowProcess();
                OrderHelper.InitOrderInfoCompleted += new EventHandler<ActionCompletedEventArgs<string>>(OrderHelper_InitOrderInfoCompleted);
                OrderHelper.InitOrderInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void OrderHelper_InitOrderInfoCompleted(object sender, ActionCompletedEventArgs<string> e)
        {
            try
            {
                OrderHelper.InitOrderInfoCompleted -= new EventHandler<ActionCompletedEventArgs<string>>(OrderHelper_InitOrderInfoCompleted);
                if (e != null)
                {
                    throw new Exception(e.Result);
                }
                if (FBBasePageLoaded != null)
                {
                    FBBasePageLoaded(this, null);
                }
                CloseProcess();
            }
            catch (Exception ex)
            {
                CloseProcess();
                MessageBox.Show(ex.ToString());
            }
        }

        

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

        [DefaultValue(false)]
        public static bool ShowTitleBar
        {
            get;
            set;
        }

        virtual public bool CheckPermisstion()
        {
            return true;
        }

        public void CloseParent(string msg )
        {
            try
            {
                CommonFunction.ShowErrorMessage(msg);
                System.Windows.Controls.Window winParent = this.Parent as System.Windows.Controls.Window;
                if (winParent != null)
                {
                    winParent.Close();
                }
            }
            catch
            {
                this.Content = new TextBlock { Text = msg };
            }


        }

        private static Dictionary<string, PermissionRange> dictLessPermission;
        public static Dictionary<string, PermissionRange> DictLessPermission
        {
            get
            {
                if ( dictLessPermission == null)
                {
                    dictLessPermission = new Dictionary<string, PermissionRange>();

                    dictLessPermission.Add(typeof(T_FB_COMPANYBUDGETSUMMASTER).Name, PermissionRange.Company);
                    dictLessPermission.Add(typeof(T_FB_COMPANYTRANSFERMASTER).Name, PermissionRange.Department);
                    dictLessPermission.Add(typeof(T_FB_COMPANYBUDGETMODMASTER).Name, PermissionRange.Department);

                    dictLessPermission.Add(typeof(T_FB_DEPTBUDGETSUMMASTER).Name, PermissionRange.Company);
                    dictLessPermission.Add(typeof(T_FB_COMPANYBUDGETAPPLYMASTER).Name, PermissionRange.Department);
                    
                    dictLessPermission.Add(typeof(T_FB_DEPTTRANSFERMASTER).Name, PermissionRange.Department);
                    dictLessPermission.Add(typeof(T_FB_DEPTBUDGETAPPLYMASTER).Name, PermissionRange.Department);
                    dictLessPermission.Add(typeof(T_FB_DEPTBUDGETADDMASTER).Name, PermissionRange.Department);
                }
                return dictLessPermission;
            }
        }
    }
}
