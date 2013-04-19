using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_LandDetail
    {
        /// <summary>
        /// 所属人ID。
        /// </summary>
        public string OwnerID { get; set; }
        /// <summary>
        /// 所属人姓名。
        /// </summary>
        public string OwnerName { get; set; }
        /// <summary>
        /// 所属人ID。
        /// </summary>
        public string OwnerPostID { get; set; }
        /// <summary>
        /// 所属人姓名。
        /// </summary>
        public string OwnerPostName { get; set; }
        /// <summary>
        /// 所属部门ID。
        /// </summary>
        public string OwnerDepartmentID { get; set; }
        /// <summary>
        /// 所属部门名称。
        /// </summary>
        public string OwnerDepartmentName { get; set; }
        /// <summary>
        /// 所属公司ID。
        /// </summary>
        public string OwnerCompanyID { get; set; }
        /// <summary>
        /// 所属公司名称。
        /// </summary>
        public string OwnerCompanyName { get; set; }
        /// <summary>
        /// 登录IP。
        /// </summary>
        public string LOGINIP { get; set; }
        /// <summary>
        /// 登录时间。
        /// </summary>
        public DateTime LoginDate { get; set; }
    }
}
