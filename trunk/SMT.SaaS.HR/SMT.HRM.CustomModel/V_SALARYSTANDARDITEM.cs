/*
 * 文件名：V_SALARYSTANDARDITEM.cs
 * 作  用：薪资标准薪资项实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年5月21日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_SALARYSTANDARDITEM
    {
        /// <summary>
        /// 薪资标准薪资项ID
        /// </summary>
        public string STANDRECORDITEMID { get; set; }
        /// <summary>
        /// 薪资标准ID
        /// </summary>
        public string SALARYSTANDARDID { get; set; }
        /// <summary>
        /// 薪资项名称
        /// </summary>
        public string SALARYITEMNAME { get; set; }
        /// <summary>
        /// 薪资金额
        /// </summary>
        public string SUM { get; set; }
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
    }
}
