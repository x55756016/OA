using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel.Reports
{
    public class PieEmployeece
    {
        /// <summary>
        /// 婚姻状态
        /// </summary>
        public string marriage { get; set; }

        /// <summary>
        /// 人数
        /// </summary>
        public int CountEmployeece { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string EMPLOYEEID { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public string Age { get; set; }

        /// <summary>
        /// 入职日期
        /// </summary>
        public DateTime ENTRYDATE{ get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public string  OWNERCOMPANYID { get; set; }


        /// <summary>
        /// 公司英文名
        /// </summary>
        public string ENAME { get; set; }

        /// <summary>
        /// 公司中文名
        /// </summary>
        public string CNAME { get; set; }
    }
}
