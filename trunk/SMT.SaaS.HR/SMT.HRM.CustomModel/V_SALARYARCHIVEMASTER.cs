using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class V_SALARYARCHIVEMASTER
    {
        public string CompanyName { get; set; } //公司名
        public string CompanyID { get; set; } //公司ID
        public string DepartmentName { get; set; }//部门名
        public string DepartmentID { get; set; }//部门ID
        public string PostName { get; set; }//岗位名
        public string PostID { get; set; } //岗位Id
        public string standerID { get; set; } //标准ID
        public string standerName { get; set; } //标准名
        public T_HR_SALARYARCHIVE archive { get; set; } // 薪资档案
    }
}
