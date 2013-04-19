using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_MonthDeductionTax
    {
        public string PayCompany { get; set; } //发薪机构	
        public string Organization { get; set; } //行政单位	
        public string IdNumber { get; set; } //身份证号	
        public string EmployeeName { get; set; } //姓名	
        public string Sum { get; set; } //计税合计
        public string Balance { get; set; } //差额	
        public string TaxesBasic { get; set; } //扣税基数	
        public string TaxesRate { get; set; } //税率	
        public string CalculateDeduct { get; set; } //速算扣除数	
        public string Personalincometax { get; set; } //个人所得税

    }
}
