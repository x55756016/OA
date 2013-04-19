/*
 * 文件名：V_SALARYARCHIVES.cs
 * 作  用：薪资档案和档案历史实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年9月28日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_SALARYARCHIVES
    {
        /// <summary>
        /// 薪资档案ID
        /// </summary>
        public string SALARYARCHIVEID { get; set; }

        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }

        /// <summary>
        /// 员工编号
        /// </summary>
        public string EMPLOYEECODE { get; set; }

        /// <summary>
        /// 薪资等级
        /// </summary>
        public string SALARYLEVEL { get; set; }

        /// <summary>
        /// 岗位等级
        /// </summary>
        public string POSTLEVEL { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATEDATE { get; set; }
    }
}
