using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEEDETAIL
    {
        public string EMPLOYEEID { get; set; }
        public string EMPLOYEENAME { get; set; }
        public string EMPLOYEEENAME { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string EMPLOYEESTATE { get; set; }
        public int WORKAGE { get; set; }
        public string SEX { get; set; }
        public string OFFICEPHONE { get; set; }
        public string MOBILE { get; set; }
        public byte[] PHOTO { get; set; }
        public string POSTID { get; set; }
        public string URGENCYPERSON { get; set; }
        public string URGENCYCONTACT { get; set; }
        public string CURRENTADDRESS { get; set; }
        // public List<string> POSTS {get;set;}
        public List<V_EMPLOYEEPOSTBRIEF> EMPLOYEEPOSTS { get; set; }
        public V_UserLogin sysuser { get; set; }
    }

    
    public class V_UserLogin
    {
        /// <summary>
        /// 权限中系统用户ID
        /// </summary>
        public string SYSUSERID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 是否管理员
        /// </summary>
        public string ISMANAGER { get; set; }
        /// <summary>
        /// 登录记录ID
        /// </summary>
        public string LOGINRECORDID { get; set; }


    }
}
