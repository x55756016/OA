using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.SaaS.Permission.Services
{
    public class LoginUserInfo
    {
        public string userID { get; set; }                   //用户ID
        public string postID { get; set; }                   //岗位ID
        public string departmentID { get; set; }             //部门ID
        public string companyID { get; set; }                //公司ID
        public string userName { get; set; }                 //用户姓名
        public string postName { get; set; }                 //岗位名称
        public string departmentName { get; set; }           //部门名称
        public string companyName { get; set; }              //公司名称
    }
}
