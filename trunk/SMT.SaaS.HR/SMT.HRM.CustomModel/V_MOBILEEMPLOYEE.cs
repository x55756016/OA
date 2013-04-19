using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 手机端通讯录模型
    /// </summary>
    public class V_MOBILEEMPLOYEE
    {
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// 员工姓名拼音
        /// </summary>
        public string EmployeeName { get; set; }
        /// <summary>
        /// 员工中文名
        /// </summary>
        public string EmployeeCNName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 照片
        /// </summary>
        public byte[] Photo { get; set; }
        /// <summary>
        /// 公司邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 目前居住地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 办公电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 紧急联系人
        /// </summary>
        public string UrgencyPerson { get; set; }
        /// <summary>
        /// 紧急联系电话
        /// </summary>
        public string UrgencyContact { get; set; }
        /// <summary>
        /// 所属公司ID
        /// </summary>
        public string OwnerCompanyID { get; set; }
        /// <summary>
        /// 所属公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司简称
        /// </summary>
        public string BriefName { get; set; }
        /// <summary>
        /// 所属部门ID
        /// </summary>
        public string OwnerDepartMentID { get; set; }
        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartMentName { get; set; }
        /// <summary>
        /// 所属岗位ID
        /// </summary>
        public string OwnerPostID { get; set; }
        /// <summary>
        /// 所属岗位名称
        /// </summary>
        public string PostName { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserID { get; set; }
        /// <summary>
        /// 所属人
        /// </summary>
        public string OwnerID { get; set; }
    }
}
