using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using SMT.HRM.UI.Views.Salary;
using SMT.HRM.UI.Form.Salary;
using SMT.HRM.UI.Views.Performance;

namespace SMT.HRM.UI
{
    public partial class UIDictionary
    {
        private static void GetUIDictionary_WD()
        {
            //薪资体系设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalarySystemSet), new List<string> { "CHECKSTATE", "SALARYLEVEL", "POSTLEVEL" }));
            //薪资项目设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CalculateItem), new List<string> { "CALCULATETYPES", "SALARYITEMTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CalculateItemForm), new List<string> { "GUERDONCATEGORY", "SALARYITEMTYPE" }));
            //员工加扣款项
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeAddSum), new List<string> { "PROTECTTYPE", "CHECKSTATE", "ASSIGNEDOBJECTTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeAddSumForm), new List<string> { "PROTECTTYPE", "ASSIGNEDOBJECTTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeAddSumMassAudit), new List<string> { "PROTECTTYPE", "CHECKSTATE", "ASSIGNEDOBJECTTYPE" }));
            //地区差异补贴
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaAllowance), new List<string> { "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaAllowanceForm), new List<string> { "POSTLEVEL" }));
            //薪资方案设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalarySolution), new List<string> { "BANKNAME", "PAYTYPE", "SALARYPRECISION", "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalarySolutionForm), new List<string> { "BANKNAME", "PAYTYPE" }));
            //薪资方案分配
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalarySolutionAssign), new List<string> { "ASSIGNEDOBJECTTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalarySolutionAssignForm), new List<string> { "ASSIGNEDOBJECTTYPE" }));
            //员工薪资发放
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Payment), new List<string> { "PAYSTATE" }));
            //绩效奖金设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PerformanceRewardSet), new List<string> { "PERFORMANCECATEGORY", "CALCULATETYPE", "CHECKSTATE" }));
            //岗位薪资设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PostLevelDistinction), new List<string> { "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PostSalaryForm), new List<string> { "POSTLEVEL" }));
            //薪资级别设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalaryLevel), new List<string> { "POSTLEVEL", "SALARYLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalaryLevelForm), new List<string> { "POSTLEVEL", "SALARYLEVEL" }));
            //自定义薪资项设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CustomSalary), new List<string> { "GUERDONCATEGORY", "CALCULATORTYPE", "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CustomSalaryForm), new List<string> { "GUERDONCATEGORY", "CALCULATORTYPE" }));
            //城市分类
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaSortForm), new List<string> { "PROVINCE", "CITY" }));
            //预算考核汇总
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PerformanceList), new List<string> { "CHECKSTATE", "SUMTYPE" }));
        }
    }
}
