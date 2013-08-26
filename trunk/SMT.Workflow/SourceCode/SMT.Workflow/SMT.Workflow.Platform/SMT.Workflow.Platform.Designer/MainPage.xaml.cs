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
using SMT.Workflow.Platform.Designer.Utils;


namespace SMT.Workflow.Platform.Designer
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            #region 加载登录人员信息         
            Utility.CurrentUser = new Saas.Tools.PermissionWS.T_SYS_USER();
            Utility.CurrentUser.CREATEDATE = DateTime.Now;
            Utility.CurrentUser.CREATEUSER = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            Utility.CurrentUser.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;
            Utility.CurrentUser.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            Utility.CurrentUser.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;     
            Utility.CurrentUser.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;           
            Utility.CurrentUser.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            Utility.CurrentUser.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            Utility.CurrentUser.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;        
            Utility.CurrentUser.STATE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeState;
            Utility.CurrentUser.SYSUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID;          
            Utility.CurrentUser.UPDATEDATE = DateTime.Now;
            Utility.CurrentUser.UPDATEUSER = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            Utility.CurrentUser.USERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            if (Application.Current.Resources["CurrentUserID"] == null)
            {
                Application.Current.Resources.Add("CurrentUserID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
           
            #endregion
            //发布时下面代码要注释
            //System.Windows.Controls.Window.Parent = windowParent;
            //System.Windows.Controls.Window.TaskBar = new StackPanel();
            //System.Windows.Controls.Window.Wrapper = this;
            //System.Windows.Controls.Window.IsShowtitle = false;
        }
        private void txtExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        bool isselect = true;
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl1 != null)
            {
                switch (tabControl1.SelectedIndex)
                {
                    case 0:
                   
                        break;
                    case 1:
                        if (isselect)
                        {
                            Engine.EngineInit();
                            isselect = false;
                        }
                        break;
                    case 2:
                        break;
                }
            }
        }
    }
}

