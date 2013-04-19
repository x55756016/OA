/*
 * 文件名：V_RETURNFBI.cs
 * 作  用：薪资统计预算相关实体扩展类
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
    public class V_RETURNFBI
    {
        /// <summary>
        /// 公司ID
        /// </summary>
        public string COMPANYID { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string COMPANYNAME { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string DEPARTMENTID { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DEPARTMENTNAME { get; set; }
        /// <summary>
        /// 薪资统计值
        /// </summary>
        public decimal? SALARYSUM { get; set; }
    }
}
