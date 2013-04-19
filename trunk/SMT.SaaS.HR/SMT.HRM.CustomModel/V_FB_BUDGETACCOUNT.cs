/*
 * 文件名：V_FB_BUDGETACCOUNT.cs
 * 作  用：月度预算实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年4月8日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_FB_BUDGETACCOUNT
    {
        /// <summary>
        /// 当前可用额度
        /// </summary>
        public decimal? USABLEMONEY { get; set; }

        /// <summary>
        /// 当前月预算额度
        /// </summary>
        public decimal? BUDGETMONEY { get; set; }
        /// <summary>
        /// 当前公司
        /// </summary>
        public string OWNERCOMPANYID { get; set; }
        /// <summary>
        /// 当前部门
        /// </summary>
        public decimal? OWNERDEPARTMENTID { get; set; }
        /// <summary>
        /// 当前月份
        /// </summary>
        public string BUDGETMONTH { get; set; }
        /// <summary>
        /// 当前年份
        /// </summary>
        public string BUDGETYEAR { get; set; }

    }
}
