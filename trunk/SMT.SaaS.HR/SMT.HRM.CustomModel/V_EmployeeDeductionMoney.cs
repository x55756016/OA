using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EmployeeDeductionMoney
    {
        public string PayCompany { get; set; } //发薪机构	
        public string Organization { get; set; } //行政单位	
        public string IdNumber { get; set; } //身份证号	
        public string EmployeeName { get; set; } //姓名	
        public string DeductionBorrow { get; set; } //扣借款
        public string OtherAddDeduction { get; set; } //其它加扣款	
        public string OtherSubjoin { get; set; } //其它代扣款	
        public string MantissaDeduct { get; set; } //尾数扣款	
        public string Remark { get; set; } // 备注
    }
}
