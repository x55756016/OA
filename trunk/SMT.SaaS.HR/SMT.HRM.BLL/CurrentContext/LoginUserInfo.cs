using System;
using System.Net;

namespace SMT.HRM.BLL
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
        public string sexID { get; set; }
        public byte[] photo { get; set; }
        public string sysuserID { get; set; } //系统用户ID，供权限系统使用
        /// <summary>
        ///员工电话
        /// </summary>
        public string telphone { set; get; }

        public LoginUserInfo(string userID, string postID, string departmentID, string companyID,
                        string userName, string postName, string departmentName, string companyName,string telphone,byte[] photo,string sexID,string SysUserID)
        {
            this.userID = userID;
            this.postID = postID;
            this.departmentID = departmentID;
            this.companyID = companyID;
            this.userName = userName;
            this.postName = postName;
            this.departmentName = departmentName;
            this.companyName = companyName;
            this.telphone = telphone;
            this.sexID = sexID;
            this.photo = photo;
            this.sysuserID = SysUserID;
        }
    }
}
