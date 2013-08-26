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
using System.Linq;

using SMT.Workflow.Platform.Designer.Common;

namespace SMT.Workflow.Platform.Designer.Utils
{
    public class WfUtils
    {
        //规则列表
        public static List<StateType> StateList;  

   


        /// <summary>
        /// 获取提交人类型
        /// </summary>
        /// <returns></returns>
        public static List<UserType> GetUserTypeList()
        {
            List<UserType> UserTypeList = new List<UserType> 
                       {
                         new UserType{ TypeCode="CREATEUSER", TypeName="单据所有者"},
                         new UserType{ TypeCode="EDITUSER", TypeName="审批人"}
                       
                        };

            return UserTypeList;
        }

        /// <summary>
        /// 通过公司ID筛选角色
        /// </summary>
        /// <param name="RoleList"></param>
        /// <param name="CompanyID"></param>
        /// <returns></returns>
        public static List<StateType> GetRoleListByCompanyID(List<StateType> RoleList, string CompanyID)
        {

            List<StateType> StateList = new List<StateType>();
            if (RoleList != null && RoleList.Count > 0)
            {
                List<StateType> dt = RoleList.Where(c => c.CompanyID == CompanyID || c.CompanyID == null).ToList();
                StateType tmp;
                for (int i = 0; i < dt.Count; i++)
                {
                    tmp = new StateType();
                    tmp.StateCode = dt[i].StateCode;
                    tmp.StateName = dt[i].StateName;
                    tmp.CompanyID = dt[i].CompanyID;
                    StateList.Add(tmp);

                }
            }
            return StateList;
        }
    }
}
