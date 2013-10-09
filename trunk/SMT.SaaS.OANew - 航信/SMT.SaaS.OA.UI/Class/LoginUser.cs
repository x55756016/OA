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

using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.Generic;
namespace SMT.SaaS.OA.UI.Class
{
    public class LoginUser
    {
        public LoginUser()
        {
            CustomPerms = new List<T_SYS_ENTITYMENUCUSTOMPERM>();
        }
        public T_SYS_USER UserInfo { get; set; }
        public List<V_Permission> PermissionInfo { get; set; }
        public V_EMPLOYEEPOST EmployeeInfo { get; set; }
        
        public List<T_SYS_ENTITYMENUCUSTOMPERM> CustomPerms { get; set; }


        private T_HR_POST currentLoginPost;

        public T_HR_POST CurrentLoginPost
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
