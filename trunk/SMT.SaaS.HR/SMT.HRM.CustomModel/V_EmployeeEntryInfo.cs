using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工入职信息，用于批量导入，字段比较简单，就不备注了
    /// </summary>
    public class V_EmployeeEntryInfo
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string IdNumber { get; set; }
        private string sex { get; set; }
        public string Sex
        {
            get
            {
                return sex;
            }
            set
            {
                sex = value;
                if (value == "男")
                {
                    sexDic = 1;
                }
                else if (value == "女")
                {
                    sexDic = 0;
                }
            }
        }
        private int sexDic { get; set; }
        public int SexDic
        {
            get { return sexDic; }
            set { sexDic = value; }
        }
        public string Birthday { get; set; }
        public string CompanyName { get; set; }
        public string CompamyID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentID { get; set; }
        public string PostName { get; set; }
        public string PostID { get; set; }
        public string PostLevel { get; set; }
        public string EntryDate { get; set; }//入职时间
        public string OnPostDate { get; set; }//到岗时间
        public string SocialServiceYear { get; set; }//社保缴交记录
        public string Height { get; set; }
        public string FingerPrintID { get; set; }
        public string Bank { get; set; }
        public string BankCardNumber { get; set; }
        public string RegResidence { get; set; }//户口所在地
        public string FamilyAddress { get; set; }
        public string CurrentAddress { get; set; }//目前居住地
        public string Mobile { get; set; }

        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Email { get; set; }

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
