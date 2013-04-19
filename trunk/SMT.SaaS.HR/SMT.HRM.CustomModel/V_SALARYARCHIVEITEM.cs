using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class V_SALARYARCHIVEITEM
    {
        /// <summary>
        /// 薪资项名
        /// </summary>
        public string SALARYITEMNAME { get; set; }
        /// <summary>
        /// 薪资档案项ID
        /// </summary>
        public string SALARYARCHIVEITEM { get; set; }
        /// <summary>
        /// 薪资档案ID
        /// </summary>
        public string SALARYARCHIVEID { get; set; }
        /// <summary>
        /// 薪资标准ID
        /// </summary>
        public string SALARYSTANDARDID { get; set; }
        /// <summary>
        /// 薪资项ID
        /// </summary>
        public string SALARYITEMID { get; set; }
        /// <summary>
        /// 计算公式
        /// </summary>
        public string CALCULATEFORMULA { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string SUM { get; set; }
        /// <summary>
        /// 计算公式编码
        /// </summary>
        public string CALCULATEFORMULACODE { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public decimal? ORDERNUMBER { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        public string REMARK { get; set; }
       
    }
}
