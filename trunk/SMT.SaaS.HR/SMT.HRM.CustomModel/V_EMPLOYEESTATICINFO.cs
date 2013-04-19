using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEESTATICINFO
    {
        public string CName { get; set; }//公司名
        public string OWNERCOMPANYID { get; set; } // 公司ID
        public string DepartmentName { get; set; }//部门名
        public string OWNERDEPARTMENTID { get; set; }//部门ID
        public string PostName { get; set; }//岗位名
        public string OWNERPOSTID { get; set; }// 岗位ID
        public string EmployeeName { get; set; }//姓名
        public DateTime? EntryDate { get; set; }//入职日期
        public string Sex { get; set; }// 性别
        public string Nation { get; set; }//民族
        public string Education { get; set; }//学历
        public DateTime? BirthDay { get; set; }//生日
        public string IDNumber { get; set; }//身份证
        public int? Age { get; set; }//年龄
        public int? WorkAge { get; set; }//工龄
        public string EmployeeState { get; set; }// 员工状态
        public string PostChangeType { get; set; }//异动类型
        public string FamilyAddress { get; set; }//家庭地址
        public string OWNERID { get; set; }//所属人
        public string CREATEUSERID { get; set; }//创建人
        public string IsAgency { get; set; }//是否兼职标记
    }
}
