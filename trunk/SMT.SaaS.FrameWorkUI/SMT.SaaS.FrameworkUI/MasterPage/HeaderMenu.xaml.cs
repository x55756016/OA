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

namespace SMT.SaaS.FrameworkUI
{
    public partial class HeaderMenu : UserControl
    {
        public HeaderMenu()
        {
            InitializeComponent();   
        }
        
        public void SetUserNameAndDepartmentName(string UserName,string PostName,string DepartmentName,string CompanyName)
        {
            TextUserDepartment.Text = string.Format("{0} - {1} - {2} - {3}",CompanyName,DepartmentName, PostName,UserName);
        }

        #region Button控件
        public Button HBHome
        {
            get { return this.NH1; }
        }
        public Button HBFullScreen
        {
            get { return this.NH3; }
        }
        public Button HBHelp
        {
            get { return this.NH4; }
        }
        public Button HBPervious
        {
            get { return this.ButtonPrevious; }
            set { ButtonPrevious = value; }
        }
        public Button HBNext
        {
            get { return this.ButtonNext; }
            set { ButtonNext = value; }
        }
        public Button HBLoginOut
        {
            get { return this.LoginOut; }
            set { LoginOut = value; }
        }
        #endregion
    }
}
