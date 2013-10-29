
namespace SMT.SaaS.LocalData.Tables
{
    /// <summary>
    /// 本地公司信息实体
    /// </summary>
    public class V_CompanyInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID { get; set; }

        /// <summary>
        /// 员工ID
        /// </summary>
        public string UserID { get; set; }
        
        public string BRIEFNAME { get; set; }
        
        public string CHECKSTATE { get; set; }
        
        public string CNAME { get; set; }
        
        public string COMPANRYCODE { get; set; }
        
        public string COMPANYID { get; set; }
        
        public string COMPANYTYPE { get; set; }
        
        public string EDITSTATE { get; set; }
        
        public string ENAME { get; set; }
        
        public string FATHERCOMPANYID { get; set; }
        
        public string FATHERID { get; set; }
        
        public string FATHERTYPE { get; set; }
        
        public decimal SORTINDEX { get; set; }
    }
}
