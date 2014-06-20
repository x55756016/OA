using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_SalarySummary
    {
        public string BankID { get; set; } //银行账号
        public string BankName { get; set; }//开户行
        public string PayCompany { get; set; } //发薪机构
        public string Organization { get; set; } //行政单位
        public string CompanyName { get; set; }//公司
        public string DeptName { get; set; }//部门
        public string PostName { get; set; }//岗位
        public string EmployeeName { get; set; }//姓名
        public string IDNumber { get; set; }//身份证号
        public string PostCode { get; set; }//职级代码
        public string SalaryDate { get; set; } //发放月份
        public string ActuallyPay { get; set; } //   假期其它扣款
        public string VacationDeduct { get; set; } //   假期其它扣款
        public string BasicSalary { get; set; } // 基本工资
        public string SecurityAllowance { get; set; }   //    保密津贴
        public string HousingAllowance { get; set; }   //     住房补贴
        public string WorkingSalary { get; set; }    //    出勤工资
        public string TaxCoefficient { get; set; }       //    纳税系数
        public string Sum { get; set; }      //    计税工资
        public string AreadifAllowance { get; set; }       //    地区差异补贴
        public string TaxesBasic { get; set; }     //    扣税基数
        public string PersonalsiCost { get; set; }    //    个人社保负担
        public string PostSalary { get; set; }          //    岗位工资
        public string HousingDeduction { get; set; }     //    住房公积金扣款
        public string TaxesRate { get; set; }     //    税率
        public string FoodAllowance { get; set; }    //     餐费补贴
        public string Personalincometax { get; set; }   //   个人所得税
        public string AbsenceDays { get; set; }   //   缺勤天数
        public string OvertimeSum { get; set; }   //  加班费
        public string OtherSubjoin { get; set; }   //  其它代扣款
        public string FixIncomeSum { get; set; }    //   固定收入合计
        public string AttendanceUnusualDeduct { get; set; }    //   考勤异常扣款
        public string DutyAllowance { get; set; }    //    值班津贴
        public string OtherAddDeduction { get; set; }   //    其它加扣款
        public string PretaxSubTotal { get; set; }   //    税前应发合计
        public string SubTotal { get; set; }    //    应发小计
        public string Balance { get; set; }     //    差额
        public string CalculateDeduct { get; set; }   //   速算扣除数
        public string MantissaDeduct { get; set; }   //  尾数扣款
        public string DeductTotal { get; set; }      //  扣款合计
        public string PerformancerewardRecord { get; set; }    //  绩效奖金
        public string DeductRemark { get; set; } //扣款备注
    }

    public class GenerateUserInfo
    {
        public GenerateUserInfo()
        {
        }
        public string GenerateUserId;
        public string GenerateUserPostId;
        public string GenerateUserDepartmentId;
        public string GenerateUserCompanyId;
        public DateTime GenerateDate;
        public string GeneratePostId;
        public string GenerateDepartmentId;
        public string GenerateCompanyId;


    }

}
