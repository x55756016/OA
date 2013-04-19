using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工汇总表
    /// </summary>
    public class V_EmployeeCollect
    {
        /// <summary>
        /// 组织ID
        /// </summary>
        public string ORANIZATIONID { get; set; }
        /// <summary>
        /// 组织名称 可以为公司、部门
        /// </summary>
        public string ORGANIZATIONNAME { get; set; }
        /// <summary>
        /// 前台人数
        /// </summary>
        public decimal FRONTNUMERS { get; set; }
        /// <summary>
        /// 后台人数
        /// </summary>
        public decimal BACKNUMERS { get; set; }
        /// <summary>
        /// 合计
        /// </summary>
        public decimal TOTALNUMERS { get; set; }
        /// <summary>
        /// 是否顶级
        /// </summary>
        public string ISTOP { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CREATEDATE { get; set; }
    }
    /// <summary>
    /// 员工数据明细
    /// </summary>
    public class V_EMPLOYEEDATADETAIL
    {
        /// <summary>
        /// 类型 离职、在职
        /// </summary>
        public string COLLECTTYPE { get; set; }
        public List<V_EmployeeCollect> LISTEMPLOYEESCOUNT { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CREATEDATE { get; set; }

        
    }

    
}
