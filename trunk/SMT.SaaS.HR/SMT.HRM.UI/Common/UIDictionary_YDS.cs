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
using SMT.HRM.UI.Form.Salary;
using SMT.HRM.UI.Views.Performance;
using SMT.HRM.UI.Views.Salary;
using SMT.HRM.UI.Views.Attendance;
using SMT.HRM.UI.Form.Attendance;

namespace SMT.HRM.UI
{
    public partial class UIDictionary
    {
        private static void GetUIDictionary_YDS()
        {
            //自己写分配到自己的页面的字典就好.此处是一个Add的例子.
            //出差申请
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalaryArchiveForm), new List<string> { "POSTLEVEL", "SALARYLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(KPIPointSet), new List<string> { "Flag" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalaryArchive), new List<string> { "CHECKSTATE", "POSTLEVEL", "SALARYLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ComplainFlow), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(KPIDetails), new List<string> { "KPICOMPLAIN" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttSolRd), new List<string> { "ATTENDANCETYPE","CARDTYPE","WORKDAYTYPE","OFFESTTYPE","EDITSTATE","CHECKSTATE" }));            
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(NoAttendCardEmployees), new List<string> { "ASSIGNEDOBJECTTYPE","CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendMonthlyBalance), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeSalaryRecord), new List<string> { "CHECKSTATE", "ASSIGNEDOBJECTTYPE", "SALARYAUDITTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalaryStandard), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SalaryRecordMassAudit), new List<string> { "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaSort), new List<string> { "CITY" }));

            //考勤方案定义
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttSolRdLeaveSet), new List<string> { "LEAVETYPEVALUE", "LEAVEFINETYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttSolRdDeductSet), new List<string> { "ATTENDABNORMALTYPE", "ATTEXFINETYPE", "ISCHECKED" }));
        }
    }
}
