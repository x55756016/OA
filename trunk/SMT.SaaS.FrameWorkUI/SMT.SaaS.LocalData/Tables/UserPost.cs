
namespace SMT.SaaS.LocalData
{
    /// <summary>
    /// 用户岗位信息
    /// </summary>
    public class UserPost
    {
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 员工岗位关联ID
        /// </summary>
        public string EmployeePostID { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public string PostID { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string PostName { get; set; }
        /// <summary>
        /// 岗位等级
        /// </summary>
        public decimal? PostLevel { get; set; }
        /// <summary>
        /// 标识是否为主岗位。True：主岗位,非代理岗位 True : 代理岗位
        /// </summary>
        public bool IsAgency { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string DepartmentID { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string CompanyID { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
    }
}
