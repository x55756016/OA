using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EmployeeAddsumView
    {
        public string ADDSUMID { get; set; }			//加扣款ID
        public string EMPLOYEECODE { get; set; }			//员工编号
        public string EMPLOYEENAME { get; set; }			//员工姓名
        public string PROJECTNAME { get; set; }			//项目类型(1.员工加扣款2.员工代扣款)
        public string PROJECTCODE { get; set; }			//项目编码
        public decimal? PROJECTMONEY { get; set; }			//项目金额
        public string SYSTEMTYPE { get; set; }			//来源系统
        public string DEALYEAR { get; set; }			//处理年份
        public string DEALMONTH { get; set; }			//处理月份
        public string CHECKSTATE { get; set; }			//审核状态
        public string REMARK { get; set; }			   //备注
        public string OWNERID { get; set; }			  //所属员工ID
        public string OWNERPOSTID { get; set; }			//所属岗位ID
        public string OWNERDEPARTMENTID { get; set; }			//所属部门ID
        public string OWNERCOMPANYID { get; set; }			//所属公司ID
        public string CREATEPOSTID { get; set; }			//创建人岗位ID
        public string CREATEDEPARTMENTID { get; set; }			//创建人部门ID
        public string CREATECOMPANYID { get; set; }			//创建人公司ID
        public string CREATEUSERID { get; set; }			//创建人
        public DateTime? CREATEDATE { get; set; }			//创建时间
        public string UPDATEUSERID { get; set; }			//修改人
        public DateTime? UPDATEDATE { get; set; }			//修改时间
        public string EMPLOYEEID { get; set; }			//员工ID
        public string MONTHLYBATCHID { get; set; }			//月度批量结算ID
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public string PostName { get; set; }
    }
}
