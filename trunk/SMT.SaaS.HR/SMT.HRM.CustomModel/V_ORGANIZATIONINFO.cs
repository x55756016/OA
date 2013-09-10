using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 组织架构视图，用于批量导入部门和岗位信息使用，字段比较简单，就不写备注了
    /// </summary>
    public class V_ORGANIZATIONINFO
    {
        public string DepartmentName { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentDictionaryID { get; set; }
        public string DepartmentCode { get; set; }

        public string PostName { get; set; }
        public string PostID { get; set; }
        public string PostDictionaryID { get; set; }
        public string PostCode { get; set; }
        public string PostNumber { get; set; }

        private string fatherType { get; set; }
        public string FatherType
        {
            get
            {
                return fatherType;
            }
            set
            {
                fatherType = value;
                if (value == "公司")
                {
                    fatherIntType = 0;
                }
                else if (value == "部门")
                {
                    fatherIntType = 1;
                }
            }
        }
        private int fatherIntType { get; set; }
        public int FatherIntType
        {
            get { return fatherIntType; }
            set { fatherIntType = value; }
        }
        public string FatherName { get; set; }
        public string FatherID { get; set; }

        public string OwnerID { get; set; }
        public string OwnerPostID { get; set; }
        public string OwnerDepartmentID { get; set; }
        public string OwnerCompanyID { get; set; }
        /// <summary>
        /// 用于存放错误信息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}
