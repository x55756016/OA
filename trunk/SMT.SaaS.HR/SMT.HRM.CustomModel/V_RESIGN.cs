/*
 * 文件名：V_PAYMENT.cs
 * 作  用：加扣款离职实体扩展类
 * 创建人： 喻建华
 * 创建时间：2011年3月11日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    //[注:不能添加实体属性]
    public class V_RESIGN
    {
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }
        /// <summary>
        /// 员工编号
        /// </summary>
        public string EMPLOYEECODE { get; set; }
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public string PostName { get; set; }
        public DateTime? LeftDate { get; set; }
        public string EmployeePostID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
        public string CREATEUSERID { get; set; }
    }
}
