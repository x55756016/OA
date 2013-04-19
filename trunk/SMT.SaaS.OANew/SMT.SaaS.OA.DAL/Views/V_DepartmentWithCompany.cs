using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.OA.DAL.Views
{
    /// <summary>
    /// 用于获取某公司的部门id，并附上公司id、公司名字和公司简称-by luojie
    /// </summary>
    public class V_DepartmentWithCompany
    {
        private string departmentId;
        private string companyId;
        private string companyName;
        private string companyShortName;
        private string approvalTypeValue;

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
            get { return companyShortName; }
            set { companyShortName = value; }
        }
        /// <summary>
        /// 事项审批类型
        /// </summary>
        public string APPROVALTYPEVALUE
        {
            get { return approvalTypeValue; }
            set { approvalTypeValue = value; }
        }
    }
}
