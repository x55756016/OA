using System.Collections.Generic;
namespace SMT.SaaS.LocalData
{
    /// <summary>
    /// 记录当前登录的用户信息
    /// </summary>
    public class LoginUserInfo
    {
        /// <summary>
        /// 用户系统ID
        /// </summary>
        public string SysUserID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 员工名称
        /// </summary>
        public string EmployeeName { get; set; }
        /// <summary>
        /// 员工状态
        /// </summary>
        public string EmployeeState { get; set; }
        /// <summary>
        /// 员工编号
        /// </summary>
        public string EmployeeCode { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string SexID { get; set; }
        /// <summary>
        /// 照片
        /// </summary>
        public byte[] Photo { get; set; }
       
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsManager { get; set; }
        /// <summary>
        /// 员工工龄,单位：月
        /// </summary>
        public decimal? WorkingAge { get; set; }
        /// <summary>
        /// 员工办公电话
        /// </summary>
        public string OfficeTelphone { set; get; }
        /// <summary>
        /// 员工私人手机号码
        /// </summary>
        public string MobileTelphone { set; get; }
        /// <summary>
        /// 当前登录记录ID
        /// </summary>
        public string LoginRecordID { get; set; }
        /// <summary>
        /// 多岗位信息，若用户有兼职的情况
        /// </summary>
        public List<UserPost> UserPosts { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>        
        public string UserName { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string UserPwd { get; set; }
        /// <summary>
        /// 员工系统权限信息，按需加载
        /// </summary>
        public List<V_UserPermissionUI> PermissionInfoUI { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LoginUserInfo() 
        {
            if (UserPosts == null)
            {
                UserPosts = new List<UserPost>();
            }
        }
    }
}
