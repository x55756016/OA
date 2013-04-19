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

using System.Collections.Generic;

namespace SMT.HRM.UI
{
    public class LoginUser
    {
        public LoginUser()
        {
            CustomPerms = new List<SMT.Saas.Tools.PermissionWS.T_SYS_ENTITYMENUCUSTOMPERM>();
        }

        public SMT.Saas.Tools.PermissionWS.T_SYS_USER UserInfo { get; set; }
        public List<SMT.Saas.Tools.PermissionWS.V_Permission> PermissionInfo { get; set; }
        public List<SMT.Saas.Tools.PermissionWS.T_SYS_ENTITYMENUCUSTOMPERM> CustomPerms { get; set; }

        public SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST EmployeeInfo { get; set; }

        private SMT.Saas.Tools.PersonnelWS.T_HR_POST currentLoginPost;

        public SMT.Saas.Tools.PersonnelWS.T_HR_POST CurrentLoginPost
        {
            get
            {
                if (currentLoginPost == null)
                {
                    if (EmployeeInfo != null && EmployeeInfo.EMPLOYEEPOSTS != null)
                    {
                        currentLoginPost = EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST;
                    }
                }
                return currentLoginPost;
            }
            set { currentLoginPost = value; }
        }
  
    }
}
