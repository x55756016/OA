
namespace SMT.SaaS.LocalData
{
    /// <summary>
    /// 员工岗位信息
    /// </summary>
    public class V_EMPLOYEEPOSTBRIEF
    {
        public string EMPLOYEEPOSTID { get; set; }
        public string POSTID { get; set; }
        public string PostName { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string ISAGENCY { get; set; }
        public decimal? POSTLEVEL { get; set; }
    }
}
