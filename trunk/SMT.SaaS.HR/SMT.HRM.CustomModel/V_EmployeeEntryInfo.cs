using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工导入导出，备注写后面了就
    /// </summary>
    public class V_EmployeeEntryInfo
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string IdNumber { get; set; }//身份证号
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
        }//性别
        private int sexDic { get; set; }
        public int SexDic
        {
            get { return sexDic; }
            set { sexDic = value; }
        }
        public string EmployeeCode { get; set; }//员工编号
        public string Birthday { get; set; }//出生年月
        public DateTime? BirthdayDate { get; set; }//出生年月,时间类型
        public string CompanyName { get; set; }//公司名称
        public string CompamyID { get; set; }
        public string DepartmentName { get; set; }//部门名称
        public string DepartmentID { get; set; }
        public string PostName { get; set; }//岗位名称
        public string PostID { get; set; }
        public string PostLevel { get; set; }
        public string EntryDate { get; set; }//入职时间
        public DateTime? EntryDates { get; set; }//入职时间
        public string OnPostDate { get; set; }//到岗时间
        public DateTime? OnPostDates { get; set; }//到岗时间
        public string SocialServiceYear { get; set; }//社保缴交记录
        public string Height { get; set; }//身高
        public string FingerPrintID { get; set; }//指纹编号
        public string Bank { get; set; }//银行
        public string BankCardNumber { get; set; }//银行账号
        public string RegResidence { get; set; }//户口所在地
        public string FamilyAddress { get; set; }//家庭地址
        public string CurrentAddress { get; set; }//目前居住地
        public string Mobile { get; set; }//手机（电话）号码
        public string OfficePhone { get; set; }//办公电话
        public string Nation { get; set; }//民族
        public string Education { get; set; }//学历
        public string WorkAge { get; set; }//服务时间（单位：月）
        public string Age { get; set; }//年龄
        public string Marriage { get; set; }//婚姻状况
        public DateTime? CheckDate { get; set; }//转正时间
        public string Specialty { get; set; }//所学专业
        public string GraduateSchool { get; set; }//毕业院校
        public string UrgencyPerson { get; set; }//紧急联系人
        public string UrgencyContact { get; set; }//紧急联系方法
        public string PensionComputerNo { get; set; }//社保电脑号
        public string PensionCardID { get; set; }//社保卡号
        public string PensionCheckState { get; set; }//社保档案审核状态
        public string EmployeeState { get; set; }//员工状态
        public string Remark { get; set; }//员工档案备注

        public string UserName { get; set; }//用户名
        public string PassWord { get; set; }//密码
        public string Email { get; set; }//邮箱

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
