using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_SALARYSOLUTIONITEM
    {
        /// <summary>
        /// 薪资方案薪资项ID
        /// </summary>
        public string SOLUTIONITEMID { get; set; }
        /// <summary>
        /// 薪资方案ID
        /// </summary>
        public string SALARYSOLUTIONID { get; set; }
        /// <summary>
        /// 薪资项名称
        /// </summary>
        public string SALARYITEMNAME { get; set; }
        /// <summary>
        /// 薪资金额
        /// </summary>
        public decimal? SUM { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        //public DateTime CREATEDATE { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public decimal? ORDERNUMBER { get; set; }
        /// <summary>
        ///必选项标志 （1 必选）
        /// </summary>
        public string MUSTSELECTED { get; set; }
    }
}
