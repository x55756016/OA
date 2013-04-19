using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    ///     用于储存可用部门id，并附上公司信息
    /// </summary>
    public class V_DEPARTMENTSWITHCOMPANY
    {
        private string departmentId;
        private string companyId;
        private string companyName;
        private string briefName;

        /// <summary>
        /// 部门ID
        /// </summary>
        public string DEPARTMENTID
        {
            get { return departmentId; }
            set { departmentId = value; }
        }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string COMPANYID
        {
            get { return companyId; }
            set { companyId = value; }
        }
        /// <summary>
        /// 公司名字
        /// </summary>
        public string COMPANYNAME
        {
            get { return companyName; }
            set { companyName = value; }
        }
        /// <summary>
        /// 公司简称
        /// </summary>
        public string BRIEFNAME
        {
            get { return briefName; }
            set { briefName = value; }
        }
    }
}
