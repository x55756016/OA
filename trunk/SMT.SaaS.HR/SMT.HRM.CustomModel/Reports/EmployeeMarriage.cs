using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel.Reports
{
    public class EmployeeMarriage
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? DataEnty { get; set; }

        /// <summary>
        /// marriage(婚否)
        /// </summary>
        public string MarriageEnty { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string EmployName { get; set; }
    }
}
