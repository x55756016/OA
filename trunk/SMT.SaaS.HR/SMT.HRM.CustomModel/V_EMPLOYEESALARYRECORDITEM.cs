/*
 * 文件名：V_EMPLOYEESALARYRECORDITEM
 * 作  用：薪资记录薪资项实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年5月31日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEESALARYRECORDITEM
    {
        /// <summary>
        /// 薪资记录薪资项ID
        /// </summary>
        public string SALARYRECORDITEMID { get; set; }
        /// <summary>
        /// 薪资项ID 
        /// </summary>
        public string SALARYITEMID { get; set; }
        /// <summary>
        /// 薪资项名称 
        /// </summary>
        public string SALARYITEMNAME { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string SUM { get; set; }
        /// <summary>
        /// 计算公式
        /// </summary>
        public string CALCULATEFORMULA { get; set; }
        /// <summary>
        /// 薪资项排序号
        /// </summary>
        public decimal? ORDERNUMBER { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATEDATE { get; set; }

        /// <summary>
        /// 薪资项代码
        /// </summary>
        public string SALARYITEMCODE { get; set; }
    }
}
